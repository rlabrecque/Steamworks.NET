import os
import sys
from copy import deepcopy
from SteamworksParser.steamworksparser import BlankLine, FieldOffset, Parser, Settings, Struct, StructField

g_TypeConversionDict = {
    "uint8": "byte",
    "uint16": "ushort",
    "uint32": "uint",
    "uint64": "ulong",
    "uint8_t": "byte",
    "uint16_t": "ushort",
    "uint32_t": "uint",
    "uint64_t": "ulong",

    "char": "string",
    "int32": "int",
    "int64": "long",
    "int32_t": "int",
    "int64_t": "long",

    "uint8 *": "IntPtr",
    "const char *": "string",
    "const char **": "IntPtr",

    # This is for CallbackMsg_t
    "HSteamUser": "int"
}

g_CustomPackSize = {
    # Callbacks
    "AvatarImageLoaded_t": "4",
    "FriendRichPresenceUpdate_t": "4",
    "GameConnectedClanChatMsg_t": "4",
    "GameConnectedChatLeave_t": "1",
    "JoinClanChatRoomCompletionResult_t": "4",
    "GameConnectedFriendChatMsg_t": "4",
    "FriendsGetFollowerCount_t": "4",
    "FriendsIsFollowing_t": "4",
    "FriendsEnumerateFollowingList_t": "4",
    "GSClientDeny_t": "4",
    "GSClientKick_t": "4",
    "GSClientGroupStatus_t": "1",
    "GSStatsReceived_t": "4",
    "GSStatsStored_t": "4",
    "P2PSessionConnectFail_t": "1",
    "SocketStatusCallback_t": "4",
    "ValidateAuthTicketResponse_t": "4",

    # Structs
    "InputAnalogActionData_t": "1",
    "InputDigitalActionData_t": "1",
}

g_SkippedStructs = (
    # We remap these ISteamController structs to ISteamInput
    "ControllerAnalogActionData_t",
    "ControllerDigitalActionData_t",
    "ControllerMotionData_t",

    # String formatting functions. We just use .ToString() instead.
    "SteamNetworkingIdentityRender",
    "SteamNetworkingIPAddrRender",
    "SteamNetworkingPOPIDRender",

    # Custom Types
    "SteamIPAddress_t",
    "SteamInputActionEvent_t",
    "RemotePlayInput_t",
)

g_SequentialStructs = (
    "MatchMakingKeyValuePair_t",
    "SteamNetConnectionInfo_t"
)

g_SpecialFieldTypes = {
    "PersonaStateChange_t": {
        "m_nChangeFlags": "EPersonaChange"
    },

    "HTML_NeedsPaint_t": {
        "pBGRA": "IntPtr"
    },

    # These two are returned by a function and the struct needs to be blittable.
    "InputAnalogActionData_t": {
        "bActive": "byte", # Originally bool
    },

    "InputDigitalActionData_t": {
        "bState":  "byte", # Originally bool
        "bActive": "byte", # Originally bool
    },
}

g_ExplicitStructs = {
    "UserStatsReceived_t": {
        "m_nGameID" : "0",
        "m_eResult" : "8",
        "m_steamIDUser" : "12",
    }
}

def main(parser: Parser):
    try:
        os.makedirs("../com.rlabrecque.steamworks.net/Runtime/autogen/")
    except OSError:
        pass

    packsizeAwareStructNames = parser.packSizeAwareStructs

    lines = []
    callbacklines = []
    
    anyCpuConditionalMarshallerLines = [] # Contains conditional marshaller code only

    for f in parser.files:
        for struct in f.structs:
            lines.extend(parse(struct, True, anyCpuConditionalMarshallerLines, packsizeAwareStructNames, parser))
        for callback in f.callbacks:
            callbacklines.extend(parse(callback, True, anyCpuConditionalMarshallerLines, packsizeAwareStructNames, parser))

    with open("../com.rlabrecque.steamworks.net/Runtime/autogen/SteamStructs.cs", "wb") as out:
        with open("templates/header.txt", "r") as f:
            out.write(bytes(f.read(), "utf-8"))
        out.write(bytes("namespace Steamworks {\n", "utf-8"))
        for line in lines:
            out.write(bytes(line + "\n", "utf-8"))
        out.write(bytes("}\n\n", "utf-8"))
        out.write(bytes("#endif // !DISABLESTEAMWORKS\n", "utf-8"))

    with open("../com.rlabrecque.steamworks.net/Runtime/autogen/SteamCallbacks.cs", "wb") as out:
        with open("templates/header.txt", "r") as f:
            out.write(bytes(f.read(), "utf-8"))
        out.write(bytes("namespace Steamworks {\n", "utf-8"))
        for line in callbacklines:
            out.write(bytes(line + "\n", "utf-8"))
        out.write(bytes("}\n\n", "utf-8"))
        out.write(bytes("#endif // !DISABLESTEAMWORKS\n", "utf-8"))
    
    with open("../Standalone3.0/ConditionalMarshallerTable.g.cs", "wb") as out:
        with open("templates/header.txt", "r") as f:
            out.write(bytes(f.read(), "utf-8"))
        
        with open("templates/anycpu/ConditionalMarshallerTable.head.cs", "r") as f:
            out.write(bytes(f.read(), "utf-8"))
        
        for line in anyCpuConditionalMarshallerLines:
            out.write(bytes("\t\t\t" + line + "\n", "utf-8"))
        
        with open("templates/anycpu/ConditionalMarshallerTable.tail.cs", "r") as f:
            out.write(bytes(f.read(), "utf-8"))
        
        out.write(bytes("#endif // !DISABLESTEAMWORKS\n", "utf-8"))

