import os
import sys
from SteamworksParser import steamworksparser

class InternalConstant:
    def __init__(self, name, value, type_, precomments, comment, spacing):
        self.name = name
        self.value = value
        self.type = type_
        self.precomments = precomments
        self.comment = comment
        self.spacing = spacing

g_TypeDict = {
    # Not a bug... But, it's a giant hack.
    # The issue is that most of these are used as the MarshalAs SizeConst in C# amongst other things and C# wont auto convert them.
    "uint16": "ushort",

    "uint32": "int",
    "unsigned int": "int",

    "uint64": "ulong",
    "size_t": "int",
}

g_SkippedDefines = (
    "VALVE_COMPILE_TIME_ASSERT(",
    "REFERENCE(arg)",
    "STEAM_CALLBACK_BEGIN(",
    "STEAM_CALLBACK_MEMBER(",
    "STEAM_CALLBACK_ARRAY(",
    "END_CALLBACK_INTERNAL_BEGIN(",
    "END_CALLBACK_INTERNAL_SWITCH(",
    "END_CALLBACK_INTERNAL_END()",
    "STEAM_CALLBACK_END(",
    "INVALID_HTTPCOOKIE_HANDLE",
    "BChatMemberStateChangeRemoved(",
    "STEAM_COLOR_RED(",
    "STEAM_COLOR_GREEN(",
    "STEAM_COLOR_BLUE(",
    "STEAM_COLOR_ALPHA(",
    "INVALID_SCREENSHOT_HANDLE",
    "_snprintf",
    "S_API",
    "STEAM_CALLBACK(",
    "STEAM_CALLBACK_MANUAL(",
    "STEAM_GAMESERVER_CALLBACK(",
    "k_steamIDNil",
    "k_steamIDOutofDateGS",
    "k_steamIDLanModeGS",
    "k_steamIDNotInitYetGS",
    "k_steamIDNonSteamGS",
    "STEAM_PS3_PATH_MAX",
    "STEAM_PS3_SERVICE_ID_MAX",
    "STEAM_PS3_COMMUNICATION_ID_MAX",
    "STEAM_PS3_COMMUNICATION_SIG_MAX",
    "STEAM_PS3_LANGUAGE_MAX",
    "STEAM_PS3_REGION_CODE_MAX",
    "STEAM_PS3_CURRENT_PARAMS_VER",
    "STEAMPS3_MALLOC_INUSE",
    "STEAMPS3_MALLOC_SYSTEM",
    "STEAMPS3_MALLOC_OK",
    "S_CALLTYPE",
    "POSIX",
    "STEAM_PRIVATE_API(",
    "STEAMNETWORKINGSOCKETS_INTERFACE",

    # We just create multiple versions of this struct, Valve renamed them.
    "ControllerAnalogActionData_t",
    "ControllerDigitalActionData_t",
    "ControllerMotionData_t",

    #"INVALID_HTTPREQUEST_HANDLE",
)

g_SkippedConstants = (
    # ISteamFriends
    "k_FriendsGroupID_Invalid",

    # ISteamHTMLSurface
    "INVALID_HTMLBROWSER",

    # ISteamInventory
    "k_SteamItemInstanceIDInvalid",
    "k_SteamInventoryResultInvalid",
    "k_SteamInventoryUpdateHandleInvalid",

    # ISteamMatchmaking
    "HSERVERQUERY_INVALID",

    # ISteamRemoteStorage
    "k_UGCHandleInvalid",
    "k_PublishedFileIdInvalid",
    "k_PublishedFileUpdateHandleInvalid",
    "k_UGCFileStreamHandleInvalid",

    # ISteamUGC
    "k_UGCQueryHandleInvalid",
    "k_UGCUpdateHandleInvalid",

    # SteamClientPublic
    "k_HAuthTicketInvalid",

    # SteamTypes
    "k_uAppIdInvalid",
    "k_uDepotIdInvalid",
    "k_uAPICallInvalid",

    # steamnetworkingtypes.h
    "k_HSteamNetConnection_Invalid",
    "k_HSteamListenSocket_Invalid",
    "k_HSteamNetPollGroup_Invalid",
    "k_SteamDatagramPOPID_dev",

    # steam_gameserver.h
    "MASTERSERVERUPDATERPORT_USEGAMESOCKETSHARE",
)

