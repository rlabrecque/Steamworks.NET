import os
import re
import sys
from collections import OrderedDict
from SteamworksParser import steamworksparser
from SteamworksParser.steamworksparser import Arg, Function, FunctionAttribute, Interface, Parser, Struct, ArgAttribute

g_SkippedFiles = (
    # We don't currently support the following interfaces because they don't provide a factory of their own.
    # You are expected to call GetISteamGeneric to get them.
    "isteamappticket.h",
    "isteamgamecoordinator.h",
    # PS3 is not supported.
    "isteamps3overlayrenderer.h",
)

g_SkippedInterfaces = (
    "ISteamNetworkingConnectionCustomSignaling",
    "ISteamGameServerNetworkingConnectionCustomSignaling",
    "ISteamNetworkingCustomSignalingRecvContext",
    "ISteamGameServerNetworkingCustomSignalingRecvContext",
    "ISteamNetworkingFakeUDPPort",
)

g_TypeDict = {
    # Built in types
    "char*": "IntPtr",
    "char *": "IntPtr",
    "char **": "out IntPtr",
    "const char*": "InteropHelp.UTF8StringHandle",
    "const char *": "InteropHelp.UTF8StringHandle",

    "const void *": "IntPtr",
    "unsigned short": "ushort",
    "void *": "IntPtr",
    "void*": "IntPtr",

    "uint8": "byte",

    "int16": "short",
    "uint16": "ushort",

    "int32": "int",
    "uint32": "uint",
    "unsigned int": "uint",
    "const uint32": "uint",

    "int64": "long",
    "uint64": "ulong",
    "uint64_t": "ulong",
    "size_t": "ulong",

    "intptr_t": "IntPtr",

    # Only used in FileLoadDialogResponse
    "const char **": "IntPtr",

    "RTime32": "uint",
    "const SteamItemInstanceID_t": "SteamItemInstanceID_t",
    "const SteamItemDef_t": "SteamItemDef_t",
    "SteamParamStringArray_t *": "IntPtr",
    "const SteamParamStringArray_t *": "IntPtr",
    "ISteamMatchmakingServerListResponse *": "IntPtr",
    "ISteamMatchmakingPingResponse *": "IntPtr",
    "ISteamMatchmakingPlayersResponse *": "IntPtr",
    "ISteamMatchmakingRulesResponse *": "IntPtr",
    #"MatchMakingKeyValuePair_t **": "IntPtr", HACK in parse_args()

    "ControllerAnalogActionData_t": "InputAnalogActionData_t",
    "ControllerDigitalActionData_t": "InputDigitalActionData_t",
    "ControllerMotionData_t": "InputMotionData_t",

    "SteamNetworkPingLocation_t &": "out SteamNetworkPingLocation_t",
    "const SteamNetworkPingLocation_t &": "ref SteamNetworkPingLocation_t",
    "SteamNetworkingIPAddr &": "out SteamNetworkingIPAddr",
    "const SteamNetworkingIPAddr &": "ref SteamNetworkingIPAddr",
    "const SteamNetworkingConfigValue_t *": "SteamNetworkingConfigValue_t[]",
    "const SteamNetworkingIdentity &": "ref SteamNetworkingIdentity",
    "const SteamNetworkingIdentity *": "ref SteamNetworkingIdentity",
    "SteamNetworkingErrMsg &": "out SteamNetworkingErrMsg",
    "const SteamNetConnectionInfo_t &": "ref SteamNetConnectionInfo_t",
    "SteamNetworkingMessage_t": "IntPtr",

    # SteamNetworkingTypes which are stubbed
    "SteamDatagramGameCoordinatorServerLogin *": "IntPtr",
    "ISteamNetworkingFakeUDPPort *": "IntPtr",

    # External type that we don't currently support.
    "const ScePadTriggerEffectParam *": "IntPtr"
}

g_WrapperArgsTypeDict = {
    "SteamParamStringArray_t *": "System.Collections.Generic.IList<string>",
    "const SteamParamStringArray_t *": "System.Collections.Generic.IList<string>",
    "ISteamMatchmakingServerListResponse *": "ISteamMatchmakingServerListResponse",
    "ISteamMatchmakingPingResponse *": "ISteamMatchmakingPingResponse",
    "ISteamMatchmakingPlayersResponse *": "ISteamMatchmakingPlayersResponse",
    "ISteamMatchmakingRulesResponse *": "ISteamMatchmakingRulesResponse",
    "MatchMakingKeyValuePair_t **": "MatchMakingKeyValuePair_t[]",
    "char **": "out string",
}

g_ReturnTypeDict = {
    # Built in types
    "const char *": "IntPtr",

    # Steamworks types
    "CSteamID": "ulong",
    "gameserveritem_t *": "IntPtr",
    "SteamNetworkingMessage_t *": "IntPtr",

    # TODO: UGH
    "ISteamApps *": "IntPtr",
    "ISteamController *": "IntPtr",
    "ISteamFriends *": "IntPtr",
    "ISteamGameSearch *": "IntPtr",
    "ISteamGameServer *": "IntPtr",
    "ISteamGameServerStats *": "IntPtr",
    "ISteamHTMLSurface *": "IntPtr",
    "ISteamHTTP *": "IntPtr",
    "ISteamInput *": "IntPtr",
    "ISteamInventory *": "IntPtr",
    "ISteamMatchmaking *": "IntPtr",
    "ISteamMatchmakingServers *": "IntPtr",
    "ISteamMusic *": "IntPtr",
    "ISteamMusicRemote *": "IntPtr",
    "ISteamNetworking *": "IntPtr",
    "ISteamParentalSettings *": "IntPtr",
    "ISteamParties *": "IntPtr",
    "ISteamPS3OverlayRender *": "IntPtr",
    "ISteamRemotePlay *": "IntPtr",
    "ISteamRemoteStorage *": "IntPtr",
    "ISteamScreenshots *": "IntPtr",
    "ISteamUGC *": "IntPtr",
    "ISteamUser *": "IntPtr",
    "ISteamUserStats *": "IntPtr",
    "ISteamUtils *": "IntPtr",
    "ISteamVideo *": "IntPtr",
}

