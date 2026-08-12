"""Re-identify Unique tray hands with stricter blob→shape matching."""
from __future__ import annotations

import json
import os
from collections import Counter, deque
from pathlib import Path

import numpy as np
from PIL import Image

import _extract_hands as eh

SRC = Path(r"C:\Users\hwanj\OneDrive\바탕 화면\Blocks\Unique")
OUT_DIR = Path(__file__).resolve().parent
SLOTS = OUT_DIR / "_unique_slots_v2"
HANDS_OUT = OUT_DIR / "unique_hands_v2.json"


def connected_cells(mask: np.ndarray) -> list[tuple[int, int]]:
    h, w = mask.shape
    seen = np.zeros_like(mask, dtype=bool)
    best: list[tuple[int, int]] = []
    for y in range(h):
        for x in range(w):
            if not mask[y, x] or seen[y, x]:
                continue
            q = deque([(y, x)])
            seen[y, x] = True
            cells: list[tuple[int, int]] = []
            while q:
                cy, cx = q.popleft()
                cells.append((cx, cy))
                for ny, nx in ((cy - 1, cx), (cy + 1, cx), (cy, cx - 1), (cy, cx + 1)):
                    if 0 <= ny < h and 0 <= nx < w and mask[ny, nx] and not seen[ny, nx]:
                        seen[ny, nx] = True
                        q.append((ny, nx))
            if len(cells) > len(best):
                best = cells
    return best


def score_origin(
    mask: np.ndarray,
    origin_x: float,
    origin_y: float,
    cell: float,
    blob_w: int,
    blob_h: int,
    est_cells: float,
) -> tuple[int, float] | None:
    cells: set[tuple[int, int]] = set()
    fill_sum = 0.0
    half = max(2, int(cell * 0.30))
    for row in range(6):
        for col in range(5):
            cx = int(origin_x + (col + 0.5) * cell)
            cy = int(origin_y + (row + 0.5) * cell)
            if cy < 0 or cx < 0 or cy >= mask.shape[0] or cx >= mask.shape[1]:
                continue
            patch = mask[
                max(0, cy - half) : min(mask.shape[0], cy + half + 1),
                max(0, cx - half) : min(mask.shape[1], cx + half + 1),
            ]
            fill = float(patch.mean()) if patch.size else 0.0
            if fill > 0.40:
                cells.add((col, row))
                fill_sum += fill
    if not cells:
        return None
    sid = eh.match_shape(cells)
    if sid is None:
        return None
    expected = eh.CELL_COUNTS[sid]
    if len(cells) != expected:
        return None
    # Reject if detected cell count far from area estimate.
    if est_cells >= 1.5 and abs(expected - est_cells) > 1.6:
        return None
    max_c = max(c for c, _ in cells)
    max_r = max(r for _, r in cells)
    pred_w = (max_c + 1) * cell
    pred_h = (max_r + 1) * cell
    err = abs(pred_w - blob_w) / max(1, blob_w) + abs(pred_h - blob_h) / max(1, blob_h)
    if err > 0.65:
        return None
    avg = fill_sum / expected
    # Prefer closer cell-count to estimate, then denser fill, then lower bbox error.
    score = (-abs(expected - est_cells), avg, -err, -expected)
    return sid, score


def match_slot(slot_rgb: np.ndarray, bg: np.ndarray) -> tuple[int | None, dict]:
    diff = np.linalg.norm(slot_rgb.astype(float) - bg, axis=2)
    debug = {}
    best = None
    for thr in (26.0, 20.0, 32.0, 16.0, 38.0):
        mask = diff > thr
        if float(mask.mean()) < 0.008:
            continue
        pix = connected_cells(mask)
        if len(pix) < 25:
            continue
        xs = [p[0] for p in pix]
        ys = [p[1] for p in pix]
        x0, x1 = min(xs), max(xs)
        y0, y1 = min(ys), max(ys)
        bw = max(1, x1 - x0 + 1)
        bh = max(1, y1 - y0 + 1)
        # Estimate cell size from blob geometry against common extents.
        for cols in range(1, 6):
            for rows in range(1, 7):
                cell = min(bw / cols, bh / rows)
                if cell < 7 or cell > 48:
                    continue
                est_cells = (len(pix) / (cell * cell)) * 0.85
                for ox in range(-4, 5):
                    for oy in range(-4, 5):
                        hit = score_origin(mask, x0 + ox, y0 + oy, cell, bw, bh, est_cells)
                        if hit is None:
                            continue
                        sid, score = hit
                        if best is None or score > best[0]:
                            best = (score, sid, thr, est_cells, cell)
    if best is None:
        return None, debug
    _, sid, thr, est, cell = best
    debug = {"thr": thr, "est_cells": round(est, 2), "cell": round(cell, 2)}
    return sid, debug


def extract_hand(path: Path) -> tuple[list[int] | None, list[dict]]:
    im = Image.open(path).convert("RGB")
    if im.width < 500:
        im = im.resize((im.width * 2, im.height * 2), Image.Resampling.NEAREST)
    arr = np.array(im)
    h, w = arr.shape[:2]
    bg = eh.estimate_background(arr)
    y1 = int(h * 0.735)
    y2 = int(h * 0.895)
    half_w = int(w * 0.155)
    ids: list[int] = []
    dbg: list[dict] = []
    for i, cx in enumerate(eh.slot_centers(w)):
        x1 = max(0, cx - half_w)
        x2 = min(w, cx + half_w)
        slot = arr[y1:y2, x1:x2]
        sid, info = match_slot(slot, bg)
        info = dict(info)
        info["slot"] = i
        if sid is None:
            dbg.append(info)
            return None, dbg
        ids.append(sid)
        info["id"] = sid
        dbg.append(info)
        # also save crop for audit
        SLOTS.mkdir(exist_ok=True)
        Image.fromarray(slot).save(SLOTS / f"{path.stem}_s{i}.png")
    return ids, dbg


def main() -> None:
    results = []
    success = 0
    shape = Counter()
    for name in sorted(os.listdir(SRC)):
        if not name.lower().endswith(".jpg"):
            continue
        ids, dbg = extract_hand(SRC / name)
        if ids is None:
            print(name, "FAIL", dbg, flush=True)
            results.append({"file": name, "note": "fail", "debug": dbg})
            continue
        print(name, ids, flush=True)
        results.append({"file": name, "ids": ids, "debug": dbg})
        success += 1
        for i in ids:
            shape[i] += 1
    out = {
        "processed": len(results),
        "success": success,
        "shape_freq": [{"id": i, "count": n} for i, n in sorted(shape.items(), key=lambda x: (-x[1], x[0]))],
        "hands": results,
    }
    HANDS_OUT.write_text(json.dumps(out, indent=2, ensure_ascii=False), encoding="utf-8")
    print("success", success, "/", len(results), flush=True)
    print("freq", out["shape_freq"], flush=True)


if __name__ == "__main__":
    main()
