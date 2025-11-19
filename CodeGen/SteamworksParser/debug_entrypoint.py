from steamworksparser import parse, Settings

# Settings.print_skippedtypedefs = True
# Settings.print_unuseddefines = True
# Settings.warn_spacing = True
Settings.print_debug = True

parse("./steamtest") # put steam headers inside