g_SpecialReturnTypeDict = {
    "ISteamUtils_GetAppID": "AppId_t",
    "ISteamGameServerUtils_GetAppID": "AppId_t",
}

# These args are missing a clang attribute like ARRAY_COUNT
g_SpecialArgsDict = {
    # ISteamApps
    "ISteamApps_GetInstalledDepots": {
        "pvecDepots": "DepotId_t[]",
    },

    # ISteamClient
    "ISteamClient_SetLocalIPBinding": {
        "unIP": "ref SteamIPAddress_t",
    },
    "ISteamGameServerClient_SetLocalIPBinding": {
        "unIP": "ref SteamIPAddress_t",
    },

    # ISteamGameServer
    "ISteamGameServer_SendUserConnectAndAuthenticate_DEPRECATED": {
        "pvAuthBlob": "byte[]",
    },
    "ISteamGameServer_GetAuthSessionTicket": {
        "pTicket": "byte[]",
    },
    "ISteamGameServer_BeginAuthSession": {
        "pAuthTicket": "byte[]",
    },
    "ISteamGameServer_HandleIncomingPacket": {
        "pData": "byte[]",
    },
    "ISteamGameServer_GetNextOutgoingPacket": {
        "pOut": "byte[]",
    },

    # ISteamHTTP
    "ISteamHTTP_GetHTTPResponseHeaderValue": {
        "pHeaderValueBuffer": "byte[]",
    },
    "ISteamGameServerHTTP_GetHTTPResponseHeaderValue": {
        "pHeaderValueBuffer": "byte[]",
    },

    "ISteamHTTP_GetHTTPResponseBodyData": {
        "pBodyDataBuffer": "byte[]",
    },
    "ISteamGameServerHTTP_GetHTTPResponseBodyData": {
        "pBodyDataBuffer": "byte[]",
    },

    "ISteamHTTP_GetHTTPStreamingResponseBodyData": {
        "pBodyDataBuffer": "byte[]",
    },
    "ISteamGameServerHTTP_GetHTTPStreamingResponseBodyData": {
        "pBodyDataBuffer": "byte[]",
    },

    "ISteamHTTP_SetHTTPRequestRawPostBody": {
        "pubBody": "byte[]",
    },
    "ISteamGameServerHTTP_SetHTTPRequestRawPostBody": {
        "pubBody": "byte[]",
    },

    # ISteamInventory
    "ISteamInventory_SerializeResult": {
        "pOutBuffer": "byte[]",
    },
    "ISteamGameServerInventory_SerializeResult": {
        "pOutBuffer": "byte[]",
    },

    "ISteamInventory_DeserializeResult": {
        "pBuffer": "byte[]",
    },
    "ISteamGameServerInventory_DeserializeResult": {
        "pBuffer": "byte[]",
    },

    "ISteamInventory_GetResultItems": {
        "punOutItemsArraySize": "ref uint",
    },
    "ISteamGameServerInventory_GetResultItems": {
        "punOutItemsArraySize": "ref uint",
    },

    "ISteamInventory_GetItemDefinitionProperty": {
        "punValueBufferSizeOut": "ref uint",
    },
    "ISteamGameServerInventory_GetItemDefinitionProperty": {
        "punValueBufferSizeOut": "ref uint",
    },

    "ISteamInventory_GetResultItemProperty": {
        "punValueBufferSizeOut": "ref uint",
    },
    "ISteamGameServerInventory_GetResultItemProperty": {
        "punValueBufferSizeOut": "ref uint",
    },

    "ISteamInventory_GetItemDefinitionIDs": {
        "punItemDefIDsArraySize": "ref uint",
    },
    "ISteamGameServerInventory_GetItemDefinitionIDs": {
        "punItemDefIDsArraySize": "ref uint",
    },

    "ISteamInventory_GetEligiblePromoItemDefinitionIDs": {
        "punItemDefIDsArraySize": "ref uint",
    },
    "ISteamGameServerInventory_GetEligiblePromoItemDefinitionIDs": {
        "punItemDefIDsArraySize": "ref uint",
    },

    # ISteamMatchmaking
    "ISteamMatchmaking_SendLobbyChatMsg": {
        "pvMsgBody": "byte[]",
    },
    "ISteamMatchmaking_GetLobbyChatEntry": {
        "pvData": "byte[]",
    },
    "ISteamMusicRemote_SetPNGIcon_64x64": {
        "pvBuffer": "byte[]",
    },

    # ISteamMusicRemote
    "ISteamMusicRemote_UpdateCurrentEntryCoverArt": {
        "pvBuffer": "byte[]",
    },

    # ISteamNetworking
    "ISteamNetworking_SendP2PPacket": {
        "pubData": "byte[]",
    },
    "ISteamGameServerNetworking_SendP2PPacket": {
        "pubData": "byte[]",
    },

    "ISteamNetworking_ReadP2PPacket": {
        "pubDest": "byte[]",
    },
    "ISteamGameServerNetworking_ReadP2PPacket": {
        "pubDest": "byte[]",
    },

    "ISteamNetworking_SendDataOnSocket": {
        "pubData": "byte[]",
    },
    "ISteamGameServerNetworking_SendDataOnSocket": {
        "pubData": "byte[]",
    },

    "ISteamNetworking_RetrieveDataFromSocket": {
        "pubDest": "byte[]",
    },
    "ISteamGameServerNetworking_RetrieveDataFromSocket": {
        "pubDest": "byte[]",
    },

    "ISteamNetworking_RetrieveData": {
        "pubDest": "byte[]",
    },
    "ISteamGameServerNetworking_RetrieveData": {
        "pubDest": "byte[]",
    },

    # ISteamNetworkingUtils
    "ISteamNetworkingUtils_SteamNetworkingIPAddr_ToString": {
        "addr": "ref SteamNetworkingIPAddr",
        "cbBuf": "uint",
    },
    "ISteamGameServerNetworkingUtils_SteamNetworkingIPAddr_ToString": {
        "addr": "ref SteamNetworkingIPAddr",
        "cbBuf": "uint",
    },

    "ISteamNetworkingUtils_SteamNetworkingIdentity_ToString": {
        "identity": "ref SteamNetworkingIdentity",
        "cbBuf": "uint",
    },
    "ISteamGameServerNetworkingUtils_SteamNetworkingIdentity_ToString": {
        "identity": "ref SteamNetworkingIdentity",
        "cbBuf": "uint",
    },

    "ISteamNetworkingUtils_GetConfigValue": {
        "cbResult": "ref ulong",
    },
    "ISteamGameServerNetworkingUtils_GetConfigValue": {
        "cbResult": "ref ulong",
    },

    "ISteamNetworkingSockets_SendMessages": {
        "pMessages": "IntPtr[]",
        "pOutMessageNumberOrResult": "long[]",
    },
    "ISteamGameServerNetworkingSockets_SendMessages": {
        "pMessages": "IntPtr[]",
        "pOutMessageNumberOrResult": "long[]",
    },

    # ISteamNetworkingSockets
    "ISteamNetworkingSockets_GetConnectionRealTimeStatus": {
        "pStatus": "ref SteamNetConnectionRealTimeStatus_t",
        "pLanes": "ref SteamNetConnectionRealTimeLaneStatus_t",
    },
    "ISteamGameServerNetworkingSockets_GetConnectionRealTimeStatus": {
        "pStatus": "ref SteamNetConnectionRealTimeStatus_t",
        "pLanes": "ref SteamNetConnectionRealTimeLaneStatus_t",
    },

    "ISteamNetworkingSockets_ConfigureConnectionLanes": {
        "pLanePriorities": "int[]",
        "pLaneWeights": "ushort[]",
    },
    "ISteamGameServerNetworkingSockets_ConfigureConnectionLanes": {
        "pLanePriorities": "int[]",
        "pLaneWeights": "ushort[]",
    },

    # ISteamParties
    "ISteamParties_CreateBeacon": {
        "pBeaconLocation": "ref SteamPartyBeaconLocation_t",
    },
    "ISteamParties_GetAvailableBeaconLocations": {
        "pLocationList": "SteamPartyBeaconLocation_t[]",
    },

    # ISteamRemotePlay
    "ISteamRemotePlay_GetInput": {
        "pInput": "RemotePlayInput_t[]",
    },

    # ISteamRemoteStorage
    "ISteamRemoteStorage_FileWrite": {
        "pvData": "byte[]",
    },
    "ISteamRemoteStorage_FileRead": {
        "pvData": "byte[]",
    },
    "ISteamRemoteStorage_FileWriteAsync": {
        "pvData": "byte[]",
    },
    "ISteamRemoteStorage_FileReadAsyncComplete": {
        "pvBuffer": "byte[]",
    },
    "ISteamRemoteStorage_FileWriteStreamWriteChunk": {
        "pvData": "byte[]",
    },
    "ISteamRemoteStorage_UGCRead": {
        "pvData": "byte[]",
    },

    # ISteamScreenshots
    "ISteamScreenshots_WriteScreenshot": {
        "pubRGB": "byte[]",
    },

    # ISteamTimeline
    "ISteamTimeline_OpenOverlayToTimelineEvent": {
        "ulEvent": "TimelineEventHandle_t",
    },

    # ISteamUGC
    "ISteamUGC_CreateQueryUGCDetailsRequest": {
        "pvecPublishedFileID": "PublishedFileId_t[]",
    },
    "ISteamGameServerUGC_CreateQueryUGCDetailsRequest": {
        "pvecPublishedFileID": "PublishedFileId_t[]",
    },

    "ISteamUGC_GetQueryUGCChildren": {
        "pvecPublishedFileID": "PublishedFileId_t[]",
    },
    "ISteamGameServerUGC_GetQueryUGCChildren": {
        "pvecPublishedFileID": "PublishedFileId_t[]",
    },

    "ISteamUGC_GetSubscribedItems": {
        "pvecPublishedFileID": "PublishedFileId_t[]",
    },
    "ISteamGameServerUGC_GetSubscribedItems": {
        "pvecPublishedFileID": "PublishedFileId_t[]",
    },

    "ISteamUGC_StartPlaytimeTracking": {
        "pvecPublishedFileID": "PublishedFileId_t[]",
    },
    "ISteamGameServerUGC_StartPlaytimeTracking": {
        "pvecPublishedFileID": "PublishedFileId_t[]",
    },

    "ISteamUGC_StopPlaytimeTracking": {
        "pvecPublishedFileID": "PublishedFileId_t[]",
    },
    "ISteamGameServerUGC_StopPlaytimeTracking": {
        "pvecPublishedFileID": "PublishedFileId_t[]",
    },

    "ISteamUGC_GetUserContentDescriptorPreferences": {
        "pvecDescriptors": "EUGCContentDescriptorID[]",
    },
    "ISteamGameServerUGC_GetUserContentDescriptorPreferences": {
        "pvecDescriptors": "EUGCContentDescriptorID[]",
    },

    "ISteamUGC_SetItemsDisabledLocally": {
        "pvecPublishedFileIDs": "PublishedFileId_t[]",
    },
    "ISteamGameServerUGC_SetItemsDisabledLocally": {
        "pvecPublishedFileIDs": "PublishedFileId_t[]",
    },

    "ISteamUGC_SetSubscriptionsLoadOrder": {
        "pvecPublishedFileIDs": "PublishedFileId_t[]",
    },
    "ISteamGameServerUGC_SetSubscriptionsLoadOrder": {
        "pvecPublishedFileIDs": "PublishedFileId_t[]",
    },

    # ISteamUser
    "ISteamUser_InitiateGameConnection_DEPRECATED": {
        "pAuthBlob": "byte[]",
    },
    "ISteamUser_GetAvailableVoice": {
        "pcbUncompressed_Deprecated": "IntPtr",
    },
    "ISteamUser_GetVoice": {
        "pDestBuffer": "byte[]",
        "pUncompressedDestBuffer_Deprecated": "IntPtr",
        "nUncompressBytesWritten_Deprecated": "IntPtr",
    },
    "ISteamUser_DecompressVoice": {
        "pCompressed": "byte[]",
        "pDestBuffer": "byte[]",
    },
    "ISteamUser_GetAuthSessionTicket": {
        "pTicket": "byte[]",
    },
    "ISteamUser_BeginAuthSession": {
        "pAuthTicket": "byte[]",
    },
    "ISteamUser_RequestEncryptedAppTicket": {
        "pDataToInclude": "byte[]",
    },
    "ISteamUser_GetEncryptedAppTicket": {
        "pTicket": "byte[]",
    },

    # ISteamUserStats
    "ISteamUserStats_GetDownloadedLeaderboardEntry": {
        "pDetails": "int[]",
    },
    "ISteamUserStats_UploadLeaderboardScore": {
        "pScoreDetails": "int[]",
    },

    # ISteamUtils
    "ISteamUtils_GetImageRGBA": {
        "pubDest": "byte[]",
    },
    "ISteamGameServerUtils_GetImageRGBA": {
        "pubDest": "byte[]",
    },

    # ISteamVideo
    "ISteamVideo_GetOPFStringForApp": {
        "pnBufferSize": "ref int"
    },

    # This is a little nicety that we provide
    "ISteamFriends_GetFriendCount": {
        "iFriendFlags": "EFriendFlags",
    },
    "ISteamFriends_GetFriendByIndex": {
        "iFriendFlags": "EFriendFlags",
    },
    "ISteamFriends_HasFriend": {
        "iFriendFlags": "EFriendFlags",
    },
}

