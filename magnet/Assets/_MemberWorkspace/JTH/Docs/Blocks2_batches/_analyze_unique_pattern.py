"""Analyze Unique screenshots: duplicate hands + unlock-pattern match vs user's rule."""
from __future__ import annotations

import json
from collections import Counter
from pathlib import Path

import numpy as np
from PIL import Image

import _extract_hands as eh
import _extract_unique_blob as blob

SRC = Path(r"C:\Users\hwanj\OneDrive\바탕 화면\Blocks\Unique")
HANDS = Path(__file__).resolve().parent / "unique_hands.json"
OUT = Path(__file__).resolve().parent / "unique_pattern_analysis.json"

SIZE = 8


def extract_board(path: Path) -> np.ndarray | None:
    """Return 8x8 bool occupied (True=filled). Uses tray-relative geometry."""
    im = Image.open(path).convert("RGB")
    if im.width < 500:
        im = im.resize((im.width * 2, im.height * 2), Image.Resampling.NEAREST)
    arr = np.array(im)
    h, w = arr.shape[:2]
    # Calibrated on Unique/1972.jpg (2x upscaled): exact 8x8 match.
    y0 = int(h * 0.215)
    y1 = int(h * 0.66)
    x0 = int(w * 0.09)
    x1 = int(w * 0.89)
    board = arr[y0:y1, x0:x1]
    bh, bw = board.shape[:2]
    # Empty cell luminance is low; gems are bright.
    lum = board.astype(np.float32).mean(axis=2)
    # Estimate empty via darkest quartile of cell samples after grid split.
    cell_h = bh / SIZE
    cell_w = bw / SIZE
    samples = []
    centers = []
    for r in range(SIZE):
        for c in range(SIZE):
            cy = int((r + 0.5) * cell_h)
            cx = int((c + 0.5) * cell_w)
            # Sample inner 40% of cell to avoid grid lines.
            half_y = max(1, int(cell_h * 0.18))
            half_x = max(1, int(cell_w * 0.18))
            patch = lum[cy - half_y : cy + half_y + 1, cx - half_x : cx + half_x + 1]
            val = float(patch.mean()) if patch.size else 0.0
            samples.append(val)
            centers.append((r, c, val))
    # Threshold: midpoint between dark cluster and bright cluster.
    vals = np.array(samples)
    # Otsu-like: try thresholds, maximize between-class variance.
    best_t, best_score = 40.0, -1.0
    for t in np.linspace(vals.min() + 1, vals.max() - 1, 40):
        w0 = (vals <= t).mean()
        w1 = 1.0 - w0
        if w0 < 0.05 or w1 < 0.05:
            continue
        m0 = vals[vals <= t].mean()
        m1 = vals[vals > t].mean()
        score = w0 * w1 * (m0 - m1) ** 2
        if score > best_score:
            best_score = score
            best_t = float(t)
    occ = np.zeros((SIZE, SIZE), dtype=bool)
    for r, c, val in centers:
        occ[r, c] = val > best_t
    return occ


def offsets(sid: int) -> list[tuple[int, int]]:
    rows = eh.ROW_MASKS[sid]
    cells = []
    for y, mask in enumerate(rows):
        col = 0
        while mask >> col:
            if mask & (1 << col):
                cells.append((col, y))
            col += 1
    return cells


def in_bounds(x: int, y: int) -> bool:
    return 0 <= x < SIZE and 0 <= y < SIZE


def can_place(board: np.ndarray, offs: list[tuple[int, int]], px: int, py: int) -> bool:
    for ox, oy in offs:
        x, y = px + ox, py + oy
        if not in_bounds(x, y) or board[y, x]:
            return False
    return True


def can_place_anywhere(board: np.ndarray, offs: list[tuple[int, int]]) -> bool:
    for px in range(SIZE):
        for py in range(SIZE):
            if can_place(board, offs, px, py):
                return True
    return False


def place_and_clear(board: np.ndarray, offs: list[tuple[int, int]], px: int, py: int) -> tuple[np.ndarray, int]:
    b = board.copy()
    placed: list[tuple[int, int]] = []
    for ox, oy in offs:
        x, y = px + ox, py + oy
        b[y, x] = True
        placed.append((x, y))

    rows = sorted({y for _, y in placed if all(b[y, xx] for xx in range(SIZE))})
    cols = sorted({x for x, _ in placed if all(b[yy, x] for yy in range(SIZE))})
    for y in rows:
        b[y, :] = False
    for x in cols:
        b[:, x] = False
    return b, len(rows) + len(cols)


def placements(board: np.ndarray, offs: list[tuple[int, int]]) -> list[tuple[int, int]]:
    out = []
    for px in range(SIZE):
        for py in range(SIZE):
            if can_place(board, offs, px, py):
                out.append((px, py))
    return out


def alone_unlocks(board: np.ndarray, piece: list[tuple[int, int]], blocked: list[tuple[int, int]]) -> bool:
    """True if some single placement of piece (with clears) makes blocked placeable."""
    for px, py in placements(board, piece):
        after, _ = place_and_clear(board, piece, px, py)
        if can_place_anywhere(after, blocked):
            return True
    return False


