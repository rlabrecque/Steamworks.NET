Steamworks.NET CodeGen
======================

This is special sauce that generates the autogen/ and types/ directories.

It uses [SteamworksParser](https://github.com/rlabrecque/SteamworksParser) to parse the Steamworks C++ header files, then converts them to C#.

Usage
-----

1. If necessary update the files in `steam/`
2. Open a command prompt to the CodeGen directory. (The script must be run from this directory or the relative paths will be broken.)
3. Run `python3 Steamworks.NET_CodeGen.py`, preferably on a linux based OS to generate proper line endings. (I use WSL for this.)