g_SpecialWrapperArgsDict = {
    # These are void* but we want out string
    "ISteamFriends_GetClanChatMessage": {
        "prgchText": "out string",
    },
    "ISteamFriends_GetFriendMessage": {
        "pvData": "out string",
    },
}

g_FixedAttributeValues = {
    "ISteamInventory_GetItemsWithPrices": {
        "pArrayItemDefs": ArgAttribute("STEAM_OUT_ARRAY_COUNT", "unArrayLength"),
        "pCurrentPrices": ArgAttribute("STEAM_OUT_ARRAY_COUNT", "unArrayLength"),
        "pBasePrices": ArgAttribute("STEAM_OUT_ARRAY_COUNT", "unArrayLength"),
    },
    "ISteamGameServerInventory_GetItemsWithPrices": {
        "pArrayItemDefs": ArgAttribute("STEAM_OUT_ARRAY_COUNT", "unArrayLength"),
        "pCurrentPrices": ArgAttribute("STEAM_OUT_ARRAY_COUNT", "unArrayLength"),
        "pBasePrices": ArgAttribute("STEAM_OUT_ARRAY_COUNT", "unArrayLength"),
    },
    "ISteamUGC_GetQueryUGCContentDescriptors": {
        "pvecDescriptors": ArgAttribute("STEAM_OUT_ARRAY_COUNT", "cMaxEntries"),
    },
    "ISteamGameServerUGC_GetQueryUGCContentDescriptors": {
        "pvecDescriptors": ArgAttribute("STEAM_OUT_ARRAY_COUNT", "cMaxEntries"),
    },
    "ISteamNetworkingMessages_ReceiveMessagesOnChannel": {
        "ppOutMessages": ArgAttribute("STEAM_OUT_ARRAY_COUNT", "nMaxMessages"),
    },
    "ISteamGameServerNetworkingMessages_ReceiveMessagesOnChannel": {
        "ppOutMessages": ArgAttribute("STEAM_OUT_ARRAY_COUNT", "nMaxMessages"),
    },
    "ISteamNetworkingSockets_ReceiveMessagesOnConnection": {
        "ppOutMessages": ArgAttribute("STEAM_OUT_ARRAY_COUNT", "nMaxMessages"),
    },
    "ISteamGameServerNetworkingSockets_ReceiveMessagesOnConnection": {
        "ppOutMessages": ArgAttribute("STEAM_OUT_ARRAY_COUNT", "nMaxMessages"),
    },
    "ISteamNetworkingSockets_ReceiveMessagesOnPollGroup": {
        "ppOutMessages": ArgAttribute("STEAM_OUT_ARRAY_COUNT", "nMaxMessages"),
    },
    "ISteamGameServerNetworkingSockets_ReceiveMessagesOnPollGroup": {
        "ppOutMessages": ArgAttribute("STEAM_OUT_ARRAY_COUNT", "nMaxMessages"),
    },
}

