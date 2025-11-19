import os
import codecs
import copy
import re

g_SkippedFiles = (
    "steam_api_flat.h", # Valve's C API

    # PS3-only Headers not currently supported
    "isteamps3overlayrenderer.h",
    "steamps3params.h",

    # Deprecated, moved to isteaminput.h
    "isteamcontroller.h",

    # Non-steam code
    "isteamdualsense.h",
)

g_SkippedLines = (
    # steamtypes.h
    "STEAM_CLANG_ATTR",
    "#define VALVE_BIG_ENDIAN",

    # Multiple
    "public:",
    "private:",
    "protected:",
    "_STEAM_CALLBACK_",
    "#define STEAM_CALLBACK_BEGIN",
    "#define STEAM_CALLBACK_END",
    "#define STEAM_CALLBACK_MEMBER",
    "STEAM_DEFINE_INTERFACE_ACCESSOR",
)

g_SkippedStructs = (
    # steamnetworkingtypes.h
    "SteamNetworkingIPAddr",
    "SteamNetworkingIdentity",
    "SteamNetworkingMessage_t",
    "SteamNetworkingConfigValue_t",

    # steamdatagram_tickets.h
    "SteamDatagramHostedAddress",
    "SteamDatagramRelayAuthTicket",
)

g_FuncAttribs = (
    "STEAM_METHOD_DESC",
    "STEAM_IGNOREATTR",
    "STEAM_CALL_RESULT",
    "STEAM_CALL_BACK",
    "STEAM_FLAT_NAME",
)

g_ArgAttribs = (
    "STEAM_ARRAY_COUNT",
    "STEAM_ARRAY_COUNT_D",
    "STEAM_BUFFER_COUNT",
    "STEAM_DESC",
    "STEAM_OUT_ARRAY_CALL",
    "STEAM_OUT_ARRAY_COUNT",
    "STEAM_OUT_BUFFER_COUNT",
    "STEAM_OUT_STRING",
    "STEAM_OUT_STRING_COUNT",
    "STEAM_OUT_STRUCT",
)

g_GameServerInterfaces = (
    'isteamclient.h',
    #'isteamgameserver.h',
    #'isteamgameserverstats.h',
    'isteamhttp.h',
    'isteaminventory.h',
    'isteamnetworking.h',
    'isteamnetworkingmessages.h',
    'isteamnetworkingsockets.h',
    'isteamnetworkingutils.h',
    'isteamugc.h',
    'isteamutils.h',
)

class Settings:
    warn_utf8bom = False
    warn_includeguardname = False
    warn_spacing = False
    print_unuseddefines = False
    print_skippedtypedefs = False
    fake_gameserver_interfaces = False

class BlankLine(object):
    pass # linenum?

class Comment:
    def __init__(self, rawcomments, comments, rawlinecomment, linecomment):
        self.rawprecomments = rawcomments
        self.precomments = comments
        self.rawlinecomment = rawlinecomment
        self.linecomment = linecomment

class ArgAttribute:
    def __init__(self, name="", value=""):
        self.name = name
        self.value = value

class Arg:
    def __init__(self, name="", type_="", default=None, attribute=None):
        self.name = name
        self.type = type_
        self.default = default
        self.attribute = attribute  # ArgAttribute

class FunctionAttribute:
    def __init__(self):
        self.name = ""
        self.value = ""

class Function:
    def __init__(self):
        self.name = ""
        self.returntype = ""
        self.args = []  # Arg
        self.ifstatements = []
        self.comments = []
        self.linecomment = ""
        self.attributes = []  # FunctionAttribute
        self.private = False

class Interface:
    def __init__(self):
        self.name = ""
        self.functions = []  # Function
        self.c = None  # Comment

class Define:
    def __init__(self, name, value, spacing, comments):
        self.name = name
        self.value = value
        self.spacing = spacing
        self.c = comments

class Constant:
    def __init__(self, name, value, type_, comments):
        self.name = name
        self.value = value
        self.type = type_
        self.c = comments  # Comment

