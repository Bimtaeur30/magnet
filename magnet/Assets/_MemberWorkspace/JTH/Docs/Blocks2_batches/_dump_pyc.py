import marshal
import types
from pathlib import Path

p = Path(__file__).with_name("__pycache__") / "_extract_hands.cpython-314.pyc"
raw = p.read_bytes()
code = marshal.loads(raw[16:])


def dump(code_obj, indent=0):
    pad = " " * indent
    print(f"{pad}# {code_obj.co_name} args={code_obj.co_varnames[:code_obj.co_argcount]}")
    strs = [c for c in code_obj.co_consts if isinstance(c, str)]
    print(f"{pad}# strings: {strs[:40]}")
    for c in code_obj.co_consts:
        if isinstance(c, (int, float, tuple, list, dict, frozenset)) and not isinstance(c, bool):
            s = repr(c)
            if len(s) < 300:
                print(f"{pad}# const: {s}")
    for c in code_obj.co_consts:
        if isinstance(c, types.CodeType):
            dump(c, indent + 2)


dump(code)
