"""Match Unique tray pieces via connected-component blob → catalog shape."""
from __future__ import annotations

import json
import os
from collections import Counter, deque
from pathlib import Path

import numpy as np
from PIL import Image

import _extract_hands as eh

SRC = Path(r"C:\Users\hwanj\OneDrive\바탕 화면\Blocks\Unique")
OUT = Path(__file__).resolve().parent / "unique_hands.json"


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
    origin_x: int,
    origin_y: int,
    cell: float,
    blob_w: int,
    blob_h: int,
) -> tuple[int, float, float] | None:
    cells: set[tuple[int, int]] = set()
    fill_sum = 0.0
    half = max(2, int(cell * 0.28))
    for row in range(6):
        for col in range(5):
            cx = int(origin_x + (col + 0.5) * cell)
            cy = int(origin_y + (row + 0.5) * cell)
            if cy < 0 or cx < 0 or cy >= mask.shape[0] or cx >= mask.shape[1]:
                continue
            y0 = max(0, cy - half)
            y1 = min(mask.shape[0], cy + half + 1)
            x0 = max(0, cx - half)
            x1 = min(mask.shape[1], cx + half + 1)
            patch = mask[y0:y1, x0:x1]
            fill = float(patch.mean()) if patch.size else 0.0
            if fill > 0.38:
                cells.add((col, row))
                fill_sum += fill
    sid = eh.match_shape(cells)
    if sid is None:
        return None
    if len(cells) != eh.CELL_COUNTS[sid]:
        return None
    max_c = max(c for c, _ in cells)
    max_r = max(r for _, r in cells)
    pred_w = (max_c + 1) * cell
    pred_h = (max_r + 1) * cell
    # Relative bbox error — reject wild oversize fits.
    err = abs(pred_w - blob_w) / blob_w + abs(pred_h - blob_h) / blob_h
    if err > 0.55:
        return None
    return sid, fill_sum, err


def match_slot(slot_rgb: np.ndarray, bg: np.ndarray) -> int | None:
    diff = np.linalg.norm(slot_rgb.astype(float) - bg, axis=2)
    # Try a few thresholds — Unique UI glow varies by theme.
    for thr in (28.0, 22.0, 34.0, 18.0):
        mask = diff > thr
        if float(mask.mean()) < 0.01:
            continue
        pix = connected_cells(mask)
        if len(pix) < 20:
            continue
        xs = [p[0] for p in pix]
        ys = [p[1] for p in pix]
        x0, x1 = min(xs), max(xs)
        y0, y1 = min(ys), max(ys)
        bw = max(1, x1 - x0 + 1)
        bh = max(1, y1 - y0 + 1)
        best = None
        # Infer cell from blob size against plausible polyomino extents (1..5).
        for cols in range(1, 6):
            for rows in range(1, 7):
                cell = min(bw / cols, bh / rows)
                if cell < 6 or cell > 40:
                    continue
                for ox in range(-3, 4):
                    for oy in range(-3, 4):
                        hit = score_origin(mask, x0 + ox, y0 + oy, cell, bw, bh)
                        if hit is None:
                            continue
                        sid, fill, err = hit
                        expected = eh.CELL_COUNTS[sid]
                        avg = fill / max(1, expected)
                        # Lower bbox error is better.
                        key = (-err, avg, -expected, fill)
                        if best is None or key > best[0]:
                            best = (key, sid)
        if best is not None:
            return best[1]
    return None


def extract_hand(path: Path) -> list[int] | None:
    # Upscale low-res Unique shots to ~540 for stabler blobs (2x).
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
    for cx in eh.slot_centers(w):
        x1 = max(0, cx - half_w)
        x2 = min(w, cx + half_w)
        slot = arr[y1:y2, x1:x2]
        sid = match_slot(slot, bg)
        if sid is None:
            return None
        ids.append(sid)
    return ids


def main() -> None:
    results = []
    success = 0
    triples = Counter()
    shape = Counter()
    for name in sorted(os.listdir(SRC)):
        if not name.lower().endswith(".jpg"):
            continue
        ids = extract_hand(SRC / name)
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
    OUT.write_text(json.dumps(out, indent=2, ensure_ascii=False), encoding="utf-8")
    print("success", success, "/", len(results), flush=True)
    print("freq", out["shape_freq"], flush=True)


if __name__ == "__main__":
    main()