class EnumField:
    def __init__(self):
        self.name = ""
        self.value = ""
        self.prespacing = " "
        self.postspacing = " "
        self.c = None  # Comment

class Enum:
    def __init__(self, name, comments):
        self.name = name
        self.fields = []  # EnumField
        self.c = comments
        self.endcomments = None  # Comment

class Struct:
    def __init__(self, name, packsize, comments):
        self.name = name
        self.packsize = packsize
        self.c = comments  # Comment
        self.fields = []  # StructField
        self.callbackid = None
        self.endcomments = None  # Comment

class StructField:
    def __init__(self, name, typee, arraysize, comments):
        self.name = name
        self.type = typee
        self.arraysize = arraysize
        self.c = comments  # Comment

class Typedef:
    def __init__(self, name, typee, filename, comments):
        self.name = name
        self.type = typee
        self.filename = filename
        self.c = comments

class SteamFile:
    def __init__(self, name):
        self.name = name
        self.header = []
        self.includes = []
        self.defines = []  # Define
        self.constants = []  # Constant
        self.enums = []  # Enum
        self.structs = []  # Struct
        self.callbacks = [] # Struct
        self.interfaces = []  # Interface
        self.typedefs = []  # Typedef

class ParserState:
    def __init__(self, file):
        self.f = file  # SteamFile
        self.lines = []
        self.line = ""
        self.originalline = ""
        self.linesplit = []
        self.linenum = 0
        self.rawcomments = []
        self.comments = []
        self.rawlinecomment = None
        self.linecomment = None
        self.ifstatements = []
        self.packsize = []
        self.funcState = 0
        self.scopeDepth = 0

        self.interface = None
        self.function = None
        self.enum = None
        self.struct = None
        self.callbackmacro = None

        self.bInHeader = True
        self.bInMultilineComment = False
        self.bInMultilineMacro = False
        self.bInPrivate = False
        self.callbackid = None
        self.functionAttributes = [] # FunctionAttribute

