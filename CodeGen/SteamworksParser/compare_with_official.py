from types import SimpleNamespace
from steamworksparser import Struct, StructField, parse, Settings
import os
import json
import itertools
# Settings.print_skippedtypedefs = True
# Settings.print_unuseddefines = True
# Settings.warn_spacing = True
# Settings.print_debug = True

parser = parse("./steamtest") # put steam headers inside

os.makedirs("./bin/compare-official", exist_ok=True)
os.makedirs("./bin/native", exist_ok=True)


mismatchCallbackDiagnostics:list[tuple[int, str, str]] = [] # (callbackid, message, code)
matchCallbackIds: list[int] = []
redundantCallbackIds: list[int] = []

fieldsMatchedStructs: list[Struct] = []

with open("./steamtest/steam_api.json") as f:
    official = json.load(f, object_hook=lambda d: SimpleNamespace(**d))
    flattenedCallbacks = [callback for file in parser.files for callback in file.callbacks]
    flattenedStructs = [struct for file in parser.files for struct in file.structs]
    
    
    for cb in flattenedCallbacks:
        idParts = cb.callbackid.strip().split(' ')
        idConst = parser.resolveConstValue(idParts[0].strip())
        identity = None
        if len(idParts) == 3:
            identity = int(idConst.value) + int(idParts[2])
        else:
            identity = int(idConst.value)

        officialCBs: list = official.callback_structs
        officialCB = next((o for o in officialCBs if o.callback_id == identity), None)
        diag = None
        if officialCB is None:
            redundantCallbackIds.append(identity)
            continue
        if officialCB.struct != cb.name and identity != 1108: # 1108 is shared between CL and GS
            diag = (identity, f"Name mismatch, ours is `{cb.name}`, official one is `{officialCB.struct}`", 'E1')
            mismatchCallbackDiagnostics.append(diag)
            continue
        
        if len(officialCB.fields) != len(cb.fields):
            diag = (identity, f"Callback `{cb.name}`'s field count({len(cb.fields)}) is not execpted({len(officialCB.fields)})", 'E4')
            continue

        for i, fld in enumerate(officialCB.fields):
            official_field = officialCB.fields[i]
            our_field = cb.fields[i]
            
            # compare name
            if official_field.fieldname != our_field.name:
                diag = (identity, f"field[{i}]'s name mismatch, `{official_field.fieldname}` expected, got `{our_field.name}`", 'E2')
                break

            def our_type_to_official_format(ourFld: StructField):
                if ourFld.arraysize:
                    return f"{ourFld.type} [{ourFld.arraysize}]"
                else:
                    return ourFld.type
            
            # compare type
            typeGot = our_type_to_official_format(our_field)
            if official_field.fieldtype != typeGot:
                arrayDiag = our_field.arraysize or "None"
                diag = (identity, f"Callback {officialCB.struct} field[{i}] ({official_field.fieldname})'s type mismatch, "\
                        f"`{official_field.fieldtype}` excepted, got `{our_field.name}: {typeGot}`", 'E3')
                break

        if diag is not None:
            mismatchCallbackDiagnostics.append(diag)
            continue

with open("./bin/compare-official/result.txt", "w", encoding="utf-8") as compareF:
    diagLines: list[str] = []
    for diag in mismatchCallbackDiagnostics:
        diagString = f"{diag[2]}: {diag[1]}.\n\tCallback id {diag[0]}."
        diagLines.append(diagString)
        print(diagString)
    
    diagLines = [line + '\n' for line in diagLines]
    compareF.writelines(diagLines)