g_SpecialOutStringRetCmp = {
    "ISteamFriends_GetClanChatMessage": "ret != 0",
    "ISteamFriends_GetFriendMessage": "ret != 0",
}

g_SkippedTypedefs = (
    "uint8",
    "int8",
    "int16",
    "uint16",
    "int32",
    "uint32",
    "int64",
    "uint64",
)

g_ArgDefaultLookup = {
    "k_EActivateGameOverlayToWebPageMode_Default": "EActivateGameOverlayToWebPageMode.k_EActivateGameOverlayToWebPageMode_Default",
    "k_ETimelineEventClipPriority_None": "ETimelineEventClipPriority.k_ETimelineEventClipPriority_None",
    "NULL": "null",
    "nullptr": "null",
    "0.f": "0.0f",
}

HEADER = None

g_NativeMethods = []
g_Output = []
g_Typedefs = None

def main(parser: Parser):
    try:
        os.makedirs("../com.rlabrecque.steamworks.net/Runtime/autogen/")
    except OSError:
        pass

    with open("templates/header.txt", "r") as f:
        global HEADER
        HEADER = f.read()

    global g_Typedefs
    g_Typedefs = parser.typedefs
    for f in parser.files:
        parse(f, parser)

    with open("../com.rlabrecque.steamworks.net/Runtime/autogen/NativeMethods.cs", "wb") as out:
        #out.write(bytes(HEADER, "utf-8"))
        with open("templates/nativemethods.txt", "r") as f:
            out.write(bytes(f.read(), "utf-8"))
        for line in g_NativeMethods:
            out.write(bytes(line + "\n", "utf-8"))
        out.write(bytes("\t}\n", "utf-8"))
        out.write(bytes("}\n\n", "utf-8"))
        out.write(bytes("#endif // !DISABLESTEAMWORKS\n", "utf-8"))

