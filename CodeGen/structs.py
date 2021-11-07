import os
import sys
from SteamworksParser import steamworksparser

g_TypeConversionDict = {
    "uint8": "byte",
    "uint16": "ushort",
    "uint32": "uint",
    "uint64": "ulong",

    "char": "string",
    "int32": "int",
    "int64": "long",

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
    # Lingering PS3 stuff.
    "PSNGameBootInviteResult_t",
    "PS3TrophiesInstalled_t",

    # We remap these ISteamController structs to ISteamInput
    "ControllerAnalogActionData_t",
    "ControllerDigitalActionData_t",
    "ControllerMotionData_t",

    # String formatting functions. We just use .ToString() instead.
    "SteamNetworkingIdentityRender",
    "SteamNetworkingIPAddrRender",
    "SteamNetworkingPOPIDRender",

    # CustomType
    "SteamIPAddress_t",
    "SteamInputActionEvent_t",
)

g_SequentialStructs = (
    "MatchMakingKeyValuePair_t",
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

def main(parser):
    try:
        os.makedirs("../com.rlabrecque.steamworks.net/Runtime/autogen/")
    except OSError:
        pass

    lines = []
    callbacklines = []
    for f in parser.files:
        for struct in f.structs:
            lines.extend(parse(struct))
        for callback in f.callbacks:
            callbacklines.extend(parse(callback))

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

def parse(struct):
    if struct.name in g_SkippedStructs:
        return []

    lines = []
    for comment in struct.c.rawprecomments:
        if type(comment) is steamworksparser.BlankLine:
            continue
        lines.append("\t" + comment)

    structname = struct.name

    packsize = g_CustomPackSize.get(structname, "Packsize.value")
    if g_ExplicitStructs.get(structname, False):
        lines.append("\t[StructLayout(LayoutKind.Explicit, Pack = " + packsize + ")]")
    elif struct.packsize:
        customsize = ""
        if len(struct.fields) == 0:
            customsize = ", Size = 1"
        lines.append("\t[StructLayout(LayoutKind.Sequential, Pack = " + packsize + customsize + ")]")

    if struct.callbackid:
        lines.append("\t[CallbackIdentity(Constants." + struct.callbackid + ")]")

    for name in g_SequentialStructs:
        if name == structname:
            lines.append("\t[StructLayout(LayoutKind.Sequential)]")
            break

    lines.append("\tpublic struct " + structname + " {")

    lines.extend(insert_constructors(structname))

    if struct.callbackid:
        lines.append("\t\tpublic const int k_iCallback = Constants." + struct.callbackid + ";")

    for field in struct.fields:
        lines.extend(parse_field(field, structname))

    if struct.endcomments:
        for comment in struct.endcomments.rawprecomments:
            if type(comment) is steamworksparser.BlankLine:
                lines.append("\t\t")
            else:
                lines.append("\t" + comment)

    lines.append("\t}")
    lines.append("")

    return lines

def parse_field(field, structname):
    lines = []
    for comment in field.c.rawprecomments:
        if type(comment) is steamworksparser.BlankLine:
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

    if field.arraysize:
        constantsstr = ""
        if not field.arraysize.isdigit():
            constantsstr = "Constants."

        if fieldtype == "byte[]":
            lines.append("\t\t[MarshalAs(UnmanagedType.ByValArray, SizeConst = " + constantsstr + field.arraysize + ")]")
        if structname == "MatchMakingKeyValuePair_t":
            lines.append("\t\t[MarshalAs(UnmanagedType.ByValTStr, SizeConst = " + constantsstr + field.arraysize + ")]")
        else:
            lines.append("\t\t[MarshalAs(UnmanagedType.ByValArray, SizeConst = " + constantsstr + field.arraysize + ")]")
            fieldtype += "[]"

    if fieldtype == "bool":
        lines.append("\t\t[MarshalAs(UnmanagedType.I1)]")

    if field.arraysize and fieldtype == "string[]":
        lines.append("\t\tprivate byte[] " + field.name + "_;")
        lines.append("\t\tpublic string " + field.name + comment)
        lines.append("\t\t{")
        lines.append("\t\t\tget { return InteropHelp.ByteArrayToStringUTF8(" + field.name + "_); }")
        lines.append("\t\t\tset { InteropHelp.StringToByteArrayUTF8(value, " + field.name + "_, " + constantsstr + field.arraysize + "); }")
        lines.append("\t\t}")
    else:
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

    steamworksparser.Settings.fake_gameserver_interfaces = True
    main(steamworksparser.parse(sys.argv[1]))