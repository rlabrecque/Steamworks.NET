import os
import sys
from collections import OrderedDict
from SteamworksParser import steamworksparser

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
    "SteamNetworkingMessage_t **": "IntPtr[]",

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
    "ISteamAppList *": "IntPtr",
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

g_SpecialArgsDict = {
    # These args are missing a clang attribute like ARRAY_COUNT
    "ISteamAppList_GetInstalledApps": {
        "pvecAppID": "AppId_t[]",
    },
    "ISteamApps_GetInstalledDepots": {
        "pvecDepots": "DepotId_t[]",
    },
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
    "ISteamHTTP_GetHTTPResponseHeaderValue": {
        "pHeaderValueBuffer": "byte[]",
    },
    "ISteamHTTP_GetHTTPResponseBodyData": {
        "pBodyDataBuffer": "byte[]",
    },
    "ISteamHTTP_GetHTTPStreamingResponseBodyData": {
        "pBodyDataBuffer": "byte[]",
    },
    "ISteamHTTP_SetHTTPRequestRawPostBody": {
        "pubBody": "byte[]",
    },
    "ISteamInventory_SerializeResult": {
        "pOutBuffer": "byte[]",
    },
    "ISteamInventory_DeserializeResult": {
        "pBuffer": "byte[]",
    },
    "ISteamMatchmaking_SendLobbyChatMsg": {
        "pvMsgBody": "byte[]",
    },
    "ISteamMatchmaking_GetLobbyChatEntry": {
        "pvData": "byte[]",
    },
    "ISteamMusicRemote_SetPNGIcon_64x64": {
        "pvBuffer": "byte[]",
    },
    "ISteamMusicRemote_UpdateCurrentEntryCoverArt": {
        "pvBuffer": "byte[]",
    },
    "ISteamNetworking_SendP2PPacket": {
        "pubData": "byte[]",
    },
    "ISteamNetworking_ReadP2PPacket": {
        "pubDest": "byte[]",
    },
    "ISteamNetworking_SendDataOnSocket": {
        "pubData": "byte[]",
    },
    "ISteamNetworking_RetrieveDataFromSocket": {
        "pubDest": "byte[]",
    },
    "ISteamNetworking_RetrieveData": {
        "pubDest": "byte[]",
    },
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
    "ISteamScreenshots_WriteScreenshot": {
        "pubRGB": "byte[]",
    },
    "ISteamUGC_CreateQueryUGCDetailsRequest": {
        "pvecPublishedFileID": "PublishedFileId_t[]",
    },
    "ISteamUGC_GetQueryUGCChildren": {
        "pvecPublishedFileID": "PublishedFileId_t[]",
    },
    "ISteamUGC_GetSubscribedItems": {
        "pvecPublishedFileID": "PublishedFileId_t[]",
    },
    "ISteamUGC_StartPlaytimeTracking": {
        "pvecPublishedFileID": "PublishedFileId_t[]",
    },
    "ISteamUGC_StopPlaytimeTracking": {
        "pvecPublishedFileID": "PublishedFileId_t[]",
    },
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
    "ISteamUserStats_GetDownloadedLeaderboardEntry": {
        "pDetails": "int[]",
    },
    "ISteamUserStats_UploadLeaderboardScore": {
        "pScoreDetails": "int[]",
    },
    "ISteamUtils_GetImageRGBA": {
        "pubDest": "byte[]",
    },

    # GameServer Copies
    "ISteamGameServerHTTP_GetHTTPResponseHeaderValue": {
        "pHeaderValueBuffer": "byte[]",
    },
    "ISteamGameServerHTTP_GetHTTPResponseBodyData": {
        "pBodyDataBuffer": "byte[]",
    },
    "ISteamGameServerHTTP_GetHTTPStreamingResponseBodyData": {
        "pBodyDataBuffer": "byte[]",
    },
    "ISteamGameServerHTTP_SetHTTPRequestRawPostBody": {
        "pubBody": "byte[]",
    },
    "ISteamGameServerInventory_SerializeResult": {
        "pOutBuffer": "byte[]",
    },
    "ISteamGameServerInventory_DeserializeResult": {
        "pBuffer": "byte[]",
    },
    "ISteamGameServerNetworking_SendP2PPacket": {
        "pubData": "byte[]",
    },
    "ISteamGameServerNetworking_ReadP2PPacket": {
        "pubDest": "byte[]",
    },
    "ISteamGameServerNetworking_SendDataOnSocket": {
        "pubData": "byte[]",
    },
    "ISteamGameServerNetworking_RetrieveDataFromSocket": {
        "pubDest": "byte[]",
    },
    "ISteamGameServerNetworking_RetrieveData": {
        "pubDest": "byte[]",
    },
    "ISteamGameServerUtils_GetImageRGBA": {
        "pubDest": "byte[]",
    },
    "ISteamGameServerUGC_CreateQueryUGCDetailsRequest": {
        "pvecPublishedFileID": "PublishedFileId_t[]",
    },
    "ISteamGameServerUGC_GetQueryUGCChildren": {
        "pvecPublishedFileID": "PublishedFileId_t[]",
    },
    "ISteamGameServerUGC_GetSubscribedItems": {
        "pvecPublishedFileID": "PublishedFileId_t[]",
    },
    "ISteamGameServerUGC_StartPlaytimeTracking": {
        "pvecPublishedFileID": "PublishedFileId_t[]",
    },
    "ISteamGameServerUGC_StopPlaytimeTracking": {
        "pvecPublishedFileID": "PublishedFileId_t[]",
    },

    # This is a little nicety that we provide, I don't know why Valve doesn't just change it.
    "ISteamFriends_GetFriendCount": {
        "iFriendFlags": "EFriendFlags",
    },
    "ISteamFriends_GetFriendByIndex": {
        "iFriendFlags": "EFriendFlags",
    },
    "ISteamFriends_HasFriend": {
        "iFriendFlags": "EFriendFlags",
    },

    # These end up being "out type", when we need them to be "ref type"
    "ISteamInventory_GetResultItems": {
        "punOutItemsArraySize": "ref uint",
    },
    "ISteamInventory_GetItemDefinitionProperty": {
        "punValueBufferSizeOut": "ref uint",
    },
    "ISteamInventory_GetResultItemProperty": {
        "punValueBufferSizeOut": "ref uint",
    },
    "ISteamInventory_GetItemDefinitionIDs": {
        "punItemDefIDsArraySize": "ref uint",
    },
    "ISteamInventory_GetEligiblePromoItemDefinitionIDs": {
        "punItemDefIDsArraySize": "ref uint",
    },

    # And the GameServer versions:
    "ISteamGameServerInventory_GetResultItems": {
        "punOutItemsArraySize": "ref uint",
    },
    "ISteamGameServerInventory_GetItemDefinitionProperty": {
        "punValueBufferSizeOut": "ref uint",
    },
    "ISteamGameServerInventory_GetResultItemProperty": {
        "punValueBufferSizeOut": "ref uint",
    },
    "ISteamGameServerInventory_GetItemDefinitionIDs": {
        "punItemDefIDsArraySize": "ref uint",
    },
    "ISteamGameServerInventory_GetEligiblePromoItemDefinitionIDs": {
        "punItemDefIDsArraySize": "ref uint",
    },

    "ISteamVideo_GetOPFStringForApp": {
        "pnBufferSize": "ref int"
    },

    "ISteamParties_CreateBeacon": {
        "pBeaconLocation": "ref SteamPartyBeaconLocation_t",
    },
    "ISteamParties_GetAvailableBeaconLocations": {
        "pLocationList": "SteamPartyBeaconLocation_t[]",
    },

    "ISteamClient_SetLocalIPBinding": {
        "unIP": "ref SteamIPAddress_t",
    },

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
        "pMessages": "SteamNetworkingMessage_t[]",
        "pOutMessageNumberOrResult": "long[]",
    },
    "ISteamGameServerNetworkingSockets_SendMessages": {
        "pMessages": "SteamNetworkingMessage_t[]",
        "pOutMessageNumberOrResult": "long[]",
    },

    "ISteamNetworkingSockets_GetConnectionRealTimeStatus": {
        "pStatus": "ref SteamNetConnectionRealTimeStatus_t",
        "pLanes": "ref SteamNetConnectionRealTimeLaneStatus_t",
    },
    "ISteamGameServerNetworkingSockets_GetConnectionRealTimeStatus": {
        "pStatus": "ref SteamNetConnectionRealTimeStatus_t",
        "pLanes": "ref SteamNetConnectionRealTimeLaneStatus_t",
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

    "ISteamClient_SetLocalIPBinding": {
        "unIP": "ref SteamIPAddress_t",
    },
    "ISteamGameServerClient_SetLocalIPBinding": {
        "unIP": "ref SteamIPAddress_t",
    },
}

