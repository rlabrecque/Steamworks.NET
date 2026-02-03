import os
import codecs
import copy
import re
from typing import Literal, Optional
from functools import reduce
import operator

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
    "SteamIDComponent_t"
    
    # steamclientpublic.h nested struct
    "GameID_t"
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

class ClassSpecialRBracket:
    def __init__(self, lineZB: int, action: Literal['EndStruct'] | Literal['ContniueStruct']):
        self.lineZeroBased = lineZB
        self.action: Literal['EndStruct'] | Literal['ContniueStruct'] = action

g_ClassSpecialRBracket = {
    "CSteamID" : ClassSpecialRBracket(850, 'ContniueStruct')
}

class PrimitiveType:
    def __init__(self, name: str, size: int, pack: int):
        self.name = name
        self.size = size
        self.pack = pack

g_PrimitiveTypesLayout: dict[str, PrimitiveType] = {
    "char": PrimitiveType("char", 1, 1),
    "bool": PrimitiveType("bool", 1, 1),
    "unsigned char": PrimitiveType("unsigned char", 1, 1),
    "signed char": PrimitiveType("signed char", 1, 1),
    "short": PrimitiveType("short", 2, 2),
    "unsigned short": PrimitiveType("unsigned short", 2, 2),
    "int": PrimitiveType("int", 4, 4),
    "unsigned int": PrimitiveType("unsigned int", 4, 4),
    "long long": PrimitiveType("long long", 8, 8),
    "unsigned long long": PrimitiveType("unsigned long long", 8, 8),
    "float": PrimitiveType("float", 4, 4),
    "double": PrimitiveType("double", 8, 8),
    
    # Add them here since we don't use the definiation in steamtypes.h
    "uint8": PrimitiveType("unsigned char", 1, 1),
    "int8": PrimitiveType("signed char", 1, 1),
    "int16": PrimitiveType("short", 2, 2),
    "uint16": PrimitiveType("unsigned short", 2, 2),
    "int32": PrimitiveType("int", 4, 4),
    "uint32": PrimitiveType("unsigned int", 4, 4),
    "int64": PrimitiveType("long long", 8, 8),
    "uint64": PrimitiveType("unsigned long long", 8, 8),

    "unsigned __int8": PrimitiveType("unsigned char", 1, 1),
    "__sint8": PrimitiveType("signed char", 1, 1),
    "__int16": PrimitiveType("short", 2, 2),
    "unsigned __int16": PrimitiveType("unsigned short", 2, 2),
    "__int32": PrimitiveType("int", 4, 4),
    "unsigned __int32": PrimitiveType("unsigned int", 4, 4),
    "__int64": PrimitiveType("long long", 8, 8),
    "unsigned __int64": PrimitiveType("unsigned long long", 8, 8),

    "uint8_t": PrimitiveType("unsigned char", 1, 1),
    "sint8_t": PrimitiveType("signed char", 1, 1),
    "int16_t": PrimitiveType("short", 2, 2),
    "uint16_t": PrimitiveType("unsigned short", 2, 2),
    "int32_t": PrimitiveType("int", 4, 4),
    "uint32_t": PrimitiveType("unsigned int", 4, 4),
    "int64_t": PrimitiveType("long long", 8, 8),
    "uint64_t": PrimitiveType("unsigned long long", 8, 8),
    
    "intptr": PrimitiveType("intptr", "intptr", "intptr"),
    "intp": PrimitiveType("intp", "intptr", "intptr"),
    "uintp": PrimitiveType("uintp", "intptr", "intptr"),
    "void*": PrimitiveType("void*", "intptr", "intptr"),
    
    "long int": PrimitiveType("long int", 8, 8),
    "unsigned long int": PrimitiveType("unsigned long int", 8, 8),
}

g_SpecialStructs = {
    "CSteamID": PrimitiveType("unsigned long long", 8, 8),
    "CGameID": PrimitiveType("unsigned long long", 8, 8),
    "SteamIPAddress_t": PrimitiveType("SteamIPAddress_t", 16 + 4, 1),
    "SteamNetworkingIdentity ": PrimitiveType("SteamNetworkingIdentity", 4 + 128, 1),
    # Contains bit fields that size can't be represented as bytes count
    "SteamIDComponent_t": PrimitiveType("SteamIDComponent_t", 8, 8)
}


