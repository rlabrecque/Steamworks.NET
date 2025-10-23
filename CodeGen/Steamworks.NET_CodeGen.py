from SteamworksParser import steamworksparser
from src import interfaces
from src import constants
from src import enums
from src import structs
from src import typedefs

def main():
    steam_path = "steam/"

    steamworksparser.Settings.fake_gameserver_interfaces = True
    ___parser = steamworksparser.parse(steam_path)

    interfaces.main(___parser)
    constants.main(___parser)
    enums.main(___parser)
    structs.main(___parser)
    typedefs.main(___parser)

if __name__ == "__main__":
    main()
