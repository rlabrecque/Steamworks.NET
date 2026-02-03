from types import SimpleNamespace
from steamworksparser import Struct, StructField, parse, Settings
import os
import json
import itertools
# Settings.print_skippedtypedefs = True
# Settings.print_unuseddefines = True
# Settings.warn_spacing = True
# Settings.print_debug = True

filepath1 = "./bin/packsize-aware-list.aggressive.txt"
filepath2 = "./bin/packsize-aware-list.conservative.txt"

special_structs = []
special_structs_conservative = []

# read size-different based special struct
with open(filepath1, 'r') as f:
    special_structs = [line.strip() for line in f if line.strip()]

# any differences in struct info will be a special struct, which called conservative
with open(filepath2, 'r') as f:
    special_structs_conservative = [line.strip() for line in f if line.strip()]

parser = parse("./steamtest") # put steam headers inside

mismatchCallbackDiagnostics:list[tuple[int, str, str, str]] = [] # (callbackid, name, message, code)
matchStructs: list[str] = []
mismatchStructs: list[str] = []


for structName in parser.packSizeAwareStructs:
    special = False
    specialConservative = False
    typeinfo: Struct = parser.resolveTypeInfo(structName)
    if structName in special_structs:
        special = True
        special_structs.remove(structName)
    if structName in special_structs_conservative:
        specialConservative = True
        special_structs_conservative.remove(structName)

    if not special and not specialConservative:
        mismatchStructs.append(structName)
        diagRecord = (typeinfo.callbackid, structName, f"{structName} is absolutely not a special marshalling struct", "W1")
        mismatchCallbackDiagnostics.append(diagRecord)
        continue

    if specialConservative and not special:
        diagRecord = (typeinfo.callbackid, structName, f"{structName} might be a special marshalling struct by align issues", "W2")
        mismatchCallbackDiagnostics.append(diagRecord)
        
    if special:
        matchStructs.append(structName)
    
for missingCriticialStruct in special_structs:
    typeinfo: Struct = parser.resolveTypeInfo(missingCriticialStruct)
    diagRecord = (typeinfo.callbackid, missingCriticialStruct, f"Critical special marshalling struct {missingCriticialStruct} is missing", "E3")
    mismatchCallbackDiagnostics.append(diagRecord)

for missingOptionalStruct in special_structs:
    typeinfo: Struct = parser.resolveTypeInfo(missingOptionalStruct)
    diagRecord = (typeinfo.callbackid, missingOptionalStruct, f"{structName} might be a special marshalling struct by align issues", "W2")
    mismatchCallbackDiagnostics.append(diagRecord)

with open("./bin/struct_test_result.txt", "w") as f:
    for diag in mismatchCallbackDiagnostics:
        fallbackStr = "None"
        formatted = f"{diag[3]}: {diag[2]}\n\tname: {diag[1]}, cbid(optional): {diag[0] or fallbackStr}\n"
        print(formatted)
        f.write(formatted)
