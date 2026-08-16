"""Aggregate Blocks + Blocks2 hands, exclude Unique folder & 2-cell pieces, analyze freq."""
from __future__ import annotations

import json
import os
import sys
from collections import Counter
from pathlib import Path

from PIL import Image

sys.path.insert(0, str(Path(__file__).resolve().parent))
import _extract_hands as eh

DESKTOP = None


def desktop_dir() -> Path:
    global DESKTOP
    if DESKTOP is not None:
        return DESKTOP
    import ctypes
    import ctypes.wintypes

    buf = ctypes.create_unicode_buffer(ctypes.wintypes.MAX_PATH)
    ctypes.windll.shell32.SHGetFolderPathW(None, 0, None, 0, buf)
    DESKTOP = Path(buf.value)
    return DESKTOP


def blocks_dir() -> Path:
    return desktop_dir() / "Blocks"


OUT_DIR = Path(__file__).resolve().parent
TWO_CELL = {sid for sid, n in eh.CELL_COUNTS.items() if n == 2}


def load_blocks2_batches() -> list[dict]:
    hands: list[dict] = []
    for p in sorted(OUT_DIR.glob("batch*_hands.json")):
        data = json.loads(p.read_text(encoding="utf-8"))
        # array form
        if isinstance(data, list):
            for e in data:
                if "ids" in e:
                    hands.append({"file": e["file"], "ids": e["ids"], "src": "Blocks2", "batch": p.name})
        else:
            for e in data.get("hands", []):
                if "ids" in e:
                    hands.append({"file": e["file"], "ids": e["ids"], "src": "Blocks2", "batch": p.name})
    return hands


def extract_root_blocks() -> list[dict]:
    tmp = OUT_DIR / "_blocks_root_up"
    tmp.mkdir(exist_ok=True)
    blocks = blocks_dir()
    unique_names = set()
    uq = blocks / "Unique"
    if uq.is_dir():
        unique_names = {n.lower() for n in os.listdir(uq) if n.lower().endswith(".jpg")}

    hands: list[dict] = []
    files = sorted([n for n in os.listdir(blocks) if n.lower().endswith(".jpg")])
    for name in files:
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
            print(name, "FAIL", flush=True)
            continue
        hands.append({"file": name, "ids": ids, "src": "Blocks"})
        print(name, ids, flush=True)
    return hands


def has_two_cell(ids: list[int]) -> bool:
    return any(i in TWO_CELL for i in ids)


def multiset_key(ids: list[int]) -> tuple[int, int, int]:
    return tuple(sorted(ids))


def order_key(ids: list[int]) -> tuple[int, int, int]:
    return tuple(ids)


def analyze(hands: list[dict]) -> dict:
    ok = [h for h in hands if "ids" in h]
    fail = [h for h in hands if "ids" not in h]
    with_2 = [h for h in ok if has_two_cell(h["ids"])]
    kept = [h for h in ok if not has_two_cell(h["ids"])]

    # ordered triple freq
    ordered = Counter(order_key(h["ids"]) for h in kept)
    # multiset freq (order-invariant)
    multi = Counter(multiset_key(h["ids"]) for h in kept)

    by_src = Counter(h["src"] for h in kept)
    shape = Counter()
    for h in kept:
        for i in h["ids"]:
            shape[i] += 1

    # overlap: same ordered triple appearing more than once
    ordered_overlap = [(list(k), v) for k, v in ordered.most_common() if v >= 2]
    multi_overlap = [(list(k), v) for k, v in multi.most_common() if v >= 2]

    # how many screenshots are in overlapping groups
    shots_in_ordered_overlap = sum(v for _, v in ordered_overlap)
    shots_in_multi_overlap = sum(v for _, v in multi_overlap)

    return {
        "two_cell_ids": sorted(TWO_CELL),
        "total_raw_ok": len(ok),
        "total_fail": len(fail),
        "excluded_two_cell_hands": len(with_2),
        "kept_hands": len(kept),
        "kept_by_src": dict(by_src),
        "unique_ordered_triples": len(ordered),
        "unique_multiset_triples": len(multi),
        "ordered_freq_ge2_count": len(ordered_overlap),
        "multiset_freq_ge2_count": len(multi_overlap),
        "shots_in_ordered_freq_ge2": shots_in_ordered_overlap,
        "shots_in_multiset_freq_ge2": shots_in_multi_overlap,
        "top_ordered": [{"ids": ids, "count": c} for ids, c in ordered.most_common(40)],
        "top_multiset": [{"ids": ids, "count": c} for ids, c in multi.most_common(40)],
        "shape_freq": [{"id": i, "count": n} for i, n in sorted(shape.items(), key=lambda x: (-x[1], x[0]))],
        "freq_histogram_ordered": dict(Counter(ordered.values())),
        "freq_histogram_multiset": dict(Counter(multi.values())),
        "kept_hands_detail": kept,
        "excluded_two_cell_detail": [{"file": h["file"], "ids": h["ids"], "src": h["src"]} for h in with_2],
    }


def main() -> None:
    print("TWO_CELL", sorted(TWO_CELL), flush=True)
    print("Loading Blocks2 batches...", flush=True)
    b2 = load_blocks2_batches()
    print(f"Blocks2 ok-ish entries: {sum(1 for h in b2 if 'ids' in h)}", flush=True)
    print("Extracting root Blocks (exclude Unique names)...", flush=True)
    root = extract_root_blocks()
    all_hands = b2 + root
    report = analyze(all_hands)
    # write compact summary without full detail first
    summary = {k: v for k, v in report.items() if k not in ("kept_hands_detail", "excluded_two_cell_detail")}
    out_summary = OUT_DIR / "blocks_import_overlap_summary.json"
    out_full = OUT_DIR / "blocks_import_overlap_full.json"
    out_summary.write_text(json.dumps(summary, indent=2, ensure_ascii=False), encoding="utf-8")
    out_full.write_text(json.dumps(report, indent=2, ensure_ascii=False), encoding="utf-8")
    print("=== SUMMARY ===", flush=True)
    for k in [
        "total_raw_ok",
        "total_fail",
        "excluded_two_cell_hands",
        "kept_hands",
        "kept_by_src",
        "unique_ordered_triples",
        "unique_multiset_triples",
        "ordered_freq_ge2_count",
        "multiset_freq_ge2_count",
        "shots_in_ordered_freq_ge2",
        "shots_in_multiset_freq_ge2",
        "freq_histogram_ordered",
        "freq_histogram_multiset",
    ]:
        print(k, summary.get(k), flush=True)
    print("top_ordered", summary["top_ordered"][:15], flush=True)
    print("top_multiset", summary["top_multiset"][:15], flush=True)
    print("wrote", out_summary, flush=True)


if __name__ == "__main__":
    main()