def parse(struct: Struct, isMainStruct, marshalTableLines: list[str], packsizeAwareStructNames: list[str], parser: Parser) -> list[str]:
    # ignore structs that manually defined by us
    # ignore nested structs, they probably handled by hand
    # ignore structs which has nested types, they probably interop by hand
    if struct.name in g_SkippedStructs or struct.should_not_generate():
        return []

    if struct.is_sequential and not isMainStruct:
        return []

    lines = []
    for comment in struct.c.rawprecomments:
        if type(comment) is BlankLine:
            continue
        lines.append("\t" + comment)

    structname: str = struct.name
    # We have analyzed this struct and stored the value of its packsize,
    # it's stored in Struct.pack, None for default packsize
    # If the struct is packsize-aware, we generate large variants of it.
    packsize = g_CustomPackSize.get(structname, "Packsize.value")
    isExplicitStruct = False
    if g_ExplicitStructs.get(structname, False):
        lines.append("\t[StructLayout(LayoutKind.Explicit, Pack = " + packsize + ")]")
        isExplicitStruct = True
    elif isMainStruct and not struct.is_sequential:
         
        if struct.packsize != "Packsize.value" and structname not in g_SequentialStructs:
            customsize = ""
            if len(struct.fields) == 0:
                customsize = ", Size = 1"
            lines.append("\t[StructLayout(LayoutKind.Sequential, Pack = " + packsize + customsize + ")]")
    elif not isMainStruct:
        packsize = str(8)
        customsize = ""
        if len(struct.fields) == 0:
                customsize = ", Size = 1"
        lines.append("\t[StructLayout(LayoutKind.Sequential, Pack = 8" + customsize + ")]")

    if struct.callbackid:
        lines.append("\t[CallbackIdentity(Constants." + struct.callbackid + ")]")

    # use pack-size sematic for sequential
    for name in g_SequentialStructs or struct.is_sequential:
        if name == structname or struct.is_sequential:
            lines.append("\t[StructLayout(LayoutKind.Sequential)]")
            break

    if isMainStruct:
        if struct.callbackid:
            lines.append("\tpublic struct " + structname )
            lines.append("\t#if STEAMWORKS_ANYCPU")
            lines.append("\t\t: ICallbackIdentity")
            lines.append("\t#endif")
            lines.append("\t{")
        else:
            lines.append("\tpublic struct " + structname + " {" )
    else:
        lines.append("\tinternal struct " + structname + " {")
        

    lines.extend(insert_constructors(structname))

    if struct.callbackid and isMainStruct:
        lines.append("\t\tpublic const int k_iCallback = Constants." + struct.callbackid + ";")
        lines.append("\t\tpublic static int CallbackIdentity { get; } = Constants." + struct.callbackid + ";")

    fieldHandlingStructName = structname
    for field in struct.fields:
        if not isMainStruct:
            fieldHandlingStructName = fieldHandlingStructName[:structname.rindex("_")]

        lines.extend(parse_field(field, fieldHandlingStructName, isMainStruct, parser))
        
    if fieldHandlingStructName in packsizeAwareStructNames and not isMainStruct:
        mainStructName = structname[:structname.rindex("_")]
        packKind = structname[structname.rindex("_") + 1:]

        lines.append("")
        lines.append(f"\t\tpublic static implicit operator {mainStructName}({mainStructName}_{packKind} value) {{")
        lines.append(f"\t\t\t{mainStructName} result = default;")
        
        for field in struct.fields:
            gen_fieldcopycode(field, structname, lines)
        
        lines.append(f"\t\t\treturn result;")
        lines.append("\t\t}")

        lines.append("")
        lines.append(f"\t\tpublic static implicit operator {mainStructName}_{packKind}({mainStructName} value) {{")
        lines.append(f"\t\t\t{mainStructName}_{packKind} result = default;")
        
        for field in struct.fields:
            gen_fieldcopycode(field, structname, lines)
        
        lines.append(f"\t\t\treturn result;")
        lines.append("\t\t}")
        pass

    if struct.endcomments:
        for comment in struct.endcomments.rawprecomments:
            if type(comment) is BlankLine:
                lines.append("\t\t")
            else:
                lines.append("\t" + comment)

    # Generate Any CPU marshal helper
    if isMainStruct and struct.name in packsizeAwareStructNames and not isExplicitStruct:
        marshalTableLines.append(f"if (typeof(T) == typeof({structname}) && Packsize.IsLargePack)")
        marshalTableLines.append(f"\tImpl<{structname}>.Marshaller = (unmanaged) =>")
        marshalTableLines.append(f"\t\tSystem.Runtime.InteropServices.Marshal.PtrToStructure<{structname}_LargePack>(unmanaged);")
        marshalTableLines.append("")
        

    #     pass
    
    lines.append("\t}")
    lines.append("")

    # Generate Any CPU struct variant for default pack-sized structs
    if isMainStruct and not isExplicitStruct and struct.name in packsizeAwareStructNames:
        lines.append("\t#if STEAMWORKS_ANYCPU")
        
        largePackStruct = deepcopy(struct)
        largePackStruct.name = structname + "_LargePack"
        largePackStruct.packsize = 8
        lines.extend(parse(largePackStruct, False, marshalTableLines, packsizeAwareStructNames, parser))
        
        lines.append("\t#endif")

    return lines

