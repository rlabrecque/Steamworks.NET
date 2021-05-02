import sys
from SteamworksParser import steamworksparser
import interfaces
import constants
import enums
import structs
import typedefs
import output_dummy_files

def main():
    if len(sys.argv) == 2:
        path = sys.argv[1]
    elif len(sys.argv) == 1:
        path = "steam"
    else:
        print("Usage: Steamworks.NET_CodeGen.py [path/to/sdk/public/steam]")
        print("       If a path is not included then a steam/ folder must exist within the cwd.")
        return

    steamworksparser.Settings.fake_gameserver_interfaces = True
    ___parser = steamworksparser.parse(path)

    interfaces.main(___parser)
    constants.main(___parser)
    enums.main(___parser)
    structs.main(___parser)
    typedefs.main(___parser)
    output_dummy_files.main()

if __name__ == "__main__":
    main()