class Parser:
    files = None
    typedefs = []

    def __init__(self, folder):
        self.files = [SteamFile(f) for f in os.listdir(folder) if os.path.isfile(os.path.join(folder, f)) and f.endswith(".h") and f not in g_SkippedFiles]
        self.files
        self.files.sort(key=lambda f: f.name)

        self.typedefs = []

        for f in self.files:
            s = ParserState(f)
            filepath = os.path.join(folder, f.name)
            with open(filepath, 'r', encoding="latin-1") as infile:
                s.lines = infile.readlines()

                if s.lines[0][:3] == codecs.BOM_UTF8:
                    s.lines[0] = s.lines[0][3:]
                    if Settings.warn_utf8bom:
                        printWarning("File contains a UTF8 BOM.", s)

                self.parse(s)

        # Hack to give us the GameServer interfaces.
        # We want this for autogen but probably don't want it for anything else.
        if Settings.fake_gameserver_interfaces:
            for f in [f for f in self.files if f.name in g_GameServerInterfaces]:
                gs_f = SteamFile(f.name.replace("isteam", "isteamgameserver", 1))
                gs_f.interfaces = copy.deepcopy(f.interfaces)
                for i in gs_f.interfaces:
                    i.name = i.name.replace("ISteam", "ISteamGameServer", 1)
                self.files.append(gs_f)

    def parse(self, s):
        for linenum, line in enumerate(s.lines):
            s.line = line
            s.originalline = line
            s.linenum = linenum

            s.line = s.line.rstrip()

            self.parse_comments(s)

            # Comments get removed from the line, often leaving blank lines, thus we do this after parsing comments
            if not s.line:
                continue

            s.linesplit = s.line.split()

            if s.bInHeader:
                self.parse_header(s)

            if self.parse_skippedlines(s):
                self.consume_comments(s)
                continue

            self.parse_preprocessor(s)
            self.parse_typedefs(s)
            self.parse_constants(s)
            self.parse_enums(s)
            self.parse_structs(s)
            self.parse_callbackmacros(s)
            self.parse_interfaces(s)
            if not s.line:
                continue

            self.parse_classes(s)
            self.parse_scope(s)

    def parse_comments(self, s):
        self.parse_comments_multiline(s)
        self.parse_comments_singleline(s)
        s.line = s.line.strip()

    def parse_comments_multiline(self, s):
        strComment = None
        multilineOpenerPos = s.line.find("/*")
        bHasOpening = (multilineOpenerPos != -1)
        multilineCloserPos = s.line.find("*/")
        bHasClosing = (multilineCloserPos != -1)

        multipleQuoteblocks = False
        if s.line.count("/*") > 1 or s.line.count("*/") > 1:
            multipleQuoteblocks = True

        # TODO - Ugly Code that works well
        if bHasOpening:
            if bHasClosing:
                strComment = s.line[multilineOpenerPos+2:multilineCloserPos]
                s.line = s.line[:multilineOpenerPos] + s.line[multilineCloserPos+2:]
                s.bInMultilineComment = False
            else:
                strComment = s.line[multilineOpenerPos+2:]
                s.line = s.line[:multilineOpenerPos]
                s.bInMultilineComment = True
        elif s.bInMultilineComment:
            if bHasClosing:
                strComment = s.line[:multilineCloserPos]
                s.line = s.line[multilineCloserPos+2:]
                s.bInMultilineComment = False
            else:
                strComment = s.line
                s.line = ""

        if strComment is not None:
            s.comments.append(strComment.rstrip())

        if multipleQuoteblocks:
            self.parse_comments_multiline(s)

    def parse_comments_singleline(self, s):
        if s.linecomment is not None:
            s.comments.append(s.linecomment)
            s.rawcomments.append(s.rawlinecomment)
            s.rawlinecomment = None
            s.linecomment = None

        if not s.line:
            s.rawcomments.append(BlankLine())
            return

        commentPos = s.line.find("//")

        if commentPos != -1:
            s.linecomment = s.line[commentPos+2:]
            s.line = s.line[:commentPos]

            commentPos = s.originalline.index("//")
            whitespace = len(s.originalline[:commentPos]) - len(s.originalline[:commentPos].rstrip())
            startpos = commentPos - whitespace
            s.rawlinecomment = s.originalline[startpos:].rstrip()

    def parse_header(self, s):
        if s.line:
            s.f.header.extend(s.comments)
            s.comments = []
            s.bInHeader = False

    def parse_skippedlines(self, s):
        if s.struct and s.struct.name in g_SkippedStructs:
            self.parse_scope(s)
            if s.line == "};":
                if s.scopeDepth == 0:
                    s.struct = None
            return True

        if "!defined(API_GEN)" in s.ifstatements:
            if s.line.startswith("#if"):
                s.ifstatements.append("ugh")
            elif s.line.startswith("#endif"):
                s.ifstatements.pop()
            return True

        if s.line.endswith("\\"):
            s.bInMultilineMacro = True
            return True

        if s.bInMultilineMacro:
            s.bInMultilineMacro = False
            return True

        for skip in g_SkippedLines:
            if skip in s.line:
                return True

        if not s.interface and 'inline' in s.line:
            return True

        return False

    def parse_preprocessor(self, s):
        if not s.line.startswith("#"):
            return

        elif s.line.startswith("#else"):
            previf = s.ifstatements[-1]
            s.ifstatements.pop()
            s.ifstatements.append("!(" + previf + ") // #else")
        elif s.line.startswith("#include"):
            self.consume_comments(s)
            includefile = s.linesplit[1]
            includefile = includefile[1:-1]  # Trim the "" or <>
            s.f.includes.append(includefile)
        elif s.line.startswith("#ifdef"):
            token = s.linesplit[1]
            s.ifstatements.append("defined(" + token + ")")
        elif s.line.startswith("#ifndef"):
            token = s.linesplit[1]
            s.ifstatements.append("!defined(" + token + ")")
        elif s.line.startswith("#if"):
            s.ifstatements.append(s.line[3:].strip())
        elif s.line.startswith("#endif"):
            s.ifstatements.pop()
        elif s.line.startswith("#define"):
            comments = self.consume_comments(s)
            if Settings.warn_includeguardname:
                if not s.ifstatements:
                    if s.linesplit[1] != s.f.name.upper().replace(".", "_"):
                        printWarning("Include guard does not match the file name.", s)

            if len(s.linesplit) > 2:
                spacing = s.line[s.line.index(s.linesplit[1]) + len(s.linesplit[1]):s.line.index(s.linesplit[2])]
                s.f.defines.append(Define(s.linesplit[1], s.linesplit[2], spacing, comments))
            elif Settings.print_unuseddefines:
                print("Unused Define: " + s.line)
        elif s.line.startswith("#pragma pack"):
            if "push" in s.line:
                tmpline = s.line[s.line.index(",")+1:-1].strip()
                s.packsize.append(int(tmpline))
            elif "pop" in s.line:
                s.packsize.pop
        elif s.line.startswith("#pragma"):
            pass
        elif s.line.startswith("#error"):
            pass
        elif s.line.startswith("#warning"):
            pass
        elif s.line.startswith("#elif"):
            pass
        elif s.line.startswith("#undef"):
            pass
        else:
            printUnhandled("Preprocessor", s)


    def parse_typedefs(self, s):
        if s.linesplit[0] != "typedef":
            return

        comments = self.consume_comments(s)

        # Skips typedefs in the Callback/CallResult classes
        if s.scopeDepth > 0:
            if Settings.print_skippedtypedefs:
                print("Skipped typedef because it's in a class or struct: " + s.line)
            return

        # Skips typedefs that we don't currently support, So far they are all function pointers.
        if "(" in s.line or "[" in s.line:
            if Settings.print_skippedtypedefs:
                print("Skipped typedef because it contains '(' or '[': " + s.line)
            return

        # Currently skips typedef struct ValvePackingSentinel_t
        if not s.line.endswith(";"):
            if Settings.print_skippedtypedefs:
                print("Skipped typedef because it does not end with ';': " + s.line)
            return

        name = s.linesplit[-1].rstrip(";")
        typee = " ".join(s.linesplit[1:-1])
        if name.startswith("*"):
            typee += " *"
            name = name[1:]

        typedef = Typedef(name, typee, s.f.name, comments)

        self.typedefs.append(typedef)
        s.f.typedefs.append(typedef)


    def parse_constants(self, s):
        if s.linesplit[0] != "const" and not s.line.startswith("static const"):
            return

        if s.scopeDepth > 1:
            return

        comments = self.consume_comments(s)

        # Currently skips one unfortunate function definition where the first arg on the new line starts with const. Like so:
        # void func(void arg1,
        #    const arg2) = 0;
        if "=" not in s.linesplit:
            return

        result = re.match(r".*const\s+(.*)\s+(\w+)\s+=\s+(.*);$", s.line)

        if not result:
            return

        constant = Constant(result.group(2), result.group(3), result.group(1), comments);
        s.f.constants.append(constant)

    def parse_enums(self, s):
        if s.enum:
            if s.line == "{":
                return

            if s.line.endswith("};"):
                # Hack to get comments between the last field and }; :(
                s.enum.endcomments = self.consume_comments(s)
                # Don't append unnamed (constant) enums
                if s.enum.name is not None:
                    s.f.enums.append(s.enum)

                s.enum = None
                return

            self.parse_enumfields(s)
            return

        if s.linesplit[0] != "enum":
            return

        comments = self.consume_comments(s)

        # Actually a constant like: enum { k_name = value };
        if "};" in s.linesplit:
            # Skips lines like: "enum { k_name1 = value, k_name2 = value };"
            # Currently only skips one enum in CCallbackBase
            if "," in s.line:
                return

            # Skips lines in macros
            # Currently only skips one enum in DEFINE_CALLBACK
            if s.linesplit[-1] == "\\":
                return

            if s.struct:
                result = re.match("^enum { (.*) = (.*) };", s.line)
                name = result.group(1)

                if name == "k_iCallback":
                    s.callbackid = result.group(2)
                    return

            constant = Constant(s.linesplit[2], s.linesplit[4], "int", comments);
            s.f.constants.append(constant)
            return

        if len(s.linesplit) == 1:
            s.enum = Enum(None, comments)
            # unnamed Constants like:
            '''enum {
                k_name1 = value,
                k_name2 = value,
            };'''
            return

        s.enum = Enum(s.linesplit[1], comments)

    def parse_enumfields(self, s):
        result = re.match(r"^(\w+,?)([ \t]*)=?([ \t]*)(.*)$", s.line)
        comments = self.consume_comments(s)

        # HACK: This is a hack for multiline fields :(
        if s.line.endswith("="):
            value = "="
        else:
            value = result.group(4)

        # Nameless Enums are actually just constants
        if s.enum.name is None:
            if s.enum.c:
                comments.precomments = s.enum.c.precomments
                s.enum.c = None
            constant = Constant(result.group(1), value.rstrip(","), "int", comments)
            s.f.constants.append(constant)
            return

        field = EnumField()
        field.name = result.group(1)

        if value:
            field.prespacing = result.group(2)
            field.postspacing = result.group(3)
            field.value = value

        field.c = comments
        s.enum.fields.append(field)

    def parse_structs(self, s):
        if s.enum:
            return

        if s.struct:
            if s.line == "};":
                if s.scopeDepth != 1:
                    return

                s.struct.endcomments = self.consume_comments(s)

                if s.callbackid:
                    s.struct.callbackid = s.callbackid
                    s.f.callbacks.append(s.struct)
                    s.callbackid = None
                else:
                    s.f.structs.append(s.struct)

                s.struct = None
            else:
                self.parse_struct_fields(s)

            return

        if s.linesplit[0] != "struct":
            return

        # Skip Forward Declares
        if s.linesplit[1].endswith(";"):
            return

        comments = self.consume_comments(s)

        # Skip SteamIDComponent_t and GameID_t which are a part of CSteamID/CGameID
        if s.scopeDepth != 0:
            return

        s.struct = Struct(s.linesplit[1], s.packsize, comments)

    def parse_struct_fields(self, s):
        comments = self.consume_comments(s)

        if s.line.startswith("enum"):
            return

        if s.line == "{":
            return

        fieldarraysize = None
        result = re.match(r"^([^=.]*\s\**)(\w+);$", s.line)
        if result is None:
            result = re.match(r"^(.*\s\*?)(\w+)\[\s*(\w+)?\s*\];$", s.line)
            if result is None:
                return

            fieldarraysize = result.group(3)

        fieldtype = result.group(1).rstrip()
        fieldname = result.group(2)

        s.struct.fields.append(StructField(fieldname, fieldtype, fieldarraysize, comments))

    def parse_callbackmacros(self, s):
        if s.callbackmacro:
            comments = self.consume_comments(s)
            if s.line.startswith("STEAM_CALLBACK_END("):
                s.f.callbacks.append(s.callbackmacro)
                s.callbackmacro = None
            elif s.line.startswith("STEAM_CALLBACK_MEMBER_ARRAY"):
                result = re.match(r"^STEAM_CALLBACK_MEMBER_ARRAY\(.*,\s+(.*?)\s*,\s*(\w*)\s*,\s*(\d*)\s*\)", s.line)

                fieldtype = result.group(1)
                fieldname = result.group(2)
                fieldarraysize = result.group(3)

                s.callbackmacro.fields.append(StructField(fieldname, fieldtype, fieldarraysize, comments))
            elif s.line.startswith("STEAM_CALLBACK_MEMBER"):
                result = re.match(r"^STEAM_CALLBACK_MEMBER\(.*,\s+(.*?)\s*,\s*(\w*)\[?(\d+)?\]?\s*\)", s.line)

                fieldtype = result.group(1)
                fieldname = result.group(2)
                fieldarraysize = result.group(3)

                s.callbackmacro.fields.append(StructField(fieldname, fieldtype, fieldarraysize, comments))
            
            else:
                printWarning("Unexpected line in Callback Macro")

            return

        if not s.line.startswith("STEAM_CALLBACK_BEGIN"):
            return

        comments = self.consume_comments(s)

        result = re.match(r"^STEAM_CALLBACK_BEGIN\(\s?(\w+),\s?(.*?)\s*\)", s.line)

        s.callbackmacro = Struct(result.group(1), s.packsize, comments)
        s.callbackmacro.callbackid = result.group(2)

    def parse_interfaces(self, s):
        if s.line.startswith("class ISteam"):
            comments = self.consume_comments(s)
            if s.linesplit[1].endswith(';') or s.linesplit[1].endswith("Response"):  # Ignore Forward Declares and Matchmaking Responses
                return

            s.interface = Interface()
            s.interface.name = s.linesplit[1]
            s.interface.c = comments

        if s.interface:
            self.parse_interface_functions(s)

    def parse_interface_function_atrributes(self, s):
        for a in g_FuncAttribs:
            if s.line.startswith(a):
                attr = FunctionAttribute()
                attr.name = s.line[:s.line.index("(")]
                attr.value = s.line[s.line.index("(")+1:s.line.rindex(")")].strip()
                s.functionAttributes.append(attr)

    def parse_interface_functions(self, s):
        self.parse_interface_function_atrributes(s)

        if s.line.startswith("STEAM_PRIVATE_API"):
            s.bInPrivate = True
            s.line = s.line[s.line.index("(")+1:].strip()
            s.linesplit = s.linesplit[1:]

        bInPrivate = s.bInPrivate
        if s.bInPrivate:
            if s.line.endswith(")"):
                s.bInPrivate = False
                s.line = s.line[:-1].strip()
                s.linesplit = s.linesplit[:-1]


        # Skip lines that don't start with virtual, except when we're currently parsing a function
        if not s.function and not (s.line.startswith("virtual") or s.line.startswith("inline")):
            return

        if '~' in s.line:  # Skip destructor
            return

        args = ""
        attr = None
        if s.function == None:
            s.function = Function()
            if len(s.ifstatements) > 1:
                s.function.ifstatements = s.ifstatements[-1]
            s.function.comments = s.comments
            s.function.linecomment = s.linecomment
            s.function.private = bInPrivate
            s.function.attributes = s.functionAttributes
            s.functionAttributes = []
            self.consume_comments(s)

        linesplit_iter = iter(enumerate(s.linesplit))
        for i, token in linesplit_iter:
            if s.funcState == 0:  # Return Value
                if token == "virtual" or token == "inline":
                    continue

                if token.startswith("*"):
                    s.function.returntype += "*"
                    token = token[1:]
                    s.funcState = 1
                elif "(" in token:
                    s.function.returntype = s.function.returntype.strip()
                    s.funcState = 1
                else:
                    s.function.returntype += token + " "
                    continue

            if s.funcState == 1:  # Method Name
                s.function.name = token.split("(", 1)[0]

                if token[-1] == ")":
                    s.funcState = 3
                elif token[-1] == ";":
                    s.funcState = 0
                    s.interface.functions.append(s.function)
                    s.function = None
                    break
                elif token[-1] != "(":  # Like f(void arg )
                    if Settings.warn_spacing:
                        printWarning("Function is missing whitespace between the opening parentheses and first arg.", s)
                    token = token.split("(")[1]
                    s.funcState = 2
                else:
                    s.funcState = 2
                    continue

            if s.funcState == 2:  # Args
                # Strip clang attributes
                bIsAttrib = False
                for a in g_ArgAttribs:
                    if token.startswith(a):
                        attr = ArgAttribute()
                        bIsAttrib = True
                        break
                if bIsAttrib:
                    openparen_index = token.index("(")
                    attr.name = token[:openparen_index]
                    if len(token) > openparen_index+1:
                        if token.endswith(")"):
                            attr.value = token[openparen_index+1:-1]
                            continue
                        else:
                            attr.value = token[openparen_index+1:]
                    s.funcState = 4
                    continue

                if token.startswith("**"):
                    args += token[:2]
                    token = token[2:]
                elif token.startswith("*") or token.startswith("&"):
                    args += token[0]
                    token = token[1:]

                if len(token) == 0:
                    continue

                if token.startswith(")"):  # Like f( void arg ")"
                    if args:
                        TEST = 1
                        TEST2 = 0  # TODO: Cleanup, I don't even know what the fuck is going on here anymore.
                        if "**" in s.linesplit[i-1]:
                            TEST -= 2
                            TEST2 += 2
                        elif "*" in s.linesplit[i-1] or "&" in s.linesplit[i-1]:
                            TEST -= 1
                            TEST2 += 1

                        arg = Arg()
                        arg.type = args[:-len(s.linesplit[i-1]) - TEST].strip()
                        arg.name = s.linesplit[i-1][TEST2:]
                        arg.attribute = attr
                        s.function.args.append(arg)
                        args = ""
                        attr = None
                    s.funcState = 3
                elif token.endswith(")"):  # Like f( void "arg)"
                    if Settings.warn_spacing:
                        printWarning("Function is missing whitespace between the closing parentheses and first arg.", s)

                    arg = Arg()
                    arg.type = args.strip()
                    arg.name = token[:-1]
                    arg.attribute = attr
                    s.function.args.append(arg)
                    args = ""
                    attr = None
                    s.funcState = 3
                elif token[-1] == ",":  # Like f( void "arg," void arg2 )
                    TEST2 = 0
                    if "*" in token[:-1] or "&" in token[:-1]:
                        TEST2 += 1

                    arg = Arg()
                    arg.type = args.strip()
                    arg.name = token[:-1][TEST2:]
                    arg.attribute = attr
                    s.function.args.append(arg)
                    args = ""
                    attr = None
                elif token == "=":
                    # Copied from ")" above
                    TEST = 1
                    TEST2 = 0  # TODO: Cleanup, I don't even know what the fuck is going on here anymore.
                    if "*" in s.linesplit[i-1] or "&" in s.linesplit[i-1]:
                        TEST -= 1
                        TEST2 += 1

                    arg = Arg()
                    arg.type = args[:-len(s.linesplit[i-1]) - TEST].strip()
                    arg.name = s.linesplit[i-1][TEST2:]
                    arg.default = s.linesplit[i+1].rstrip(",")
                    arg.attribute = attr
                    s.function.args.append(arg)
                    args = ""
                    attr = None
                    next(linesplit_iter, None)
                else:
                    args += token + " "

                continue

            if s.funcState == 3:  # = 0; or line
                if token.endswith(";"):
                    s.funcState = 0
                    s.interface.functions.append(s.function)
                    s.function = None
                    break
                continue

            if s.funcState == 4:  # ATTRIBS
                if token.endswith(")"):
                    attr.value += token[:-1]
                    s.funcState = 2
                else:
                    attr.value += token
                continue

    def parse_classes(self, s):
        if s.linesplit[0] != "class":
            return

        if s.line.startswith("class ISteam"):
            return

        self.consume_comments(s)

        #print("Skipped class: " + s.line)

    def parse_scope(self, s):
        if "{" in s.line:
            s.scopeDepth += 1

            if s.line.count("{") > 1:
                printWarning("Multiple occurences of '{'", s)

        if "}" in s.line:
            s.scopeDepth -= 1

            if s.interface and s.scopeDepth == 0:
                s.f.interfaces.append(s.interface)
                s.interface = None

            if s.scopeDepth < 0:
                printWarning("scopeDepth is less than 0!", s)

            if s.line.count("}") > 1:
                printWarning("Multiple occurences of '}'", s)

    def consume_comments(self, s):
        c = Comment(s.rawcomments, s.comments, s.rawlinecomment, s.linecomment)
        s.rawcomments = []
        s.comments = []
        s.rawlinecomment = None
        s.linecomment = None
        return c


def printWarning(string, s):
    print("[WARNING] " + string + " - In File: " + s.f.name + " - On Line " + str(s.linenum) + " - " + s.line)


def printUnhandled(string, s):
    print("[UNHANDLED] " + string + " - In File: " + s.f.name + " - On Line " + str(s.linenum) + " - " + s.line)


def parse(folder):
    """Parses the Steamworks headers contained in a folder"""
    return Parser(folder)
