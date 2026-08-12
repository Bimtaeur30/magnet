import json
import os
import sys
from collections import Counter
from pathlib import Path

from PIL import Image

sys.path.insert(0, str(Path(__file__).resolve().parent))
import _extract_hands as eh

src = Path(r"C:\Users\hwanj\OneDrive\바탕 화면\Blocks\Unique")
tmp = Path(__file__).resolve().parent / "_unique_up"
tmp.mkdir(exist_ok=True)

results = []
success = 0
triples = Counter()
shape = Counter()

for name in sorted(os.listdir(src)):
    if not name.lower().endswith(".jpg"):
        continue
    im = Image.open(src / name).convert("RGB")
    # Match Blocks2 width so original 48..68 cell search applies.
    target_w = 1080
    target_h = int(round(im.height * (target_w / im.width)))
    up = im.resize((target_w, target_h), Image.Resampling.BICUBIC)
    path = tmp / name
    up.save(path, quality=95)
    ids = eh.extract_hand(str(path))
    if ids is None:
        print(name, "FAIL", flush=True)
        results.append({"file": name, "note": "fail"})
        continue
    print(name, ids, flush=True)
    results.append({"file": name, "ids": ids})
    success += 1
    triples[tuple(ids)] += 1
    for i in ids:
        shape[i] += 1

out = {
    "processed": len(results),
    "success": success,
    "top_triples": [{"ids": list(k), "count": v} for k, v in triples.most_common(40)],
    "shape_freq": [
        {"id": i, "count": n} for i, n in sorted(shape.items(), key=lambda x: (-x[1], x[0]))
    ],
    "hands": results,
}
Path(__file__).resolve().parent.joinpath("unique_hands.json").write_text(
    json.dumps(out, indent=2, ensure_ascii=False), encoding="utf-8"
)
print("success", success, "/", len(results), flush=True)
print("freq", out["shape_freq"], flush=True)