g_SkippedTypedefs = (
    "uint8",
    "int8",
    "uint16",
    "int32",
    "uint32",
    "int64",
    "uint64",
)

g_CustomDefines = {
    # "Name": ("Type", "Value"),
    "k_nMaxLobbyKeyLength": ("byte", None),
    "STEAM_CONTROLLER_HANDLE_ALL_CONTROLLERS": ("ulong", "0xFFFFFFFFFFFFFFFF"),
    "STEAM_CONTROLLER_MIN_ANALOG_ACTION_DATA": ("float", "-1.0f"),
    "STEAM_CONTROLLER_MAX_ANALOG_ACTION_DATA": ("float", "1.0f"),
    "STEAM_INPUT_HANDLE_ALL_CONTROLLERS": ("ulong", "0xFFFFFFFFFFFFFFFF"),
    "STEAM_INPUT_MIN_ANALOG_ACTION_DATA": ("float", "-1.0f"),
    "STEAM_INPUT_MAX_ANALOG_ACTION_DATA": ("float", "1.0f"),
}

def main(parser):
    try:
        os.makedirs("../com.rlabrecque.steamworks.net/Runtime/autogen/")
    except OSError:
        pass

    lines = []
    constants = parse(parser)
    for constant in constants:
        for precomment in constant.precomments:
            lines.append("//" + precomment)
        lines.append("public const " + constant.type + " " + constant.name + constant.spacing + "= " + constant.value + ";" + constant.comment)

    with open("../com.rlabrecque.steamworks.net/Runtime/autogen/SteamConstants.cs", "wb") as out:
        with open("templates/header.txt", "r") as f:
            out.write(bytes(f.read(), "utf-8"))
        out.write(bytes("namespace Steamworks {\n", "utf-8"))
        out.write(bytes("\tpublic static class Constants {\n", "utf-8"))
        for line in lines:
            out.write(bytes("\t\t" + line + "\n", "utf-8"))
        out.write(bytes("\t}\n", "utf-8"))
        out.write(bytes("}\n\n", "utf-8"))
        out.write(bytes("#endif // !DISABLESTEAMWORKS\n", "utf-8"))

def parse(parser):
    interfaceversions, defines = parse_defines(parser)
    constants = parse_constants(parser)
    return interfaceversions + constants + defines

def parse_defines(parser):
    out_defines = []
    out_interfaceversions = []
    for f in parser.files:
        for d in f.defines:
            if d.name in g_SkippedDefines:
                continue

            comment = ""
            if d.c.linecomment:
                comment = " //" + d.c.linecomment

            definetype = "int"
            definevalue = d.value
            customdefine = g_CustomDefines.get(d.name, False)
            if customdefine:
                if customdefine[0]:
                    definetype = customdefine[0]
                if customdefine[1]:
                    definevalue = customdefine[1]
            elif d.value.startswith('"'):
                definetype = "string"
                if d.name.startswith("STEAM"):
                    out_interfaceversions.append(InternalConstant(d.name, definevalue, definetype, d.c.precomments, comment, " "))
                    continue

            out_defines.append(InternalConstant(d.name, definevalue, definetype, d.c.precomments, comment, d.spacing))

    return (out_interfaceversions, out_defines)


def parse_constants(parser):
    out_constants = []
    for f in parser.files:
        for constant in f.constants:
            if constant.name in g_SkippedConstants:
                continue

            comment = ""
            if constant.c.linecomment:
                comment = " //" + constant.c.linecomment

            constanttype = constant.type
            for t in parser.typedefs:
                if t.name in g_SkippedTypedefs:
                    continue

                if t.name == constant.type:
                    constanttype = t.type
                    break
            constanttype = g_TypeDict.get(constanttype, constanttype)

            constantvalue = constant.value
            if constantvalue == "0xFFFFFFFF":
                constantvalue = "-1"
            elif constantvalue == "0xffffffffffffffffull":
                constantvalue = constantvalue[:-3]

            out_constants.append(InternalConstant(constant.name, constantvalue, constanttype, constant.c.precomments, comment, " "))

    return out_constants

if __name__ == "__main__":
    if len(sys.argv) != 2:
        print("TODO: Usage Instructions")
        exit()

    steamworksparser.Settings.fake_gameserver_interfaces = True
    main(steamworksparser.parse(sys.argv[1]))