def get_arg_attribute(strEntryPoint, arg):
    return g_FixedAttributeValues.get(strEntryPoint, dict()).get(arg.name, arg.attribute)

def parse(f, parser: Parser):
    if f.name in g_SkippedFiles:
        return

    print("File: " + f.name)

    del g_Output[:]
    for interface in f.interfaces:
        parse_interface(f, interface, parser)

    if g_Output:
        with open('../com.rlabrecque.steamworks.net/Runtime/autogen/' + os.path.splitext(f.name)[0] + '.cs', 'wb') as out:
            out.write(bytes(HEADER, "utf-8"))
            out.write(bytes("namespace Steamworks {\n", "utf-8"))
            for line in g_Output:
                out.write(bytes(line + "\n", "utf-8"))
            out.write(bytes("}\n\n", "utf-8"))  # Namespace
            out.write(bytes("#endif // !DISABLESTEAMWORKS\n", "utf-8"))


def parse_interface(f, interface: Interface, parser: Parser):
    if interface.name in g_SkippedInterfaces:
        return

    print(" - " + interface.name)
    g_Output.append('\tpublic static class ' + interface.name[1:] + ' {')

    if "GameServer" in interface.name and interface.name != "ISteamGameServer" and interface.name != "ISteamGameServerStats":
        bGameServerVersion = True
    else:
        bGameServerVersion = False


    if not bGameServerVersion:
        g_NativeMethods.append("#region " + interface.name[1:])

    lastIfStatement = None
    for func in interface.functions:
        if func.ifstatements != lastIfStatement:
            if lastIfStatement is not None:
                if not bGameServerVersion:
                    g_NativeMethods[-1] = "#endif"

                g_Output[-1] = "#endif"
                lastIfStatement = None

                if func.ifstatements:
                    g_NativeMethods.append("#if " + func.ifstatements.replace("defined(", "").replace(")", ""))
                    g_Output.append("#if " + func.ifstatements.replace("defined(", "").replace(")", ""))
                    lastIfStatement = func.ifstatements
            elif func.ifstatements:
                if not bGameServerVersion:
                    g_NativeMethods[-1] = "#if " + func.ifstatements.replace("defined(", "").replace(")", "")
                    g_NativeMethods[-1] = "#if " + func.ifstatements.replace("defined(", "").replace(")", "")

                g_Output[-1] = "#if " + func.ifstatements.replace("defined(", "").replace(")", "")
                lastIfStatement = func.ifstatements

        if func.private:
            continue

        shouldGenerateLargePack = False
        strEntryPoint = interface.name + '_' + func.name
        for attr in func.attributes:
            if attr.name == "STEAM_FLAT_NAME":
                strEntryPoint = interface.name + '_' + attr.value
                break
        
        parsed_args = parse_args(strEntryPoint, func.args, None, parser)
        parse_func_native(f, interface, func, strEntryPoint, parsed_args, False, bGameServerVersion, parser)
        
        generate_wrapper_function(f, interface, func, parsed_args, strEntryPoint, bGameServerVersion, parser)
                

    # Remove last whitespace
    if not bGameServerVersion:
        del g_NativeMethods[-1]

    del g_Output[-1]

    if lastIfStatement is not None:
        if not bGameServerVersion:
            g_NativeMethods.append("#endif")

        g_Output.append("#endif")

    if not bGameServerVersion:
        g_NativeMethods.append("#endregion")

    g_Output.append("\t}")

def parse_func_native(f, interface, func: Function, strEntryPoint: str, args, generatingLargePack: bool, bGameServerVersion: bool, parser: Parser):
    wrapperreturntype = None
    strCast = ""
    returntype = func.returntype
    returntype = g_SpecialReturnTypeDict.get(strEntryPoint, returntype)
    for t in g_Typedefs:
        if t.name == returntype:
            if t.name not in g_SkippedTypedefs:
                wrapperreturntype = returntype
                strCast = "(" + returntype + ")"
                returntype = t.type
            break
    returntype = g_TypeDict.get(returntype, returntype)
    returntype = g_TypeDict.get(func.returntype, returntype)
    returntype = g_ReturnTypeDict.get(func.returntype, returntype)
    if wrapperreturntype == None:
        wrapperreturntype = returntype

    pinvokeargs = args[0]  # TODO: NamedTuple
    isPacksizeAware = args[6]
    largePackPInvokeArgs = args[9]
    if not bGameServerVersion:
        g_NativeMethods.append("\t\t[DllImport(NativeLibraryName, EntryPoint = \"SteamAPI_{0}\", CallingConvention = CallingConvention.Cdecl)]".format(strEntryPoint))

        if returntype == "bool":
            g_NativeMethods.append("\t\t[return: MarshalAs(UnmanagedType.I1)]")

        g_NativeMethods.append("\t\tpublic static extern {0} {1}({2});".format(returntype, strEntryPoint, pinvokeargs))
        g_NativeMethods.append("")

        if isPacksizeAware:
            g_NativeMethods.append("\t#if STEAMWORKS_ANYCPU")
            g_NativeMethods.append("\t\t[DllImport(NativeLibraryName, EntryPoint = \"SteamAPI_{0}\", CallingConvention = CallingConvention.Cdecl)]".format(strEntryPoint))

            if returntype == "bool":
                g_NativeMethods.append("\t\t[return: MarshalAs(UnmanagedType.I1)]")

            g_NativeMethods.append("\t\tpublic static extern {0} {1}({2});".format(returntype, strEntryPoint, largePackPInvokeArgs))
            g_NativeMethods.append("\t#endif")
            g_NativeMethods.append("")
        pass

