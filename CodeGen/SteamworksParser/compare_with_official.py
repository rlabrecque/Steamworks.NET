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

mismatchCallbackDiagnostics:list[tuple[int, str]] = []
matchCallbackIds: list[int] = []
redundantCallbackIds: list[int] = []

fieldsMatchedStructs: list[Struct] = []

with open("./steamtest/steam_api.json") as f:
    official = json.load(f, object_hook=lambda d: SimpleNamespace(**d))
    with open("./bin/compare-official/result.txt", "w", encoding="utf-8") as compareF:
        flattenedCallbacks = [callback for file in parser.files for callback in file.callbacks]
        flattenedStructs = [struct for file in parser.files for struct in file.structs]
        
        
        for cb in flattenedCallbacks:
            idParts = cb.callbackid.strip().split(' ')
            idConst = parser.resolveConstValue(idParts[0].strip())
            identity = None
            if idParts == 3:
                identity = int(idConst.value) + int(idParts[2])
            else:
                identity = int(idConst.value)

            officialCBs: list = official.callback_structs
            officialCB = next((o for o in officialCBs if o.callback_id == identity), None)
            diag = None
            if officialCB is None:
                redundantCallbackIds.append(identity)
                continue
            if officialCB.struct != cb.name:
                diag = (identity, f"E1: Name mismatch, ours is `{cb.name}`, official one is `{officialCB.struct}`")
                mismatchCallbackDiagnostics.append(diag)
                continue
            for i, fld in enumerate(officialCB.fields):
                if fld.fieldname != cb.fields[i].name:
                    diag = (identity, f"E2: field[{i}]'s name mismatch, `{fld.fieldname}` expected, got `{cb.fields[i].name}`")
                    break

                def our_type_to_official_string(ourFld: StructField):
                    if ourFld.arraysize:
                        return f"{ourFld.type} [{ourFld.arraysize}]"
                    else:
                        return ourFld.name

                if fld.fieldtype != our_type_to_official_string(cb.fields[i]):
                    arrayDiag = cb.fields[i].arraysize or "None"
                    diag = (identity, f"E3: field[{i}]'s type mismatch, `{fld.fieldtype}` expected,"
                            f" got `{cb.fields[i].type}`, array size {arrayDiag}")
                    break
            if diag is not None:
                mismatchCallbackDiagnostics.append(diag)
                continue

for diag in mismatchCallbackDiagnostics:
    print(f"{diag[1]}.\n\tCallback id {diag[0]}.")
