#!/usr/bin/env python3
"""Extract Block Blast 3-piece hands from screenshot batch → 42-IDs JSON."""

import json
import os
import sys
from collections import Counter
from pathlib import Path

import numpy as np
from PIL import Image

ROW_MASKS = {
    1: [1], 2: [1, 1], 3: [3], 4: [1, 1, 1], 5: [7],
    6: [3, 2], 7: [1, 1, 1, 1], 8: [4, 7], 9: [3, 3], 10: [2, 7],
    11: [31], 12: [7, 1, 1], 13: [7, 7, 7], 14: [3, 6], 15: [3, 1],
    16: [2, 3, 1], 17: [15], 18: [6, 3], 19: [1, 3, 2], 20: [2, 3, 2],
    21: [7, 4, 4], 22: [1, 1, 1, 1, 1], 23: [4, 4, 7], 24: [1, 1, 7],
    25: [1, 3, 1], 26: [7, 2], 27: [2, 3], 28: [1, 3], 29: [1, 1, 3],
    30: [7, 1], 31: [3, 2, 2], 32: [3, 1, 1], 33: [1, 7], 34: [7, 4],
    35: [7, 7], 36: [3, 3, 3], 37: [2, 1], 38: [1, 2], 39: [4, 2, 1],
    40: [1, 2, 4], 41: [1, 2, 4], 42: [2, 2, 3],
}

MASK_TO_ID: dict[tuple[int, ...], list[int]] = {}
for pid, rows in ROW_MASKS.items():
    MASK_TO_ID.setdefault(tuple(rows), []).append(pid)


def block_color_mask(rgb: np.ndarray) -> np.ndarray:
    r = rgb[:, :, 0].astype(np.float32)
    g = rgb[:, :, 1].astype(np.float32)
    b = rgb[:, :, 2].astype(np.float32)
    yellow = (r > 180) & (g > 160) & ((r + g) / 2 > b + 30)
    pink = (r > 165) & (r > g + 35) & (g > 100) & (g < 200) & (b < 210)
    orange = (r > 180) & (g > 100) & (g < 170) & (r > b + 40)
    return yellow | pink | orange


def find_hand_band(img: np.ndarray) -> tuple[int, int]:
    h = img.shape[0]
    y_scan0 = int(h * 0.68)
    scan = img[y_scan0 : int(h * 0.90)]
    row_counts = block_color_mask(scan).sum(axis=1)
    if row_counts.max() == 0:
        return int(h * 0.752), int(h * 0.805)

    active = row_counts > 50
    clusters: list[tuple[int, int, float]] = []
    i = 0
    while i < len(active):
        if not active[i]:
            i += 1
            continue
        j = i
        while j < len(active) and active[j]:
            j += 1
        clusters.append((i, j, float(row_counts[i:j].max())))
        i = j

    if not clusters:
        return int(h * 0.752), int(h * 0.805)

    # Merge clusters separated by <=4 inactive rows.
    merged: list[list] = [[clusters[0][0], clusters[0][1], clusters[0][2]]]
    for i0, i1, peak in clusters[1:]:
        if i0 - merged[-1][1] <= 4:
            merged[-1][1] = i1
            merged[-1][2] = max(merged[-1][2], peak)
        else:
            merged.append([i0, i1, peak])

    cutoff = int(h * 0.72) - y_scan0
    candidates = [m for m in merged if m[0] >= cutoff] or merged
    i0, i1, _ = max(candidates, key=lambda m: m[2])
    pad = 5
    y0 = y_scan0 + max(0, i0 - pad)
    y1 = y_scan0 + min(scan.shape[0], i1 + pad)
    return y0, y1


def tight_bbox(mask: np.ndarray, frac: float = 0.35) -> tuple[int, int, int, int] | None:
    row_counts = mask.sum(axis=1)
    col_counts = mask.sum(axis=0)
    if row_counts.max() == 0 or col_counts.max() == 0:
        return None
    rows = row_counts >= row_counts.max() * frac
    cols = col_counts >= col_counts.max() * frac
    ys = np.where(rows)[0]
    xs = np.where(cols)[0]
    if len(ys) == 0 or len(xs) == 0:
        return None
    return int(ys[0]), int(ys[-1]), int(xs[0]), int(xs[-1])


