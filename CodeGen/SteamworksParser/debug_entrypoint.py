from steamworksparser import parse, Settings
import os
import json
import itertools
# Settings.print_skippedtypedefs = True
# Settings.print_unuseddefines = True
# Settings.warn_spacing = True
Settings.print_debug = True

parser = parse("./steamtest") # put steam headers inside

os.makedirs("bin/compare-official")
os.makedirs("bin/native")

with open("bin/native/pack-size-test.cpp", "w", encoding="utf-8") as f:
    f.write("#include <iostream>\n")
    f.write("#include \"steam_api.h\"\n")
    f.write("#include \"steam_gameserver.h\"\n")
    f.write("#include \"isteamgamecoordinator.h\"\n")
    f.write("#include \"steamnetworkingfakeip.h\"\n")
    f.write("#ifdef _MSC_VER\n")
    f.write("#include \"fcntl.h\"\n")
    f.write("#include \"io.h\"\n")
    f.write("#endif\n")

    f.write("int main() {\n")
    f.write("#ifdef _MSC_VER\n")
    f.write("fflush(stdout);\n")
    f.write("_setmode(_fileno(stdout), _O_BINARY);\n")
    f.write("#endif\n")
    structInspectionTemplate = "std::cout << \"{0}\" << '\\t' << sizeof({0}) << '\\t' << alignof({0}) << '\\n';\n"
    for interface in parser.files:
        for s in interface.callbacks:
            f.write(structInspectionTemplate.format(s.name))
        for s in interface.structs:
            if not s.should_not_generate():
                f.write(structInspectionTemplate.format(s.name))

    f.write('}')

    # generated file still need some fix at 25/11/19