def both_unlock(
    board: np.ndarray,
    a: list[tuple[int, int]],
    b: list[tuple[int, int]],
    blocked: list[tuple[int, int]],
) -> bool:
    """True if some order a->b or b->a with total clears>=1 opens blocked."""
    for first, second in ((a, b), (b, a)):
        for p1 in placements(board, first):
            after1, c1 = place_and_clear(board, first, *p1)
            for p2 in placements(after1, second):
                after2, c2 = place_and_clear(after1, second, *p2)
                if c1 + c2 < 1:
                    continue
                if can_place_anywhere(after2, blocked):
                    return True
    return False


def classify_hand(board: np.ndarray, ids: list[int]) -> dict:
    offs = [offsets(i) for i in ids]
    placeable = [can_place_anywhere(board, o) for o in offs]
    blocked_idxs = [i for i, ok in enumerate(placeable) if not ok]
    unlock_idxs = [i for i, ok in enumerate(placeable) if ok]

    result = {
        "ids": ids,
        "placeable": placeable,
        "blocked_count": len(blocked_idxs),
        "has_duplicate": len(set(ids)) < 3,
        "duplicate_ids": [i for i in set(ids) if ids.count(i) > 1],
    }

    # User rule (strong A): exactly 1 blocked, 2 placeable unlocks;
    # neither unlock alone opens blocked; both with >=1 clear do.
    if len(blocked_idxs) == 1 and len(unlock_idxs) == 2:
        bi = blocked_idxs[0]
        u0, u1 = unlock_idxs
        blocked = offs[bi]
        a, b = offs[u0], offs[u1]
        alone0 = alone_unlocks(board, a, blocked)
        alone1 = alone_unlocks(board, b, blocked)
        both = both_unlock(board, a, b, blocked)
        result.update(
            {
                "pattern": "one_blocked_two_free",
                "blocked_id": ids[bi],
                "unlock_ids": [ids[u0], ids[u1]],
                "alone0": alone0,
                "alone1": alone1,
                "both_unlock": both,
                "matches_user_rule": (not alone0) and (not alone1) and both,
                "matches_weak_old": both,  # old generator: both unlock somehow, ignore alone
            }
        )
    elif len(blocked_idxs) == 0:
        result["pattern"] = "all_placeable"
        result["matches_user_rule"] = False
        result["matches_weak_old"] = False
    elif len(blocked_idxs) >= 2:
        result["pattern"] = "multi_blocked"
        result["matches_user_rule"] = False
        result["matches_weak_old"] = False
    else:
        result["pattern"] = "other"
        result["matches_user_rule"] = False
        result["matches_weak_old"] = False
    return result


def main() -> None:
    hands_doc = json.loads(HANDS.read_text(encoding="utf-8"))
    id_map = {h["file"]: h["ids"] for h in hands_doc["hands"] if "ids" in h}

    rows = []
    for name in sorted(p.name for p in SRC.glob("*.jpg")):
        ids = id_map.get(name)
        if not ids:
            rows.append({"file": name, "error": "no_ids"})
            continue
        board = extract_board(SRC / name)
        if board is None:
            rows.append({"file": name, "error": "no_board", "ids": ids})
            continue
        # Quick sanity: occupied count
        occ = int(board.sum())
        info = classify_hand(board, ids)
        info["file"] = name
        info["occupied"] = occ
        rows.append(info)
        print(
            name,
            ids,
            info.get("pattern"),
            "dup" if info["has_duplicate"] else "",
            "MATCH" if info.get("matches_user_rule") else "no",
            flush=True,
        )

    valid = [r for r in rows if "pattern" in r]
    dup = [r for r in valid if r["has_duplicate"]]
    match = [r for r in valid if r.get("matches_user_rule")]
    weak = [r for r in valid if r.get("matches_weak_old")]
    patterns = Counter(r["pattern"] for r in valid)

    # Sub-breakdown for one_blocked_two_free
    obtf = [r for r in valid if r["pattern"] == "one_blocked_two_free"]
    alone_either = [r for r in obtf if r.get("alone0") or r.get("alone1")]
    both_ok = [r for r in obtf if r.get("both_unlock")]
    need_both = [r for r in obtf if r.get("matches_user_rule")]

    summary = {
        "total_files": len(rows),
        "analyzed": len(valid),
        "duplicate_hands": len(dup),
        "duplicate_rate": round(len(dup) / max(1, len(valid)), 3),
        "pattern_counts": dict(patterns),
        "one_blocked_two_free": len(obtf),
        "obtf_alone_can_unlock": len(alone_either),
        "obtf_both_unlock": len(both_ok),
        "matches_user_rule_strong_A": len(need_both),
        "matches_weak_old_both_only": len(weak),
        "non_match": len(valid) - len(need_both),
        "duplicate_files": [r["file"] for r in dup],
        "match_files": [r["file"] for r in need_both],
        "obtf_but_alone_unlocks": [r["file"] for r in alone_either],
        "hands": rows,
    }
    OUT.write_text(json.dumps(summary, indent=2, ensure_ascii=False), encoding="utf-8")
    print(json.dumps({k: summary[k] for k in summary if k != "hands"}, indent=2), flush=True)


if __name__ == "__main__":
    main()