def generate_wrapper_function(f, interface, func: Function,
                               args: tuple[str, str, str, list[str], tuple[list[str], list[Arg]], OrderedDict, str, bool, str],
                                 strEntryPoint: str, bGameServerVersion: bool, parser: Parser):
    wrapperargs = args[1]
    argnames = args[2]
    stringargs = args[3]
    outstringargs = args[4][0]
    outstringsize = args[4][1]
    args_with_explicit_count = args[5]
    isPacksizeAware = args[6]
    largePackNativeArgs: str = args[7]
    largePackByrefArgs: list[(str, str, bool)] = args[8] # (typeName, argName, shouldAssignInput)
    
    strCast = ""
    wrapperreturntype = None
    returntype = func.returntype
    returntype = g_SpecialReturnTypeDict.get(strEntryPoint, returntype)
    for t in g_Typedefs:
        if t.name == returntype:
            if t.name not in g_SkippedTypedefs:
                wrapperreturntype = returntype
                strCast = "(" + returntype + ")"
                returntype = t.type
            break
    returntype = g_TypeDict.get(returntype, returntype)
    returntype = g_TypeDict.get(func.returntype, returntype)
    returntype = g_ReturnTypeDict.get(func.returntype, returntype)
    if wrapperreturntype == None:
        wrapperreturntype = returntype
    
    functionBody = []

    if 'GameServer' in interface.name:
        functionBody.append("\t\t\tInteropHelp.TestIfAvailableGameServer();")
    else:
        functionBody.append("\t\t\tInteropHelp.TestIfAvailableClient();")

    for argname, argsize in args_with_explicit_count.items():
        if argsize not in argnames:
            argsize = "Constants." + argsize
        functionBody.append("\t\t\tif ({0} != null && {0}.Length != {1}) {{".format(argname, argsize))
        functionBody.append("\t\t\t\tthrow new System.ArgumentException(\"{0} must be the same size as {1}!\");".format(argname, argsize))
        functionBody.append("\t\t\t}")

    strReturnable = "return "
    if func.returntype == "void":
        strReturnable = ""
    elif func.returntype == "const char *" or func.returntype == "const char*":
        wrapperreturntype = "string"
        strReturnable += "InteropHelp.PtrToStringUTF8("
        argnames += ")"
    elif func.returntype == "gameserveritem_t *":
        wrapperreturntype = "gameserveritem_t"
        strReturnable += "(gameserveritem_t)Marshal.PtrToStructure("
        argnames += "), typeof(gameserveritem_t)"
    elif func.returntype == "CSteamID":
        wrapperreturntype = "CSteamID"
        strReturnable += "(CSteamID)"

    if outstringargs:
        if returntype != "void":
            strReturnable = returntype + " ret = "

        for i, argName in enumerate(outstringargs):
            if not outstringsize:
                functionBody.append("\t\t\tIntPtr " + argName + "2;")
                continue

            cast = ""
            if outstringsize[i].type != "int":
                cast = "(int)"

            functionBody.append("\t\t\tIntPtr " + argName + "2 = Marshal.AllocHGlobal(" + cast + outstringsize[i].name + ");")

    # TODO fix `ISteamGameServerClient_CreateSteamPipe`
    indentlevel = "\t\t\t"
    if stringargs:
        indentlevel += "\t"
        for argName in stringargs:
            functionBody.append("\t\t\tusing (var " + argName + "2 = new InteropHelp.UTF8StringHandle(" + argName + "))")

        functionBody[-1] += " {"

    if bGameServerVersion:
        invokingNativeFunctionName = interface.name.replace("GameServer", "") + '_' + func.name

        for attr in func.attributes:
            if attr.name == "STEAM_FLAT_NAME":
                invokingNativeFunctionName = interface.name.replace("GameServer", "") + '_' + attr.value
                break
    else:
        invokingNativeFunctionName = strEntryPoint

    if not isPacksizeAware:
        functionBody.append("{0}{1}{2}NativeMethods.{3}({4});".format(
            indentlevel, strReturnable, strCast,
             invokingNativeFunctionName, argnames))
    else:
        b:list[str] = []

        if returntype != "void":
            b.append(f"{wrapperreturntype} ret;")

        invocationTemplate = "{0}{1}{2}NativeMethods.{3}({4});"
        prebuiltInvocationExpression = invocationTemplate.format(
            "", "" if returntype == "void" else "ret = ", strCast,
             invokingNativeFunctionName, argnames)
        
        prebuiltInvocationExpressionLargePack = invocationTemplate.format(
            "", "" if returntype == "void" else "ret = ", strCast,
             invokingNativeFunctionName, largePackNativeArgs
        )
        
        # b.append(f"{returntype} anyCpuResult;")
        b.append("if (!Packsize.IsLargePack) {")
        b.append("\t" + prebuiltInvocationExpression)
        b.append("} else {")

        # generate large-pack byref intermediate struct variables 
        for lpArg in largePackByrefArgs:
            if not lpArg[0].endswith("[]"):
                assignByRefManaged = "" if not lpArg[2] else f" = {lpArg[1]}"
                b.append(f"\t{lpArg[0]} {lpArg[1]}_lp{assignByRefManaged};")
            else:
                b.append(f"\t{lpArg[0][:-2]}_LargePack[] {lpArg[1]}_lp = new {lpArg[0][:-2]}_LargePack[{lpArg[1]}.Length];")
                b.append(f"\tfor (int i = 0; i < {lpArg[1]}.Length; i++)")
                b.append(f"\t\t{lpArg[1]}_lp[i] = {lpArg[1]}[i];")

        b.append("\t" + prebuiltInvocationExpressionLargePack)
        # convert large pack form to managed form
        for lpArg in largePackByrefArgs:
            if not lpArg[0].endswith('[]'):
                b.append(f"\t{lpArg[1]} = {lpArg[1]}_lp;")
            else:
                b.append(f"\tfor (int i = 0; i < {lpArg[1]}.Length; i++)")
                b.append(f"\t\t{lpArg[1]}[i] = {lpArg[1]}_lp[i];")

        b.append("}")

        functionBody.extend(map(lambda l: "\t\t\t" + l, b))

    if outstringargs:
        retcmp = "ret != 0"
        if returntype == "bool":
            retcmp = "ret"
        elif returntype == "int":
            retcmp = "ret != -1"
        retcmp = g_SpecialOutStringRetCmp.get(strEntryPoint, retcmp)
        for argName in outstringargs:
            if returntype == "void":
                functionBody.append(indentlevel + argName + " = InteropHelp.PtrToStringUTF8(" + argName + "2);")
            else:
                functionBody.append(indentlevel + argName + " = " + retcmp + " ? InteropHelp.PtrToStringUTF8(" + argName + "2) : null;")

            if strEntryPoint != "ISteamRemoteStorage_GetUGCDetails":
                functionBody.append(indentlevel + "Marshal.FreeHGlobal(" + argName + "2);")

    if (returntype != "void" and isPacksizeAware) or (returntype != "void" and outstringargs):
        functionBody.append(indentlevel + "return ret;")

    if stringargs:
        functionBody.append("\t\t\t}")


    comments = func.comments
    if func.linecomment:
        comments.append(func.linecomment)

    if comments:
        g_Output.append("\t\t/// <summary>")
        for c in comments:
            c = c.replace('&', '&amp;').replace('<', '&lt;').replace('>', '&gt;')#.replace('/*', '').replace('*/', '')
            if c:
                g_Output.append("\t\t/// <para>" + c + "</para>")
        g_Output.append("\t\t/// </summary>")
    g_Output.append("\t\tpublic static " + wrapperreturntype + " " + func.name.rstrip("0") + "(" + wrapperargs + ") {")

    g_Output.extend(functionBody)

    g_Output.append("\t\t}")
    g_Output.append("")

