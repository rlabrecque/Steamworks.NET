import os
import sys
from SteamworksParser import steamworksparser

g_FlagEnums = (
    # ISteamFriends
    "EPersonaChange",
    "EFriendFlags",

    #ISteamHTMLSurface
    "EHTMLKeyModifiers",

    #ISteamInput
    "EControllerHapticLocation",

    #ISteamInventory
    "ESteamItemFlags",

    # ISteamMatchmaking
    "EChatMemberStateChange",

    # ISteamRemoteStorage
    "ERemoteStoragePlatform",

    # ISteamUGC
    "EItemState",

    # SteamClientPublic
    "EChatSteamIDInstanceFlags",
    "EMarketNotAllowedReasonFlags",
)

g_SkippedEnums = {
    # This is defined within CGameID and we handwrote CGameID including this.
    "EGameIDType": "steamclientpublic.h",

    # Valve redefined these twice, and ifdef decided which one to use. :(
    # We use the newer ones from isteaminput.h and skip the ones in
    # isteamcontroller.h because it is deprecated.
    "EXboxOrigin": "isteamcontroller.h",
    "ESteamInputType": "isteamcontroller.h",
}

g_ValueConversionDict = {
    "0xffffffff": "-1",
    "0x80000000": "-2147483647",
    "k_unSteamAccountInstanceMask": "Constants.k_unSteamAccountInstanceMask",
    "( 1 << k_ESteamControllerPad_Left )": "( 1 << ESteamControllerPad.k_ESteamControllerPad_Left )",
    "( 1 << k_ESteamControllerPad_Right )": "( 1 << ESteamControllerPad.k_ESteamControllerPad_Right )",
    "( 1 << k_ESteamControllerPad_Left | 1 << k_ESteamControllerPad_Right )": "( 1 << ESteamControllerPad.k_ESteamControllerPad_Left | 1 << ESteamControllerPad.k_ESteamControllerPad_Right )",
}

def main(parser):
    try:
        os.makedirs("../com.rlabrecque.steamworks.net/Runtime/autogen/")
    except OSError:
        pass

    lines = []
    for f in parser.files:
        for enum in f.enums:
            if enum.name in g_SkippedEnums and g_SkippedEnums[enum.name] == f.name:
                continue

            for comment in enum.c.rawprecomments:
                if type(comment) is steamworksparser.BlankLine:
                    continue
                lines.append("\t" + comment)

            if enum.name in g_FlagEnums:
                lines.append("\t[Flags]")

            lines.append("\tpublic enum " + enum.name + " : int {")

            for field in enum.fields:
                for comment in field.c.rawprecomments:
                    if type(comment) is steamworksparser.BlankLine:
                        lines.append("")
                    else:
                        lines.append("\t" + comment)
                line = "\t\t" + field.name
                if field.value:
                    if "<<" in field.value and enum.name not in g_FlagEnums:
                        print("[WARNING] Enum " + enum.name + " contains '<<' but is not marked as a flag! - " + f.name)

                    if field.value == "=" or field.value == "|":
                        line += " "
                    else:
                        line += field.prespacing + "=" + field.postspacing

                    value = field.value
                    for substring in g_ValueConversionDict:
                        if substring in field.value:
                            value = value.replace(substring, g_ValueConversionDict[substring], 1)
                            break

                    line += value
                if field.c.rawlinecomment:
                    line += field.c.rawlinecomment
                lines.append(line)

            for comment in enum.endcomments.rawprecomments:
                if type(comment) is steamworksparser.BlankLine:
                    lines.append("")
                else:
                    lines.append("\t" + comment)

            lines.append("\t}")
            lines.append("")

    with open("../com.rlabrecque.steamworks.net/Runtime/autogen/SteamEnums.cs", "wb") as out:
        with open("templates/header.txt", "r") as f:
            out.write(bytes(f.read(), "utf-8"))
        out.write(bytes("using Flags = System.FlagsAttribute;\n\n", "utf-8"))
        out.write(bytes("namespace Steamworks {\n", "utf-8"))
        for line in lines:
            out.write(bytes(line + "\n", "utf-8"))
        out.write(bytes("}\n\n", "utf-8"))
        out.write(bytes("#endif // !DISABLESTEAMWORKS\n", "utf-8"))

if __name__ == "__main__":
    if len(sys.argv) != 2:
        print("TODO: Usage Instructions")
        exit()

    steamworksparser.Settings.fake_gameserver_interfaces = True
    main(steamworksparser.parse(sys.argv[1]))