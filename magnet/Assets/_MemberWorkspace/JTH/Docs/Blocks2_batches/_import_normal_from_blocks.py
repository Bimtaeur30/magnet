"""Extract Blocks root + merge Blocks2 → Normal pool codegen + overlap stats."""
from __future__ import annotations

import json
import os
import sys
from collections import Counter, defaultdict
from pathlib import Path

from PIL import Image

sys.path.insert(0, str(Path(__file__).resolve().parent))
import _extract_hands as eh

OUT_DIR = Path(__file__).resolve().parent
CACHE = OUT_DIR / "blocks_root_hands.json"


def desktop_blocks() -> Path:
    import ctypes
    import ctypes.wintypes

    buf = ctypes.create_unicode_buffer(ctypes.wintypes.MAX_PATH)
    ctypes.windll.shell32.SHGetFolderPathW(None, 0, None, 0, buf)
    return Path(buf.value) / "Blocks"


def load_blocks2() -> list[dict]:
    hands: list[dict] = []
    for p in sorted(OUT_DIR.glob("batch*_hands.json")):
        data = json.loads(p.read_text(encoding="utf-8"))
        rows = data if isinstance(data, list) else data.get("hands", [])
        for e in rows:
            ids = e.get("ids")
            if ids is None or any(x is None for x in ids) or len(ids) != 3:
                continue
            hands.append({"file": e["file"], "ids": [int(x) for x in ids], "src": "Blocks2"})
    return hands


def extract_root(force: bool = False) -> list[dict]:
    if CACHE.exists() and not force:
        data = json.loads(CACHE.read_text(encoding="utf-8"))
        print(f"cache hit {CACHE} success={data.get('success')}", flush=True)
        return [h for h in data["hands"] if "ids" in h]

    blocks = desktop_blocks()
    tmp = OUT_DIR / "_blocks_root_up"
    tmp.mkdir(exist_ok=True)
    unique_names = set()
    uq = blocks / "Unique"
    if uq.is_dir():
        unique_names = {n.lower() for n in os.listdir(uq) if n.lower().endswith(".jpg")}

    hands: list[dict] = []
    files = sorted(n for n in os.listdir(blocks) if n.lower().endswith(".jpg"))
    success = 0
    for i, name in enumerate(files, 1):
        if name.lower() in unique_names:
            continue
        src = blocks / name
        im = Image.open(src).convert("RGB")
        target_w = 1080
        target_h = int(round(im.height * (target_w / im.width)))
        up = im.resize((target_w, target_h), Image.Resampling.BICUBIC)
        path = tmp / name
        up.save(path, quality=95)
        ids = eh.extract_hand(str(path))
        if ids is None:
            hands.append({"file": name, "src": "Blocks", "note": "fail"})
            print(f"[{i}/{len(files)}] {name} FAIL", flush=True)
            continue
        hands.append({"file": name, "ids": ids, "src": "Blocks"})
        success += 1
        print(f"[{i}/{len(files)}] {name} {ids}", flush=True)

    payload = {
        "processed": len(files),
        "success": success,
        "hands": hands,
    }
    CACHE.write_text(json.dumps(payload, indent=2, ensure_ascii=False), encoding="utf-8")
    print(f"wrote cache success={success}/{len(files)}", flush=True)
    return [h for h in hands if "ids" in h]


def main() -> None:
    b2 = load_blocks2()
    root = extract_root(force=False)
    all_hands = b2 + root
    print(f"Blocks2={len(b2)} Blocks_root={len(root)} total={len(all_hands)}", flush=True)

    ordered = Counter(tuple(h["ids"]) for h in all_hands)
    multi = Counter(tuple(sorted(h["ids"])) for h in all_hands)
    by_src_ordered: dict[tuple, Counter] = defaultdict(Counter)
    for h in all_hands:
        by_src_ordered[tuple(h["ids"])][h["src"]] += 1

    # preferred order = most common exact order for each multiset; store all ordered variants separately
    # Normal pool: one entry per ordered triple, weight=clamp(count,1,5)
    entries = []
    for idx, ((a, b, c), count) in enumerate(ordered.most_common(), 1):
        w = min(max(count, 1), 5)
        bid = f"n{idx:03d}"
        entries.append({"id": bid, "ids": [a, b, c], "count": count, "weight": w})

    overlap_ordered = sum(1 for _, c in ordered.items() if c >= 2)
    overlap_multi = sum(1 for _, c in multi.items() if c >= 2)
    shots_overlap_ordered = sum(c for _, c in ordered.items() if c >= 2)
    shots_overlap_multi = sum(c for _, c in multi.items() if c >= 2)

    summary = {
        "blocks2_ok": len(b2),
        "blocks_root_ok": len(root),
        "total_hands": len(all_hands),
        "unique_ordered_bundles": len(ordered),
        "unique_multiset_bundles": len(multi),
        "ordered_bundles_freq_ge2": overlap_ordered,
        "multiset_bundles_freq_ge2": overlap_multi,
        "shots_in_ordered_freq_ge2": shots_overlap_ordered,
        "shots_in_multiset_freq_ge2": shots_overlap_multi,
        "freq_hist_ordered": dict(Counter(ordered.values())),
        "freq_hist_multiset": dict(Counter(multi.values())),
        "top20_ordered": [
            {"ids": list(k), "count": v, "by_src": dict(by_src_ordered[k])}
            for k, v in ordered.most_common(20)
        ],
        "bundle_count_for_normal": len(entries),
    }
    (OUT_DIR / "normal_import_summary.json").write_text(
        json.dumps(summary, indent=2, ensure_ascii=False), encoding="utf-8"
    )
    (OUT_DIR / "normal_import_entries.json").write_text(
        json.dumps(entries, indent=2, ensure_ascii=False), encoding="utf-8"
    )

    # C# snippet
    lines = [
        "        public static List<AreaBundleEntry> CreateNormal()",
        "        {",
        "            return new List<AreaBundleEntry>",
        "            {",
    ]
    for e in entries:
        a, b, c = e["ids"]
        lines.append(
            f'                E("{e["id"]}", {a}, {b}, {c}, {e["weight"]}),'
        )
    lines.append("            };")
    lines.append("        }")
    (OUT_DIR / "normal_import_CreateNormal.cs.txt").write_text(
        "\n".join(lines) + "\n", encoding="utf-8"
    )

    print(json.dumps(summary, indent=2, ensure_ascii=False), flush=True)
    print("wrote normal_import_*.json / CreateNormal.cs.txt", flush=True)


if __name__ == "__main__":
    main()
