import os
import sys
from collections import OrderedDict
from SteamworksParser import steamworksparser

g_PrettyFilenames = {
    "SteamClientpublic": "SteamClientPublic",
    "SteamHtmlsurface": "SteamHTMLSurface",
    "SteamHttp": "SteamHTTP",
    "SteamRemotestorage": "SteamRemoteStorage",
    "SteamUgc": "SteamUGC",
    "SteamUserstats": "SteamUserStats",
    "SteamRemoteplay": "SteamRemotePlay",
}

g_TypeDict = {
    "int16": "short",
    "int32": "int",
    "int64": "long",
    "uint32": "uint",
    "uint64": "ulong",
    "void*": "System.IntPtr",
}

g_UnusedTypedefs = [
    # SteamTypes
    "int8",
    "int16",
    "int32",
    "int64",
    "intp",
    "lint64",
    "uint8",
    "uint16",
    "uint32",
    "uint64",
    "uintp",
    "ulint64",
]

g_ReadOnlyValues = {
    "HAuthTicket": OrderedDict([
        ("Invalid", "0"),
    ]),

    "FriendsGroupID_t": OrderedDict([
        ("Invalid", "-1"),
    ]),

    "HHTMLBrowser": OrderedDict([
        ("Invalid", "0"),
    ]),

    "HTTPCookieContainerHandle": OrderedDict([
        ("Invalid", "0"),
    ]),

    "HTTPRequestHandle": OrderedDict([
        ("Invalid", "0"),
    ]),

    "SteamInventoryResult_t": OrderedDict([
        ("Invalid", "-1"),
    ]),

    "SteamItemInstanceID_t": OrderedDict([
        ("Invalid", "0xFFFFFFFFFFFFFFFF"),
    ]),

    "HServerListRequest": OrderedDict([
        ("Invalid", "System.IntPtr.Zero"),
    ]),

    "HServerQuery": OrderedDict([
        ("Invalid", "-1"),
    ]),

    "PublishedFileId_t": OrderedDict([
        ("Invalid", "0"),
    ]),

    "PublishedFileUpdateHandle_t": OrderedDict([
        ("Invalid", "0xffffffffffffffff"),
    ]),

    "UGCFileWriteStreamHandle_t": OrderedDict([
        ("Invalid", "0xffffffffffffffff"),
    ]),

    "UGCHandle_t": OrderedDict([
        ("Invalid", "0xffffffffffffffff"),
    ]),

    "ScreenshotHandle": OrderedDict([
        ("Invalid", "0"),
    ]),

    "AppId_t": OrderedDict([
        ("Invalid", "0x0"),
    ]),

    "DepotId_t": OrderedDict([
        ("Invalid", "0x0"),
    ]),

    "SteamAPICall_t": OrderedDict([
        ("Invalid", "0x0"),
    ]),

    "UGCQueryHandle_t": OrderedDict([
        ("Invalid", "0xffffffffffffffff"),
    ]),

    "UGCUpdateHandle_t": OrderedDict([
        ("Invalid", "0xffffffffffffffff"),
    ]),

    "ClientUnifiedMessageHandle": OrderedDict([
        ("Invalid", "0"),
    ]),

    "SiteId_t": OrderedDict([
        ("Invalid", "0"),
    ]),

    "SteamInventoryUpdateHandle_t": OrderedDict([
        ("Invalid", "0xffffffffffffffff"),
    ]),

    "PartyBeaconID_t": OrderedDict([
        ("Invalid", "0"),
    ]),

    "HSteamNetConnection": OrderedDict([
        ("Invalid", "0"),
    ]),

    "HSteamListenSocket": OrderedDict([
        ("Invalid", "0"),
    ]),

    "HSteamNetPollGroup": OrderedDict([
        ("Invalid", "0"),
    ]),
}


def main(parser):
    with open("templates/header.txt", "r") as f:
        HEADER = f.read()

    with open("templates/typetemplate.txt", "r") as f:
        template = f.read()

    for root, directories, filenames in os.walk('templates/custom_types/'):
        for filename in filenames:
            outputdir = "../com.rlabrecque.steamworks.net/Runtime/types/" + root[len('templates/custom_types/'):]
            try:
                os.makedirs(outputdir)
            except OSError:
                pass

            with open(os.path.join(outputdir, filename), "wb") as out:
                out.write(bytes(HEADER, "utf-8"))
                with open(os.path.join(root, filename), "r") as customType:
                    out.write(bytes(customType.read(), "utf-8"))

    for t in parser.typedefs:
        if t.name in g_UnusedTypedefs:
            continue

        readonly = ""
        try:
            for k, v in g_ReadOnlyValues[t.name].items():
                readonly += "\t\tpublic static readonly " + t.name + " " + k + " = new " + t.name + "(" + v + ");\n"
        except KeyError:
            pass

        ourtemplate = template
        ourtype = g_TypeDict.get(t.type, t.type)
        if ourtype == "System.IntPtr":
            ourtemplate = ourtemplate.replace(", System.IComparable<{NAME}>", "", 1)
            ourtemplate = ourtemplate.replace("""
\t\tpublic int CompareTo({NAME} other) {
\t\t\treturn m_{NAMESTRIPPED}.CompareTo(other.m_{NAMESTRIPPED});
\t\t}
""", "", 1)

        ourtemplate = ourtemplate.replace("{NAME}", t.name)
        ourtemplate = ourtemplate.replace("{NAMESTRIPPED}", t.name.replace("_t", "", 1))
        ourtemplate = ourtemplate.replace("{TYPE}", ourtype)
        ourtemplate = ourtemplate.replace("{READONLY}\n", readonly)

        foldername = os.path.splitext(t.filename)[0]
        foldername = foldername.replace("isteam", "steam", 1)
        foldername = "Steam" + foldername[len("Steam"):].capitalize()
        foldername = g_PrettyFilenames.get(foldername, foldername)

        try:
            os.makedirs("../com.rlabrecque.steamworks.net/Runtime/types/" + foldername)
        except OSError:
            pass

        with open("../com.rlabrecque.steamworks.net/Runtime/types/" + foldername + "/" + t.name + ".cs", "wb") as out:
            out.write(bytes(HEADER, "utf-8"))
            out.write(bytes(ourtemplate, "utf-8"))

if __name__ == "__main__":
    if len(sys.argv) != 2:
        print("TODO: Usage Instructions")
        exit()

    steamworksparser.Settings.fake_gameserver_interfaces = True
    main(steamworksparser.parse(sys.argv[1]))