class Settings:
    warn_utf8bom = False
    warn_includeguardname = False
    warn_spacing = False
    print_unuseddefines = False
    print_skippedtypedefs = False
    fake_gameserver_interfaces = False
    print_debug = False

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
        self.args: list[Arg] = []  # Arg
        self.ifstatements = []
        self.comments = []
        self.linecomment = ""
        self.attributes = []  # FunctionAttribute
        self.private = False

class Interface:
    def __init__(self):
        self.name = ""
        self.functions: list[Function] = []  # Function
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
        # enums' size is always 4(an int)
        self.size = 4
        self.pack = 4

class Struct:
    def __init__(self, name, packsize: int | Literal["PlatformABIDefault"] | None, comments, scopePath):
        self.name = name
        # keep it to remain compatibility
        self.packsize = packsize 
        self.c = comments  # Comment
        self.fields: list[StructField] = []  # StructField
        self.nested_struct: list[Struct] = []  # nested structs
        self.outer_type: Struct | Union | None = None
        self.scopeDepth: int = scopePath
        self.callbackid: str | None = None
        self.endcomments = None  # Comment
        self.pack = packsize 
        self.size: int | None = None
        self.packsize_aware = False
        self.is_skipped: bool = False
        self.is_sequential = packsize == "PlatformABIDefault"
        
    def calculate_offsets(self, defaultAlign: int):
        def calcRealSize(sizelike: int | Literal['intptr']) -> int:
            if sizelike == 'intptr':
                return 8
            else:
                return sizelike

        if not self.fields:
            self.size = 1
            return []
        
        effective_struct_pack = self.packsize if self.packsize is not None else defaultAlign
        
        result = []
        current_offset = 0

        for field in self.fields:
            pack = field.pack or defaultAlign
            effective_field_pack = calcRealSize(pack) 
            effective_field_pack = min(effective_field_pack, defaultAlign)
            padding = 0
            if effective_field_pack > 0:
                padding = (effective_field_pack - (current_offset % effective_field_pack)) % effective_field_pack
                current_offset += padding
            
            if field.size is None: # For classes with typedef inside
                return []
             
            effective_struct_pack = max(effective_struct_pack, effective_field_pack)
            field_total_size = calcRealSize(field.size) * (field.arraysize or 1)
            
            # store offset and total size into layout info
            result.append(FieldOffset(field.name, current_offset))
            
            current_offset += field_total_size
        
        total_size = current_offset
        # if effective_struct_pack > 0:
        #     padding = (effective_struct_pack - (total_size % effective_struct_pack)) % effective_struct_pack
        #     total_size += padding
        
        self.pack = min(
            calcRealSize(max(self.fields, key=lambda x: calcRealSize(x.size)).size),
            effective_struct_pack
        )
        self.size = total_size
        return result

    def should_not_generate(self):
        return self.name in g_SkippedStructs or self.is_skipped or (self.outer_type is not None) or len(self.nested_struct) > 0

class Union:
    def __init__(self, name, isUnnamed, pack):
        self.name: str = name
        self.isUnnamed: bool = isUnnamed
        self.pack: int = pack
        self.size: int | None = None
        self.fields: list[StructField] = []
        self.outer_type: Struct | Union | None = None
        self.endcomments = None  # Comment
        pass
    
    def calculate_offsets(self, defaultAlign: int):
        if not self.fields:
            self.size = 1
            return self.size
            
        # find out the largest pack and it's size
        max_field = max(self.fields, key=lambda f: f.size)
        max_size = max_field.size
        max_pack = max_field.pack if max_field.pack != None else defaultAlign

        # align with largest field's packsize
        if max_pack:
            remainder = max_size % max_pack
            if remainder != 0:
                self.size = max_size + (max_pack - remainder)
            else:
                self.size = max_size
        else:
            self.size = max_size
        
        return self.size		

'''Also used in Union'''
class StructField:
    def __init__(self, name, typee, arraysize: str | None, comments):
        self.name: str = name
        self.type = typee
        self.arraysizeStr = arraysize
        self.arraysize: int | None = None
        self.c = comments  # Comment
        self.size: int | Literal['intptr'] = None # Popluated after parsed, before generate
        self.pack: int | Literal['intptr'] = None # Also populated lazily