g_FixedAttributeValues = {
    "ISteamInventory_GetItemsWithPrices": {
        "pArrayItemDefs": {
            "STEAM_OUT_ARRAY_COUNT": "unArrayLength"
        },
        "pCurrentPrices": {
            "STEAM_OUT_ARRAY_COUNT": "unArrayLength"
        },
        "pBasePrices": {
            "STEAM_OUT_ARRAY_COUNT": "unArrayLength"
        },
    },
    "ISteamGameServerInventory_GetItemsWithPrices": {
        "pArrayItemDefs": {
            "STEAM_OUT_ARRAY_COUNT": "unArrayLength"
        },
        "pCurrentPrices": {
            "STEAM_OUT_ARRAY_COUNT": "unArrayLength"
        },
        "pBasePrices": {
            "STEAM_OUT_ARRAY_COUNT": "unArrayLength"
        },
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
    "NULL": "null",
    "nullptr": "null",
}

HEADER = None

g_NativeMethods = []
g_Output = []
g_Typedefs = None

def main(parser):
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
        parse(f)

    with open("../com.rlabrecque.steamworks.net/Runtime/autogen/NativeMethods.cs", "wb") as out:
        #out.write(bytes(HEADER, "utf-8"))
        with open("templates/nativemethods.txt", "r") as f:
            out.write(bytes(f.read(), "utf-8"))
        for line in g_NativeMethods:
            out.write(bytes(line + "\n", "utf-8"))
        out.write(bytes("\t}\n", "utf-8"))
        out.write(bytes("}\n\n", "utf-8"))
        out.write(bytes("#endif // !DISABLESTEAMWORKS\n", "utf-8"))


def parse(f):
    if f.name in g_SkippedFiles:
        return

    print("File: " + f.name)

    del g_Output[:]
    for interface in f.interfaces:
        parse_interface(f, interface)

    if g_Output:
        with open('../com.rlabrecque.steamworks.net/Runtime/autogen/' + os.path.splitext(f.name)[0] + '.cs', 'wb') as out:
            if f.name in ["isteamnetworkingutils.h", "isteamnetworkingsockets.h", "isteamgameservernetworkingutils.h", "isteamgameservernetworkingsockets.h"]:
                out.write(bytes("#define STEAMNETWORKINGSOCKETS_ENABLE_SDR\n", "utf-8"))
            out.write(bytes(HEADER, "utf-8"))
            out.write(bytes("namespace Steamworks {\n", "utf-8"))
            for line in g_Output:
                out.write(bytes(line + "\n", "utf-8"))
            out.write(bytes("}\n\n", "utf-8"))  # Namespace
            out.write(bytes("#endif // !DISABLESTEAMWORKS\n", "utf-8"))


def parse_interface(f, interface):
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

        parse_func(f, interface, func)

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

def parse_func(f, interface, func):
    strEntryPoint = interface.name + '_' + func.name

    for attr in func.attributes:
        if attr.name == "STEAM_FLAT_NAME":
            strEntryPoint = interface.name + '_' + attr.value
            break

    if "GameServer" in interface.name and interface.name != "ISteamGameServer" and interface.name != "ISteamGameServerStats":
        bGameServerVersion = True
    else:
        bGameServerVersion = False

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

    args = parse_args(strEntryPoint, func.args)
    pinvokeargs = args[0]  # TODO: NamedTuple
    wrapperargs = args[1]
    argnames = args[2]
    stringargs = args[3]
    outstringargs = args[4][0]
    outstringsize = args[4][1]
    args_with_explicit_count = args[5]

    if not bGameServerVersion:
        g_NativeMethods.append("\t\t[DllImport(NativeLibraryName, EntryPoint = \"SteamAPI_{0}\", CallingConvention = CallingConvention.Cdecl)]".format(strEntryPoint))

        if returntype == "bool":
            g_NativeMethods.append("\t\t[return: MarshalAs(UnmanagedType.I1)]")

        g_NativeMethods.append("\t\tpublic static extern {0} {1}({2});".format(returntype, strEntryPoint, pinvokeargs))
        g_NativeMethods.append("")

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

        for i, a in enumerate(outstringargs):
            if not outstringsize:
                functionBody.append("\t\t\tIntPtr " + a + "2;")
                continue

            cast = ""
            if outstringsize[i].type != "int":
                cast = "(int)"

            functionBody.append("\t\t\tIntPtr " + a + "2 = Marshal.AllocHGlobal(" + cast + outstringsize[i].name + ");")

    indentlevel = "\t\t\t"
    if stringargs:
        indentlevel += "\t"
        for a in stringargs:
            functionBody.append("\t\t\tusing (var " + a + "2 = new InteropHelp.UTF8StringHandle(" + a + "))")

        functionBody[-1] += " {"

    if bGameServerVersion:
        strEntryPoint2 = interface.name.replace("GameServer", "") + '_' + func.name

        for attr in func.attributes:
            if attr.name == "STEAM_FLAT_NAME":
                strEntryPoint2 = interface.name.replace("GameServer", "") + '_' + attr.value
                break
    else:
        strEntryPoint2 = strEntryPoint

    functionBody.append("{0}{1}{2}NativeMethods.{3}({4});".format(indentlevel, strReturnable, strCast, strEntryPoint2, argnames))

    if outstringargs:
        retcmp = "ret != 0"
        if returntype == "bool":
            retcmp = "ret"
        elif returntype == "int":
            retcmp = "ret != -1"
        retcmp = g_SpecialOutStringRetCmp.get(strEntryPoint, retcmp)
        for a in outstringargs:
            if returntype == "void":
                functionBody.append(indentlevel + a + " = InteropHelp.PtrToStringUTF8(" + a + "2);")
            else:
                functionBody.append(indentlevel + a + " = " + retcmp + " ? InteropHelp.PtrToStringUTF8(" + a + "2) : null;")

            if strEntryPoint != "ISteamRemoteStorage_GetUGCDetails":
                functionBody.append(indentlevel + "Marshal.FreeHGlobal(" + a + "2);")

        if returntype != "void":
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

def parse_args(strEntryPoint, args):
    pinvokeargs = "IntPtr instancePtr, "
    wrapperargs = ""
    argnames = ""
    stringargs = []
    outstringargs = []
    outstringsize = []
    args_with_explicit_count = OrderedDict()

    ifacename = strEntryPoint[1:strEntryPoint.index('_')]
    if "GameServer" in ifacename:
        if ifacename != "SteamGameServer" and ifacename != "SteamGameServerStats":
            ifacename = ifacename.replace("GameServer", "")
        argnames = "CSteamGameServerAPIContext.Get" + ifacename + "(), "
    else:
        argnames = "CSteamAPIContext.Get" + ifacename + "(), "

    getsize = False

    for arg in args:
        argtype = g_TypeDict.get(arg.type, arg.type)
        if argtype.endswith("*"):
            potentialtype = arg.type.rstrip("*").lstrip("const ").rstrip()
            argtype = "out " + g_TypeDict.get(potentialtype, potentialtype)
        argtype = g_SpecialArgsDict.get(strEntryPoint, dict()).get(arg.name, argtype)

        if arg.attribute:
            if arg.attribute.name == "STEAM_OUT_ARRAY" or arg.attribute.name == "STEAM_OUT_ARRAY_CALL" or arg.attribute.name == "STEAM_OUT_ARRAY_COUNT" or arg.attribute.name == "STEAM_ARRAY_COUNT" or arg.attribute.name == "STEAM_ARRAY_COUNT_D":
                potentialtype = arg.type.rstrip("*").rstrip()
                argtype = g_TypeDict.get(potentialtype, potentialtype) + "[]"
            #if arg.attribute.name == "OUT_STRING" or arg.attribute.name == "OUT_STRING_COUNT":  #Unused for now

            if arg.attribute.name == "STEAM_OUT_ARRAY_COUNT":
                fixedattrvalue = g_FixedAttributeValues.get(strEntryPoint, dict()).get(arg.name, dict()).get(arg.attribute.name, arg.attribute.value)
                commaindex = fixedattrvalue.find(',')
                if commaindex > 0:
                    args_with_explicit_count[arg.name] = fixedattrvalue[:commaindex]
                else:
                    args_with_explicit_count[arg.name] = fixedattrvalue


        if arg.type == "MatchMakingKeyValuePair_t **":  # TODO: Fixme - Small Hack... We do this because MatchMakingKeyValuePair's have ARRAY_COUNT() and two **'s, things get broken :(
            argtype = "IntPtr"

        # We skip byte[] because it is a primitive type that C# can essentially mmap and get a great perf increase while marshalling.
        # We need to do this for other primitive types eventually but that will require more testing to make sure nothing breaks.
        if argtype.endswith("[]") and argtype != "byte[]":
            argtype = "[In, Out] " + argtype
        elif argtype == "bool":
            argtype = "[MarshalAs(UnmanagedType.I1)] " + argtype

        pinvokeargs += argtype + " " + arg.name + ", "

        argtype = argtype.replace("[In, Out] ", "").replace("[MarshalAs(UnmanagedType.I1)] ", "")
        wrapperargtype = g_WrapperArgsTypeDict.get(arg.type, argtype)
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

        if argtype.startswith("out"):
            argnames += "out "
        elif wrapperargtype.startswith("ref"):
            argnames += "ref "

        if wrapperargtype == "System.Collections.Generic.IList<string>":
            argnames += "new InteropHelp.SteamParamStringArray(" + arg.name + ")"
        elif wrapperargtype == "MatchMakingKeyValuePair_t[]":
            argnames += "new MMKVPMarshaller(" + arg.name + ")"
        elif wrapperargtype.endswith("Response"):
            argnames += "(IntPtr)" + arg.name
        elif arg.name.endswith("Deprecated"):
            if argtype == "IntPtr":
                argnames += "IntPtr.Zero"
            elif argtype == "bool":
                argnames += "false"
            else:
                argnames += "0"
        else:
            argnames += arg.name

        if getsize:
            getsize = False
            outstringsize.append(arg)

        if wrapperargtype == "string":
            stringargs.append(arg.name)
            argnames += "2"
        elif wrapperargtype == "out string":
            outstringargs.append(arg.name)
            argnames += "2"
            if strEntryPoint != "ISteamRemoteStorage_GetUGCDetails":
                getsize = True

        argnames += ", "

    pinvokeargs = pinvokeargs.rstrip(", ")
    wrapperargs = wrapperargs.rstrip(", ")
    argnames = argnames.rstrip(", ")
    return (pinvokeargs, wrapperargs, argnames, stringargs, (outstringargs, outstringsize), args_with_explicit_count)


if __name__ == "__main__":
    if len(sys.argv) != 2:
        print("TODO: Usage Instructions")
        exit()

    steamworksparser.Settings.fake_gameserver_interfaces = True
    main(steamworksparser.parse(sys.argv[1]))
