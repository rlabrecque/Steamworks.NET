from steamworksparser import parse, Settings

# Settings.print_skippedtypedefs = True
# Settings.print_unuseddefines = True
# Settings.warn_spacing = True

parser = parse("./steamtest") # put steam headers inside



with open("bin/pack-size-test.cpp", "w", encoding="utf-8") as f:
    f.write("#include <iostream>\n")
    f.write("#include \"steam_api.h\"\n")
    f.write("#include \"steam_gameserver.h\"\n")
    f.write("#include \"isteamgamecoordinator.h\"\n")

    f.write("int main() {\n")
    structInspectionTemplate = "std::cout << \"{0}\" << '\t' << sizeof({0}) << '\\t' << alignof({0}) << '\\n';\n"
    f.write("std::cout << std::ios::binary;\n")  
    for interface in parser.files:
        for s in interface.callbacks:
            f.write(structInspectionTemplate.format(s.name))
        for s in interface.structs:
            if not s.should_not_generate():
                f.write(structInspectionTemplate.format(s.name))

    f.write('}')

    # generated file still need some fix at 25/11/19