class FieldOffset:
    def __init__(self, name: str, offset: int):
        self.name = name
        self.offset = offset
    
    def __eq__(self, value):
        return self.name == value.name and self.offset == value.offset

class Typedef:
    def __init__(self, name, typee, filename, comments, size: int, pack: Optional[int] | None = None):
        self.name = name
        self.type = typee
        self.filename = filename
        self.c = comments
        self.size: int | Literal['intptr'] = size
        self.pack: int | Literal['intptr'] = pack

class SteamFile:
    def __init__(self, name):
        self.name = name
        self.header = []
        self.includes = []
        self.defines: list[Define] = []  # Define
        self.constants: list[Constant] = []  # Constant
        self.enums: list[Enum] = []  # Enum
        self.structs: list[Struct] = []  # Struct
        self.callbacks: list[Struct] = [] # Struct
        self.interfaces: list[Interface] = []  # Interface
        self.typedefs:list[Typedef] = []  # Typedef
        self.unions: list[Union] = []

class ParserState:
    def __init__(self, file):
        self.f: SteamFile = file  # SteamFile
        self.lines = []
        self.line: str = ""
        self.originalline = ""
        self.linesplit: list[str] = []
        """
        linenum is Zero based
        """
        self.linenum = 0
        self.rawcomments = []
        self.comments = []
        self.rawlinecomment = None
        self.linecomment = None
        self.ifstatements = []
        # packsize stack
        self.packsize = []
        self.funcState = 0
        self.scopeDepth = 0
        self.complexTypeStack: list[Literal['union', 'struct',  'enum']] = []

        self.interface: Interface | None = None
        self.function: Function | None = None
        self.enum: Enum | None = None
        self.struct: Struct | None = None
        self.union: Union | None = None
        self.callbackmacro = None

        self.bInHeader = True
        self.bInMultilineComment = False
        self.bInMultilineMacro = False
        self.bInPrivate = False
        self.isInlineMethodDeclared = False
        self.callbackid: str | None = None
        self.isClassLikeStruct: bool | None = None
        self.functionAttributes: list[FunctionAttribute] = [] # FunctionAttribute
        
        self.currentSpecialStruct: PrimitiveType = None
        
    def beginUnion(self):
        self.complexTypeStack.append('union')
    
    def beginStruct(self):
        self.complexTypeStack.append('struct')

    def beginEnum(self):
        self.complexTypeStack.append('enum')

    def endComplexType(self):
        self.complexTypeStack.pop()
        
    def getCurrentPack(self) -> int | Literal['PlatformABIDefault'] | None:
        # pack size is default value
        # our parser can't evaluate #ifdefs, so in the situlation of 
		# using default pack, the self.packsize will be [4, 8]
        if self.packsize == [4, 8]:
            # default pack
            return None
        elif self.packsize == [4]:
            # we can't eval #ifdefs, so all push will be recorded without
            # checking if it is really sets pack value
            # one of [4, 8] is not used by #ifdef, if code pops in this state
            # it means platform ABI default pack is restored
            return 'PlatformABIDefault'
        else:
            return self.packsize[-1] if len(self.packsize) > 0 else None
        
    def getCurrentComplexType(self) -> Literal['struct', 'union', 'enum'] | None:
        return self.complexTypeStack[-1] if len(self.complexTypeStack) > 0 else None

