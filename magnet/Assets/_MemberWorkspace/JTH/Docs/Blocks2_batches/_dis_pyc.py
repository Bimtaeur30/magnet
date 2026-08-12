import dis
import marshal
from pathlib import Path

p = Path(__file__).with_name("__pycache__") / "_extract_hands.cpython-314.pyc"
code = marshal.loads(p.read_bytes()[16:])

wanted = {"slot_centers", "extract_hand", "extract_piece", "score_grid", "estimate_background", "process_batch"}


def walk(c):
    if c.co_name in wanted:
        print("=" * 60, c.co_name)
        dis.dis(c)
    for x in c.co_consts:
        if hasattr(x, "co_code"):
            walk(x)


walk(code)