def grid_to_row_masks(grid: np.ndarray) -> tuple[int, ...]:
    out = []
    for row in grid:
        mask = 0
        for col, val in enumerate(row):
            if val:
                mask |= 1 << col
        out.append(mask)
    return tuple(out)


def match_id(grid: np.ndarray) -> int | None:
    key = grid_to_row_masks(grid.astype(bool))
    ids = MASK_TO_ID.get(key)
    return ids[0] if ids else None


def sample_grid(crop: np.ndarray, nrows: int, ncols: int) -> np.ndarray:
    h, w = crop.shape
    grid = np.zeros((nrows, ncols), dtype=bool)
    for r in range(nrows):
        for c in range(ncols):
            y0 = int(r * h / nrows)
            y1 = int((r + 1) * h / nrows)
            x0 = int(c * w / ncols)
            x1 = int((c + 1) * w / ncols)
            patch = crop[y0:y1, x0:x1]
            grid[r, c] = patch.size > 0 and patch.mean() > 0.40
    return grid


CELL_COUNTS = {pid: sum(bin(row).count("1") for row in rows) for pid, rows in ROW_MASKS.items()}


def mask_to_grid(piece_mask: np.ndarray) -> np.ndarray | None:
    bbox = tight_bbox(piece_mask)
    if bbox is None:
        return None
    y0, y1, x0, x1 = bbox
    crop = piece_mask[y0 : y1 + 1, x0 : x1 + 1]
    if not crop.any():
        return None

    h, w = crop.shape
    aspect = w / max(h, 1)
    best_grid = None
    best_aspect_err = 1e9
    best_cells = 0
    for nrows in range(1, 6):
        for ncols in range(1, 6):
            grid = sample_grid(crop, nrows, ncols)
            filled = int(grid.sum())
            if filled == 0:
                continue
            pid = match_id(grid)
            if pid is None or filled != CELL_COUNTS[pid]:
                continue
            grid_aspect = ncols / nrows
            aspect_err = abs(np.log((aspect + 1e-6) / (grid_aspect + 1e-6)))
            if aspect_err < best_aspect_err or (
                abs(aspect_err - best_aspect_err) < 0.15 and filled > best_cells
            ):
                best_aspect_err = aspect_err
                best_cells = filled
                best_grid = grid
    return best_grid


def extract_hand(path: str) -> list[int] | None:
    img = np.array(Image.open(path).convert("RGB"))
    h, w = img.shape[:2]
    y0, y1 = find_hand_band(img)
    hand_rgb = img[y0:y1, :, :]
    mask = block_color_mask(hand_rgb)

    slot_w = w // 3
    ids: list[int] = []
    for slot in range(3):
        x0 = slot * slot_w
        x1 = (slot + 1) * slot_w if slot < 2 else w
        grid = mask_to_grid(mask[:, x0:x1])
        if grid is None:
            return None
        pid = match_id(grid)
        if pid is None:
            return None
        ids.append(pid)
    return ids


def main():
    batch_txt = sys.argv[1]
    out_json = sys.argv[2]
    paths = [ln.strip() for ln in Path(batch_txt).read_text(encoding="utf-8").splitlines() if ln.strip()]
    results = []
    success = 0
    triple_counter: Counter = Counter()

    for path in paths:
        basename = os.path.basename(path)
        entry: dict = {"file": basename}
        if not os.path.isfile(path):
            entry["ids"] = None
            entry["note"] = "file not found"
            results.append(entry)
            continue
        try:
            ids = extract_hand(path)
            if ids and len(ids) == 3:
                entry["ids"] = ids
                triple_counter[tuple(ids)] += 1
                success += 1
            else:
                entry["ids"] = None
                entry["note"] = "could not detect 3 pieces"
        except Exception as exc:
            entry["ids"] = None
            entry["note"] = str(exc)
        results.append(entry)

    Path(out_json).write_text(json.dumps(results, indent=2, ensure_ascii=False), encoding="utf-8")

    print(f"processed={len(paths)} success={success}")
    print("top_triples:")
    for triple, count in triple_counter.most_common(10):
        print(f"  {list(triple)} x{count}")


if __name__ == "__main__":
    main()