def parse_args(strEntryPoint: str, args: list[Arg], _: bool, parser: Parser):
    # Akarinnnnn: I think we should extract a result class
    pinvokeargs = "IntPtr instancePtr, "
    pinvokeargsLargePack = pinvokeargs
    wrapperargs = ""
    nativeFunctionArgs = ""
    nativeFunctionArgsLargePack = ""
    stringargs = []
    outstringargs = []
    outstringsize = []
    isMethodPacksizeAware = False
    args_with_explicit_count = OrderedDict()
    largePackArgMarshalInfo: list[(str,  str, bool)] = []

    ifacename = strEntryPoint[1:strEntryPoint.index('_')]

    #region init native function params string
    if "GameServer" in ifacename:
        if ifacename != "SteamGameServer" and ifacename != "SteamGameServerStats":
            ifacename = ifacename.replace("GameServer", "")
        nativeFunctionArgs = "CSteamGameServerAPIContext.Get" + ifacename + "(), "
        nativeFunctionArgsLargePack = "CSteamGameServerAPIContext.Get" + ifacename + "(), "
    else:
        nativeFunctionArgs = "CSteamAPIContext.Get" + ifacename + "(), "
        nativeFunctionArgsLargePack = "CSteamAPIContext.Get" + ifacename + "(), "

    getNextArgAsStringSize = False
    argNamesToAddAsStringSize = []

    for arg in args:

        #region populate PInvoke params list and wrapper args (both LP and SP)
        potentialtype = arg.type.rstrip("*").lstrip("const ").rstrip()
        isThisArgPackAware = potentialtype in parser.packSizeAwareStructs

        isMethodPacksizeAware = True if isThisArgPackAware else isMethodPacksizeAware

        pInvokeArgType = g_TypeDict.get(arg.type, arg.type)

        isParamArray = False
        largePackArgMarshalRecord = None
        if pInvokeArgType.endswith("*"):
            wrapperParamType = g_TypeDict.get(potentialtype, potentialtype)
            pInvokeArgType = "out " + wrapperParamType
            
        if isThisArgPackAware:
            # add this arg to marshal list
            largePackArgMarshalRecord = (potentialtype, arg.name, True)


        pInvokeArgType = g_SpecialArgsDict.get(strEntryPoint, dict()).get(arg.name, pInvokeArgType)

        argattribute = get_arg_attribute(strEntryPoint, arg)
        if argattribute:
            if argattribute.name in ("STEAM_OUT_ARRAY", "STEAM_OUT_ARRAY_CALL", "STEAM_OUT_ARRAY_COUNT", "STEAM_ARRAY_COUNT","STEAM_ARRAY_COUNT_D"):
                isParamArray = True
                pInvokeArgType = g_TypeDict.get(potentialtype, potentialtype) + "[]"
                if isMethodPacksizeAware:
                    pInvokeLargePackType = g_TypeDict.get(potentialtype, potentialtype) + "_LargePack[]"

            if argattribute.name == "STEAM_OUT_ARRAY_COUNT":
                commaindex = argattribute.value.find(',')
                if commaindex > 0:
                    args_with_explicit_count[arg.name] = argattribute.value[:commaindex]
                else:
                    args_with_explicit_count[arg.name] = argattribute.value

        if isParamArray and isThisArgPackAware  :
            (t, n, byref) = largePackArgMarshalRecord
            largePackArgMarshalRecord = (t + "[]", n, byref)

        if arg.type == "MatchMakingKeyValuePair_t **":  # TODO: Fixme - Small Hack... We do this because MatchMakingKeyValuePair's have ARRAY_COUNT() and two **'s, things get broken :(
            pInvokeArgType = "IntPtr"

        # We skip byte[] because it is a primitive type that C# can essentially mmap and get a great perf increase while marshalling.
        # We need to do this for other primitive types eventually but that will require more testing to make sure nothing breaks.
        if pInvokeArgType.endswith("[]") and pInvokeArgType != "byte[]":
            pInvokeArgType = "[In, Out] " + pInvokeArgType
        elif pInvokeArgType == "bool":
            pInvokeArgType = "[MarshalAs(UnmanagedType.I1)] " + pInvokeArgType

        pinvokeargs += pInvokeArgType + " " + arg.name + ", "
        if isThisArgPackAware:
            if pInvokeArgType.endswith('[]'):
                pinvokeargsLargePack += f"{pInvokeArgType[:-2]}_LargePack[] {arg.name}_lp, "
                if isThisArgPackAware:
                    (t, n, b) = largePackArgMarshalRecord
                    largePackArgMarshalRecord = (f"{t}[]", n, b)
            else:
                pinvokeargsLargePack += f"{pInvokeArgType}_LargePack {arg.name}_lp, "

        else:
            pinvokeargsLargePack += pInvokeArgType + " " + arg.name + ", "

        pInvokeArgType = pInvokeArgType.replace("[In, Out] ", "").replace("[MarshalAs(UnmanagedType.I1)] ", "")
        wrapperargtype = g_WrapperArgsTypeDict.get(arg.type, pInvokeArgType)
        wrapperargtype = g_SpecialWrapperArgsDict.get(strEntryPoint, dict()).get(arg.name, wrapperargtype)
        
        if wrapperargtype == "InteropHelp.UTF8StringHandle":
            wrapperargtype = "string"
        elif arg.type == "char *" or arg.type == "char*":
            wrapperargtype = "out string"

        if not arg.name.endswith("Deprecated"):
            wrapperargs += wrapperargtype + " " + arg.name
            if arg.default:
                wrapperargs += " = " + g_ArgDefaultLookup.get(arg.default, arg.default)
            wrapperargs += ", "
            

        if pInvokeArgType.startswith("out"):
            nativeFunctionArgs += "out "
            nativeFunctionArgsLargePack += "out "
            if isThisArgPackAware:
                (t, n, _) = largePackArgMarshalRecord
                largePackArgMarshalRecord = (t, n, False)
        elif wrapperargtype.startswith("ref"):
            nativeFunctionArgs += "ref "
            nativeFunctionArgsLargePack += "ref "
            if isThisArgPackAware:
                # make original value passing in
                (t, n, _) = (largePackArgMarshalRecord)
                largePackArgMarshalRecord = (t, n, True) 

        if wrapperargtype == "System.Collections.Generic.IList<string>":
            nativeFunctionArgs += "new InteropHelp.SteamParamStringArray(" + arg.name + ")"
            nativeFunctionArgsLargePack += "new InteropHelp.SteamParamStringArray(" + arg.name + ")"
        elif wrapperargtype == "MatchMakingKeyValuePair_t[]":
            nativeFunctionArgs += "new MMKVPMarshaller(" + arg.name + ")"
            nativeFunctionArgsLargePack += "new MMKVPMarshaller(" + arg.name + ")"
        elif wrapperargtype.endswith("Response"):
            nativeFunctionArgs += "(IntPtr)" + arg.name
            nativeFunctionArgsLargePack += "(IntPtr)" + arg.name
        elif arg.name.endswith("Deprecated"):
            if pInvokeArgType == "IntPtr":
                nativeFunctionArgs += "IntPtr.Zero"
                nativeFunctionArgsLargePack += "IntPtr.Zero"
            elif pInvokeArgType == "bool":
                nativeFunctionArgs += "false"
                nativeFunctionArgsLargePack += "false"
            else:
                nativeFunctionArgs += "0"
                nativeFunctionArgsLargePack += "0"
        else:
            nativeFunctionArgs += arg.name
            nativeFunctionArgsLargePack += arg.name
        
        if getNextArgAsStringSize:
            getNextArgAsStringSize = False
            outstringsize.append(arg)

        for tempargname in argNamesToAddAsStringSize:
            if tempargname == arg.name:
                outstringsize.append(arg)

        if wrapperargtype == "string":
            stringargs.append(arg.name)
            nativeFunctionArgs += "2"
            nativeFunctionArgsLargePack += "2"
        elif wrapperargtype == "out string":
            outstringargs.append(arg.name)
            nativeFunctionArgs += "2"
            nativeFunctionArgsLargePack += "2"
            if argattribute:
                if argattribute.name == "STEAM_OUT_STRING_COUNT":
                    argNamesToAddAsStringSize.append(argattribute.value)
                if argattribute.name == "STEAM_OUT_STRING":
                    pass
            else:
                getNextArgAsStringSize = True
        

        
        if isThisArgPackAware:
            nativeFunctionArgsLargePack += "_lp"
            largePackArgMarshalInfo.append(largePackArgMarshalRecord)
        
        nativeFunctionArgs += ", "
        nativeFunctionArgsLargePack += ", "


    pinvokeargs = pinvokeargs.rstrip(", ")
    pinvokeargsLargePack = pinvokeargsLargePack.rstrip(", ")
    nativeFunctionArgsLargePack = nativeFunctionArgsLargePack.rstrip(", ")
    wrapperargs = wrapperargs.rstrip(", ")
    nativeFunctionArgs = nativeFunctionArgs.rstrip(", ")
    nativeFunctionArgsLargePack = nativeFunctionArgsLargePack.rstrip(", ")
    return (pinvokeargs, wrapperargs, nativeFunctionArgs, stringargs, (outstringargs, outstringsize),
             args_with_explicit_count, isMethodPacksizeAware, nativeFunctionArgsLargePack,
             largePackArgMarshalInfo, pinvokeargsLargePack if isMethodPacksizeAware else None)


if __name__ == "__main__":
    if len(sys.argv) != 2:
        print("TODO: Usage Instructions")
        exit()

    steamworksparser.Settings.fake_gameserver_interfaces = True
    main(steamworksparser.parse(sys.argv[1]))