def gen_fieldcopycode(field, structname, marshalTableLines):
    fieldtype = g_TypeConversionDict.get(field.type, field.type)
    fieldtype = g_SpecialFieldTypes.get(structname, dict()).get(field.name, fieldtype)

    if field.arraysize and fieldtype == "string":
        marshalTableLines.append(f"\t\t\tresult.{field.name}_ = value.{field.name}_;")
    else:
        marshalTableLines.append(f"\t\t\tresult.{field.name} = value.{field.name};")
                
def parse_field(field: StructField, structname: str, isMainStruct: bool, parser: Parser):
    lines = []
    for comment in field.c.rawprecomments:
        if type(comment) is BlankLine:
            lines.append("\t\t")
        else:
            lines.append("\t" + comment)

    fieldtype = g_TypeConversionDict.get(field.type, field.type)
    fieldtype = g_SpecialFieldTypes.get(structname, dict()).get(field.name, fieldtype)

    explicit = g_ExplicitStructs.get(structname, False)
    if explicit:
        lines.append("\t\t[FieldOffset(" + explicit[field.name] + ")]")

    comment = ""
    if field.c.rawlinecomment:
        comment = field.c.rawlinecomment

    if field.arraysizeStr:
        constantsstr = ""
        if not field.arraysizeStr.isdigit():
            constantsstr = "Constants."

        if fieldtype == "byte[]":
            lines.append("\t\t[MarshalAs(UnmanagedType.ByValArray, SizeConst = " + constantsstr + field.arraysizeStr + ")]")
        if structname == "MatchMakingKeyValuePair_t":
            lines.append("\t\t[MarshalAs(UnmanagedType.ByValTStr, SizeConst = " + constantsstr + field.arraysizeStr + ")]")
        else:
            lines.append("\t\t[MarshalAs(UnmanagedType.ByValArray, SizeConst = " + constantsstr + field.arraysizeStr + ")]")
            fieldtype += "[]"

    if fieldtype == "bool":
        lines.append("\t\t[MarshalAs(UnmanagedType.I1)]")

	# HACK real type is `string`, `[]` is added by `fieldtype += "[]"`
    if field.arraysizeStr and fieldtype == "string[]":
        lines.append("\t\tinternal byte[] " + field.name + "_;")
        lines.append("\t\tpublic string " + field.name + comment)	
        lines.append("\t\t{")
        lines.append("\t\t\tget { return InteropHelp.ByteArrayToStringUTF8(" + field.name + "_); }")
        lines.append("\t\t\tset { InteropHelp.StringToByteArrayUTF8(value, " + field.name + "_, " + constantsstr + field.arraysizeStr + "); }")
        lines.append("\t\t}")
    else:
        if not isMainStruct:
           typeInfo = parser.resolveTypeInfo(fieldtype)
           if isinstance(typeInfo, Struct) and typeInfo.packsize_aware:
               fieldtype = fieldtype + "_LargePack"

        lines.append("\t\tpublic " + fieldtype + " " + field.name + ";" + comment)

    return lines

def insert_constructors(name):
    lines = []
    if name == "MatchMakingKeyValuePair_t":
        lines.append("\t\tMatchMakingKeyValuePair_t(string strKey, string strValue) {")
        lines.append("\t\t\tm_szKey = strKey;")
        lines.append("\t\t\tm_szValue = strValue;")
        lines.append("\t\t}")
        lines.append("")

    return lines

if __name__ == "__main__":
    if len(sys.argv) != 2:
        print("TODO: Usage Instructions")
        exit()

    Settings.fake_gameserver_interfaces = True
    main(parse(sys.argv[1]))