class Parser:
    files = None
    typedefs = []
    ignoredStructs: list[Struct] = []

    def __init__(self, folder):
        self.files: list[SteamFile] = [SteamFile(f) for f in os.listdir(folder)
                                       if os.path.isfile(os.path.join(folder, f))
                                        and f.endswith(".h")
                                        and f not in g_SkippedFiles]
        self.files
        self.files.sort(key=lambda f: f.name)

        self.typedefs:list[Typedef] = [
		]


        for f in self.files:
            s = ParserState(f)
            filepath = os.path.join(folder, f.name)
            with open(filepath, 'r', encoding="latin-1") as infile:
                s.lines = infile.readlines()

                if s.lines[0][:3] == codecs.BOM_UTF8:
                    # Reload file with UTF-8 encoding
                    infile.close()
                    infile = open(filepath, 'r', encoding="utf-8")
                    s.lines = infile.readlines()
                    s.lines[0] = s.lines[0][3:]
                 
                    if Settings.warn_utf8bom:
                        printWarning("File contains a UTF8 BOM.", s)

                self.parse(s)
                
        
        self.populate_typedef_layouts()
        # self.populate_struct_field_sizes()

        # Hack to give us the GameServer interfaces.
        # We want this for autogen but probably don't want it for anything else.
        if Settings.fake_gameserver_interfaces:
            for f in [f for f in self.files if f.name in g_GameServerInterfaces]:
                gs_f = SteamFile(f.name.replace("isteam", "isteamgameserver", 1))
                gs_f.interfaces = copy.deepcopy(f.interfaces)
                for i in gs_f.interfaces:
                    i.name = i.name.replace("ISteam", "ISteamGameServer", 1)
                self.files.append(gs_f)

        self.findout_platform_aware_structs()

    def parse(self, s: ParserState):
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
            self.visit_union(s)
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

    def parse_preprocessor(self, s: ParserState):
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
                
                packsize = None
                try:
                    packsize = int(tmpline)
                except ValueError:
                    pass
                
                s.packsize.append(packsize)
            elif "pop" in s.line:
                s.packsize.pop()
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


    def parse_typedefs(self, s: ParserState):
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

        aliasedType = self.resolveTypeInfo(typee)
        size = aliasedType.size
        pack = aliasedType.pack

        typedef = Typedef(name, typee, s.f.name, comments, size, pack)

        self.typedefs.append(typedef)
        s.f.typedefs.append(typedef)

    def populate_typedef_layouts(self):
        for typedef in self.typedefs:
            typee = typedef.type
            
            if typee in g_PrimitiveTypesLayout.keys():
                primitive_def = g_PrimitiveTypesLayout[typee]
                typedef.pack = primitive_def.pack
                typedef.size = primitive_def.size
                return

            def resolveFinalType(typee):
                underlying_type: PrimitiveType | Typedef | None = g_PrimitiveTypesLayout.get(typee)

                if '*' in typee:
                    return g_PrimitiveTypesLayout["intptr"]

                if underlying_type == None:
                    underlying_type = next((typedef for typedef in self.typedefs if typedef.name == typee), None)
                
                if underlying_type == None:
                    # scan in steam interface files
                    underlying_type = next([typedef for file in self.files for typedef in file["typedefs"]], None)

                if underlying_type.name not in g_PrimitiveTypesLayout.keys():
                    return resolveFinalType(underlying_type)
                else:
                    return underlying_type

            underlying_type = resolveFinalType(typee)

            if underlying_type == None and '*' not in typee:
                print(f"[WARNING] typedef \"{typedef.name}\"'s underlying type \"{typee}\" is not in primitive list")
            # is pointer
            elif '*' in typee:
                size = 'intptr'
                pack = 'intptr'
            else:
                size = underlying_type.size
                pack = underlying_type.pack
            
            typedef.size = size
            typedef.pack = pack

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

        result = re.match(".*const\s+(.*)\s+(\w+)\s+=\s+(.*);$", s.line)

        if not result:
            return

        constant = Constant(result.group(2), result.group(3), result.group(1), comments);
        s.f.constants.append(constant)

    def parse_enums(self, s: ParserState):
        if s.enum:
            if s.line == "{":
                return

            if s.line.endswith("};"):
                # Hack to get comments between the last field and }; :(
                s.enum.endcomments = self.consume_comments(s)
                # Don't append unnamed (constant) enums
                if s.enum.name is not None:
                    s.f.enums.append(s.enum)

                s.endComplexType()
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
            #
			# Also steamnetworkingtypes.h has
			# two different anon enum defined same named field,
            # broke our project. Skip it.
            if "," in s.line or s.f.name == 'steamnetworkingtypes.h':
                return

            # Skips lines in macros
            # Currently only skips one enum in DEFINE_CALLBACK
            if s.linesplit[-1] == "\\":
                return

            if s.struct:
                # not to push complex type stack here, since it's single line only
                result = re.match("^enum { (.*) = (.*) };", s.line)
                name = result.group(1)

                if name == "k_iCallback":
                    s.callbackid = result.group(2)
                    return

            constant = Constant(s.linesplit[2], s.linesplit[4], "int", comments);
            s.f.constants.append(constant)
            return

        if len(s.linesplit) == 1 or (len(s.linesplit) >= 2 and '{' == s.linesplit[1]):
            s.beginEnum()
            s.enum = Enum(None, comments)
            # unnamed Constants like:
            '''enum {
                k_name1 = value,
                k_name2 = value,
            };'''
            return

        s.beginEnum()
        s.enum = Enum(s.linesplit[1], comments)

    def parse_enumfields(self, s):
        result = re.match("^(\w+,?)([ \t]*)=?([ \t]*)(.*)$", s.line)
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

    def parse_structs(self, s: ParserState):
        if s.enum:
            return

        if s.struct and s.linesplit[0] != "struct":
            if s.line == "};":
                if s.struct.name in g_ClassSpecialRBracket:
                    special = g_ClassSpecialRBracket[s.struct.name]
                    if special.lineZeroBased == s.line\
                    and special.action == 'ContniueStruct':
                        return
                    

                s.struct.endcomments = self.consume_comments(s)

                if s.callbackid:
                    s.struct.callbackid = s.callbackid
                    s.f.callbacks.append(s.struct)
                    s.callbackid = None
                else:
                    s.f.structs.append(s.struct)
                
                s.isClassLikeStruct = None
                s.endComplexType()

                currentStruct: Struct = s.struct
                
                # restore current struct in parser state to outer struct
                if len(s.complexTypeStack) >= 2 and s.complexTypeStack[-2] == 'struct':
                    currentStruct.outer_type.nested_struct.append(currentStruct)
                
                if s.struct.name in g_SpecialStructs:
                    s.struct.packsize_aware = False # HACK hope so

                s.struct = currentStruct.outer_type
            else:
                self.parse_struct_fields(s)
        else:
            if s.linesplit[0] != "struct":
                return

            if len(s.linesplit) > 1 and s.linesplit[1].startswith("ISteam"):
                return

            # Skip Forward Declares
            if s.linesplit[1].endswith(";"):
                return

			# special structs
            typeNameCandidate = s.linesplit[1]
            if (typeNameCandidate in ("CCallResult", "CCallback", "CCallbackBase", "CCallbackImpl", "CCallbackManual")):
                self.ignoredStructs.append(Struct(typeNameCandidate, 8, None, ""))
                return

            if typeNameCandidate in g_SpecialStructs.keys():
                if s.linesplit[0] == 'struct':
                    s.currentSpecialStruct = g_SpecialStructs[typeNameCandidate]

                self.parse_scope(s)
                
                if s.line.startswith('}'):
                    varNameMatchResult = re.match(r"^}\s*(\w*);$", s.line)
                    if varNameMatchResult != None:
                        s.struct.outer_type.fields.append(StructField(varNameMatchResult.group(1), typeNameCandidate, None, ""))
                    return

            s.beginStruct()
           
            if s.linesplit[0] == "class":
                s.isClassLikeStruct = True
            else:
                s.isClassLikeStruct = False
            
            comments = self.consume_comments(s)

            outerTypeCandidate = s.struct
            s.struct = Struct(s.linesplit[1].strip(), s.getCurrentPack(),\
                              comments, s.scopeDepth)
            s.struct.outer_type = outerTypeCandidate
            if s.linesplit[1].strip() in g_SkippedStructs:
                s.struct.is_skipped = True

    def visit_inline_method(self, s: ParserState):
        if s.struct:
            if s.function == None:
                s.function = Function()
                if len(s.ifstatements) > 1:
                    s.function.ifstatements = s.ifstatements[-1]
                s.function.comments = s.comments
                s.function.linecomment = s.linecomment
                s.function.private = True
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

            s.isInlineMethodDeclared = True
            return


    def parse_struct_fields(self, s):
        comments = self.consume_comments(s)

        if s.line.startswith("enum"):
            return
        
        if s.line.startswith('friend '): # in classes
            return

        if s.line == "{":
            return

        def try_match(line, s: ParserState):
            if ':' in line:
                # Contains bitfield that can't be represented
                printWarning(f"{s.struct.name} contains bitfield, skipping", s)
                self.parse_scope(s)
                return

            typeinfo = s.struct if s.struct else s.union

            fieldarraysizeText = None
        
            result = re.match(r"^([^=.]*\s\**)(\w+);$", line)
            if result is None:
                result = re.match(r"^([^=.]*\s\**)(\w+);$", line)
                if result is None:
                    result = re.match(r"^(.*\s\*?)(\w+)\[\s*(\w+)?\s*\];$", line)
                    if result is not None:
                        fieldarraysizeText = result.group(3)
                    else:
                        return

            fieldtype = result.group(1).rstrip()
            fieldname = result.group(2)

            # ignore wrongly parsed result
            # for example {type 'void' name: '(int a0, int a1)'
            if '(' in fieldname or '(' in fieldtype\
                or ')' in fieldname or ')' in fieldtype\
                or '*' in fieldname\
            	or '{' in fieldtype or '}' in fieldtype\
            	or '{' in fieldname or '}' in fieldname:
                return


            newField = StructField(fieldname, fieldtype, fieldarraysizeText, comments)
            typeinfo.fields.append(newField)
        
        if ',' in s.line:
            result = re.match(r"^(\s*\w+)\s*([\w,\s\[$*\d]*);$", s.line)
            if not result: return

            
            mainType = result.group(1).strip()
            varNames = result.group(2).split(',')

            for varName in varNames:
                try_match(f"{mainType} {varName};", s)
        else:
            try_match(s.line, s)

    def visit_union(self, s: ParserState):
        if s.enum:
            return
        
        if s.union and s.linesplit[0] != "union":
            if s.line == "{":
                # some unions put open brace at next line
                return
            
            if s.line == "};":
                s.union.endcomments = self.consume_comments(s)
                s.f.unions.append(s.union)
                s.endComplexType()
                s.union = None
            else:
                self.parse_struct_fields(s)
            pass
        elif s.union == None:
            if s.linesplit[0] != "union":
                return

            # Skip Forward Declares
            if len(s.linesplit) >= 2 and s.linesplit[1].endswith(";"):
                return

            s.beginUnion()
            typeName = None
            # varName = None
            isUnnamed = True
            
            if s.linesplit[0] == 'union':
                if len(s.linesplit) > 2:
                    typeName = s.linesplit[1]
                    isUnnamed = False
                else:
                    typeName = f"union__{s.f.name[:-2]}_{s.linenum + 1}"
                
            s.union = Union(typeName, isUnnamed, s.packsize)
            if s.union.outer_type:
               # just ignore it's name, generate one for it
               s.union.outer_type.fields.append(StructField(f"unnamed_field_{typeName}", typeName, 1, ""))

                
        pass

    def parse_callbackmacros(self, s: ParserState):
        if s.callbackmacro:
            comments = self.consume_comments(s)
            if s.line.startswith("STEAM_CALLBACK_END("):
                s.f.callbacks.append(s.callbackmacro)
                s.callbackmacro = None
            elif s.line.startswith("STEAM_CALLBACK_MEMBER_ARRAY"):
                result = re.match("^STEAM_CALLBACK_MEMBER_ARRAY\(.*,\s+(.*?)\s*,\s*(\w*)\s*,\s*(\d*)\s*\)", s.line)

                fieldtype = result.group(1)
                fieldname = result.group(2)
                fieldarraysize = result.group(3)

                s.callbackmacro.fields.append(StructField(fieldname, fieldtype, fieldarraysize, comments))
            elif s.line.startswith("STEAM_CALLBACK_MEMBER"):
                result = re.match("^STEAM_CALLBACK_MEMBER\(.*,\s+(.*?)\s*,\s*(\w*)\[?(\d+)?\]?\s*\)", s.line)

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

        result = re.match("^STEAM_CALLBACK_BEGIN\(\s?(\w+),\s?(.*?)\s*\)", s.line)

        s.callbackmacro = Struct(result.group(1), s.getCurrentPack(), comments, s.scopeDepth)
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

    def parse_classes(self, s: ParserState):
        if s.linesplit[0] != "class":
            return

        if s.line.startswith("class ISteam"):
            return

        self.consume_comments(s)

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
    
    # I initially choose camel case by my habit, but keep this name here
    # for hinting it this is an external available API is also useful
    def resolveTypeInfo(self, typeName):
        # search order: primitive, pointer, enum, typedef, struct. no callbacks
        result = g_PrimitiveTypesLayout.get(typeName)
        
        if not result and '*' in typeName:
            return g_PrimitiveTypesLayout["intptr"]

        if not result:
            result = g_SpecialStructs.get(typeName)

        if not result:
            result = next((typedef for typedef in self.typedefs if typedef.name == typeName), None)

        if not result:
            # see enums
            allEnums = reduce(operator.concat, [f.enums for f in self.files ])
            result = next((enum for enum in allEnums if enum.name == typeName), None)
        if not result:
            # see structs or callback structs
            allstructs = reduce(operator.concat, [f.structs for f in self.files ])
            allcallbacks = reduce(operator.concat, [f.callbacks for f in self.files])
            
            allstructs = allstructs + allcallbacks
            
            result = next((struct for struct in allstructs if struct.name == typeName), None)

        if not result:
            print(f"[WARNING] typename {typeName} not found across primitive,\
 struct and typedef, maybe it is a nested type.")
        
        return result

    def resolveConstValue(self, name) -> Constant:
        for f in self.files:
            result = next((constant for constant in f.constants if constant.name == name), None)
            if result is not None:
                return result
            
        return None
            

    def populate_union_sizes(self, defaultPack = 8):
        for file in self.files:
            unions = file.unions
            for union in unions:
                union.calculate_offsets(defaultPack)
    
    def populate_struct_field_layout(self, struct: Struct, defaultPack = 8):
        for field in struct.fields:
            typeinfo = self.resolveTypeInfo(field.type)

            if typeinfo is None:
                # this usually means typedef is used inside a class,
                # but reminder we treat classes as struct
                self.ignoredStructs.append(struct)
                return []

            # check if we facing a struct which may not populated yet
            if isinstance(typeinfo, Struct):
                # we assume there will no circular references across structs
                if not typeinfo.size:
                    self.populate_struct_field_layout(typeinfo, defaultPack)
                    typeinfo.calculate_offsets(defaultPack)
                    
            field.size = typeinfo.size
            field.pack = typeinfo.pack or struct.pack or defaultPack
            if (field.arraysizeStr is not None):
                arrsize = field.arraysizeStr
                field.arraysize = int(arrsize) if arrsize.isdigit() else eval(self.resolveConstValue(arrsize).value, {}, )
            
        struct.calculate_offsets(defaultPack)


    def findout_platform_aware_structs(self):
        self.packSizeAwareStructs: list[str] = []
        self.populate_typedef_layouts()
        
        for file in self.files:
            structs: list[Struct] = []
            structs.extend(file.callbacks)
            structs.extend(file.structs)
            
            for struct in structs:
                if struct.is_sequential:
                    print_debug(f"Struct {struct.name} is aligns by platform ABI default, means sequential")
                    continue

                self.populate_struct_field_layout(struct, 8)
                offsetsLargePack: list[FieldOffset] = struct.calculate_offsets(8)
                offsetsLargePack.sort(key = lambda item: item.name)
                sizeLarge = struct.size

                self.populate_struct_field_layout(struct, 4)
                offsetsSmallPack: list[FieldOffset] = struct.calculate_offsets(4)
                offsetsSmallPack.sort(key = lambda item: item.name)
                sizeSmall = struct.size
                
                if offsetsLargePack != offsetsSmallPack or sizeLarge != sizeSmall:
                    print_debug(f"Found packsize aware struct '{struct.name}'")
                    struct.packsize_aware = True
                    self.packSizeAwareStructs.append(struct.name)

        pass

def print_debug(string: str):
    if Settings.print_debug:
        print(f"[DEBUG][PostParse] {string}")

def printWarning(string, s):
    print("[WARNING] " + string + " - In File: " + s.f.name + " - On Line " + str(s.linenum) + " - " + s.line)


def printUnhandled(string, s):
    print("[UNHANDLED] " + string + " - In File: " + s.f.name + " - On Line " + str(s.linenum) + " - " + s.line)


def parse(folder):
    """Parses the Steamworks headers contained in a folder"""
    return Parser(folder)
