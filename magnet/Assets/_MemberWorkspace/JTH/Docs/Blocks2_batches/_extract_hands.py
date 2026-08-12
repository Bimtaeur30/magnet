"""Extract Block Blast 3-piece hands from screenshot batch."""
from __future__ import annotations

import json
import os
import sys
from collections import Counter
from pathlib import Path

import numpy as np
from PIL import Image

ROW_MASKS: dict[int, list[int]] = {
    1: [1],
    2: [1, 1],
    3: [3],
    4: [1, 1, 1],
    5: [7],
    6: [3, 2],
    7: [1, 1, 1, 1],
    8: [4, 7],
    9: [3, 3],
    10: [2, 7],
    11: [31],
    12: [7, 1, 1],
    13: [7, 7, 7],
    14: [3, 6],
    15: [3, 1],
    16: [2, 3, 1],
    17: [15],
    18: [6, 3],
    19: [1, 3, 2],
    20: [2, 3, 2],
    21: [7, 4, 4],
    22: [1, 1, 1, 1, 1],
    23: [4, 4, 7],
    24: [1, 1, 7],
    25: [1, 3, 1],
    26: [7, 2],
    27: [2, 3],
    28: [1, 3],
    29: [1, 1, 3],
    30: [7, 1],
    31: [3, 2, 2],
    32: [3, 1, 1],
    33: [1, 7],
    34: [7, 4],
    35: [7, 7],
    36: [3, 3, 3],
    37: [2, 1],
    38: [1, 2],
    39: [4, 2, 1],
    40: [1, 2, 4],
    41: [1, 2, 4],
    42: [2, 2, 3],
}


def masks_to_cells(rows: list[int]) -> set[tuple[int, int]]:
    cells: set[tuple[int, int]] = set()
    for row, mask in enumerate(rows):
        col = 0
        while mask >> col:
            if mask & (1 << col):
                cells.add((col, row))
            col += 1
    return cells


def normalize_cells(cells: set[tuple[int, int]]) -> frozenset[tuple[int, int]]:
    if not cells:
        return frozenset()
    min_x = min(x for x, _ in cells)
    min_y = min(y for _, y in cells)
    return frozenset((x - min_x, y - min_y) for x, y in cells)


CATALOG: dict[frozenset[tuple[int, int]], list[int]] = {}
CELL_COUNTS: dict[int, int] = {}
for sid, rows in ROW_MASKS.items():
    cells = masks_to_cells(rows)
    key = normalize_cells(cells)
    CATALOG.setdefault(key, []).append(sid)
    CELL_COUNTS[sid] = len(cells)


def estimate_background(arr: np.ndarray):
    samples = np.vstack(
        [
            arr[0, :],
            arr[-1, :],
            arr[:, 0],
            arr[:, -1],
            arr[arr.shape[0] // 2, :],
        ]
    )
    return np.median(samples.reshape(-1, 3), axis=0)


def slot_centers(width: int) -> list[int]:
    return [width // 6, width // 2, 5 * width // 6]


def match_shape(cells: set[tuple[int, int]]) -> int | None:
    key = normalize_cells(cells)
    ids = CATALOG.get(key)
    if not ids:
        return None
    return ids[0]


def score_grid(slot: np.ndarray, origin_x: int, origin_y: int, cell: int) -> tuple[int, int, float] | None:
    cells: set[tuple[int, int]] = set()
    fill_sum = 0.0
    half = max(8, cell // 3)
    for row in range(6):
        for col in range(5):
            cx = origin_x + col * cell + cell // 2
            cy = origin_y + row * cell + cell // 2
            if cy >= slot.shape[0] - 4 or cx >= slot.shape[1] - 4:
                continue
            patch = slot[
                max(0, cy - half) : min(slot.shape[0], cy + half),
                max(0, cx - half) : min(slot.shape[1], cx + half),
            ]
            fill = float(patch.mean())
            if fill > 0.42:
                cells.add((col, row))
                fill_sum += fill
    sid = match_shape(cells)
    if sid is None:
        return None
    expected = CELL_COUNTS[sid]
    if len(cells) != expected:
        return None
    return sid, len(cells), fill_sum


def extract_piece(slot: np.ndarray) -> int | None:
    ys, xs = np.where(slot)
    if len(xs) < 50:
        return None
    x0 = int(xs.min())
    y0 = int(ys.min())
    best = None
    for cell in range(48, 68):
        for ox in range(-12, 13, 2):
            for oy in range(-12, 13, 2):
                result = score_grid(slot, x0 + ox, y0 + oy, cell)
                if result is None:
                    continue
                if best is None or (result[1], result[2]) > (best[1], best[2]):
                    best = result
    return best[0] if best else None


def extract_hand(path: str) -> list[int] | None:
    img = Image.open(path).convert("RGB")
    arr = np.array(img)
    h, w = arr.shape[:2]
    bg = estimate_background(arr)
    diff = np.linalg.norm(arr.astype(float) - bg, axis=2)
    y1 = int(h * 0.735)
    y2 = int(h * 0.895)
    half_w = int(w * 0.155)
    ids: list[int] = []
    for cx in slot_centers(w):
        x1 = max(0, cx - half_w)
        x2 = min(w, cx + half_w)
        slot = diff[y1:y2, x1:x2] > 32.0
        if slot.mean() < 0.02:
            return None
        sid = extract_piece(slot)
        if sid is None:
            slot2 = diff[y1:y2, x1:x2] > 24.0
            sid = extract_piece(slot2)
        if sid is None:
            return None
        ids.append(sid)
    return ids


def process_batch(batch_txt: str, out_json: str) -> None:
    paths = [line.strip() for line in Path(batch_txt).read_text(encoding="utf-8").splitlines() if line.strip()]
    results = []
    success = 0
    for path in paths:
        entry = {"file": os.path.basename(path), "path": path}
        ids = extract_hand(path)
        if ids is None:
            entry["note"] = "could not detect 3 pieces"
            results.append(entry)
            continue
        entry["ids"] = ids
        results.append(entry)
        success += 1
    counter = Counter(tuple(r["ids"]) for r in results if "ids" in r)
    summary = {
        "processed": len(paths),
        "success": success,
        "top_triples": [{"ids": list(k), "count": v} for k, v in counter.most_common(30)],
        "hands": results,
    }
    Path(out_json).write_text(json.dumps(summary, indent=2, ensure_ascii=False), encoding="utf-8")
    print(json.dumps({"processed": len(paths), "success": success}, indent=2))


if __name__ == "__main__":
    process_batch(sys.argv[1], sys.argv[2])
