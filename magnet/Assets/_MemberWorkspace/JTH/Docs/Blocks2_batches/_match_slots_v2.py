#!/usr/bin/env python3
"""Match unique slot crops to BlockBlastCatalog ShapeIds."""
from __future__ import annotations

import json
from pathlib import Path

import numpy as np
from PIL import Image

ROW_MASKS = {
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


def masks_to_cells(masks: list[int]) -> frozenset[tuple[int, int]]:
    cells = set()
    for r, m in enumerate(masks):
        c = 0
        while m >> c:
            if m & (1 << c):
                cells.add((c, r))
            c += 1
    min_x = min(x for x, _ in cells)
    min_y = min(y for _, y in cells)
    return frozenset((x - min_x, y - min_y) for x, y in cells)


CATALOG_BY_SHAPE: dict[frozenset[tuple[int, int]], list[int]] = {}
for sid, masks in ROW_MASKS.items():
    CATALOG_BY_SHAPE.setdefault(masks_to_cells(masks), []).append(sid)


def bg_dist(arr: np.ndarray) -> np.ndarray:
    border = np.concatenate([arr[0, :], arr[-1, :], arr[:, 0], arr[:, -1]], axis=0)
    bg = np.median(border, axis=0)
    return np.linalg.norm(arr - bg, axis=2)


def autocorr_lag(line: np.ndarray, lo: int = 18, hi: int = 40) -> int | None:
    x = line - line.mean()
    best_lag = None
    best_v = -1e18
    for lag in range(lo, min(hi, len(x) // 2)):
        v = float(np.dot(x[:-lag], x[lag:]))
        if v > best_v:
            best_v = v
            best_lag = lag
    return best_lag


def ascii_shape(cells: frozenset[tuple[int, int]] | None) -> str:
    if not cells:
        return ""
    max_x = max(x for x, _ in cells)
    max_y = max(y for _, y in cells)
    return "/".join(
        "".join("#" if (x, y) in cells else "." for x in range(max_x + 1))
        for y in range(max_y + 1)
    )


def match_id(cells: frozenset[tuple[int, int]] | None) -> tuple[int | None, list[int]]:
    if cells is None:
        return None, []
    ids = CATALOG_BY_SHAPE.get(cells, [])
    return (ids[0] if ids else None), ids


def extract_cells(path: Path) -> tuple[frozenset[tuple[int, int]] | None, dict]:
    im = Image.open(path).convert("RGB")
    arr = np.asarray(im).astype(np.float32)
    dist = bg_dist(arr)
    meta: dict = {
        "path": path.name,
        "size": list(im.size),
        "dist_max": float(dist.max()),
        "dist_p95": float(np.percentile(dist, 95)),
    }

    # Empty / ultra-low-contrast crop (dragged piece, bad extract)
    if dist.max() < 35:
        meta["error"] = "low_contrast_empty"
        return None, meta

    mask = dist > max(45.0, float(np.percentile(dist, 70)))
    # Fallback softer threshold
    if mask.sum() < 80:
        mask = dist > 35
    meta["fg_pixels"] = int(mask.sum())
    if mask.sum() < 40:
        meta["error"] = "too_few_fg"
        return None, meta

    ys, xs = np.where(mask)
    x0, x1 = int(xs.min()), int(xs.max())
    y0, y1 = int(ys.min()), int(ys.max())
    bw, bh = x1 - x0 + 1, y1 - y0 + 1
    meta["bbox"] = [x0, y0, bw, bh]
    crop = mask[y0 : y1 + 1, x0 : x1 + 1]

    # Straight bar via autocorr
    aspect = bw / max(bh, 1)
    if aspect >= 1.6 and bh <= 45:
        mid = (y0 + y1) // 2
        band = arr[max(0, mid - 2) : mid + 3, x0 : x1 + 1]
        line = (0.299 * band[..., 0] + 0.587 * band[..., 1] + 0.114 * band[..., 2]).mean(0)
        lag = autocorr_lag(line)
        n = int(round(bw / lag)) if lag else int(round(aspect))
        n = max(2, min(5, n))
        cells = frozenset((c, 0) for c in range(n))
        meta.update({"method": "horiz_bar", "lag": lag, "n": n})
        return cells, meta

    if aspect <= 0.65 and bw <= 45:
        mid = (x0 + x1) // 2
        band = arr[y0 : y1 + 1, max(0, mid - 2) : mid + 3]
        line = (0.299 * band[..., 0] + 0.587 * band[..., 1] + 0.114 * band[..., 2]).mean(1)
        lag = autocorr_lag(line)
        n = int(round(bh / lag)) if lag else int(round(1 / max(aspect, 1e-6)))
        n = max(2, min(5, n))
        cells = frozenset((0, r) for r in range(n))
        meta.update({"method": "vert_bar", "lag": lag, "n": n})
        return cells, meta

    # Estimate cell pitch
    lag_x = autocorr_lag(
        (0.299 * arr[(y0 + y1) // 2, x0 : x1 + 1, 0]
         + 0.587 * arr[(y0 + y1) // 2, x0 : x1 + 1, 1]
         + 0.114 * arr[(y0 + y1) // 2, x0 : x1 + 1, 2]),
        16,
        40,
    )
    pitch = float(lag_x) if lag_x else float(np.median([bw / 3, bh / 3, 26]))
    pitch = float(np.clip(pitch, 18, 40))

    best_cells = None
    best_score = -1e18
    best_extra = {}
    for ncols in range(1, 6):
        for nrows in range(1, 6):
            cell_w = bw / ncols
            cell_h = bh / nrows
            if abs(cell_w - pitch) > 10 and abs(cell_h - pitch) > 10:
                # allow if both near each other
                if not (18 <= cell_w <= 40 and 18 <= cell_h <= 40):
                    continue
            if cell_w < 18 or cell_h < 18 or cell_w > 42 or cell_h > 42:
                continue
            aspect_c = cell_w / cell_h
            if aspect_c < 0.72 or aspect_c > 1.38:
                continue

            cells = set()
            fracs = []
            for ry in range(nrows):
                for cx in range(ncols):
                    xa = int(round(cx * cell_w + cell_w * 0.22))
                    xb = int(round(cx * cell_w + cell_w * 0.78))
                    ya = int(round(ry * cell_h + cell_h * 0.22))
                    yb = int(round(ry * cell_h + cell_h * 0.78))
                    patch = crop[ya:yb, xa:xb]
                    frac = float(patch.mean()) if patch.size else 0.0
                    fracs.append(frac)
                    if frac > 0.40:
                        cells.add((cx, ry))
            if not cells:
                continue
            min_x = min(c for c, _ in cells)
            min_y = min(r for _, r in cells)
            norm = frozenset((c - min_x, r - min_y) for c, r in cells)
            if norm not in CATALOG_BY_SHAPE:
                continue
            occ_w = max(c for c, _ in cells) - min_x + 1
            occ_h = max(r for _, r in cells) - min_y + 1
            if occ_w != ncols or occ_h != nrows:
                continue
            filled = [f for f in fracs if f > 0.40]
            empty = [f for f in fracs if f <= 0.40]
            if empty and float(np.mean(empty)) > 0.22:
                continue
            if filled and float(np.min(filled)) < 0.50:
                continue
            sep = (float(np.mean(filled)) if filled else 0) - (float(np.mean(empty)) if empty else 1)
            # Prefer pitch agreement and more cells (avoid undercount)
            pitch_pen = abs(cell_w - pitch) + abs(cell_h - pitch)
            score = sep * 8 + len(norm) * 1.2 - 0.08 * pitch_pen
            if score > best_score:
                best_score = score
                best_cells = norm
                best_extra = {
                    "method": "grid",
                    "nrows": nrows,
                    "ncols": ncols,
                    "pitch": pitch,
                    "score": score,
                    "sep": sep,
                }

    # 1x1 fallback: compact nearly-square blob
    if best_cells is None and 20 <= bw <= 40 and 20 <= bh <= 40 and abs(bw - bh) <= 8:
        best_cells = frozenset([(0, 0)])
        best_extra = {"method": "mono"}

    meta.update(best_extra)
    meta["cells"] = sorted(best_cells) if best_cells else None
    meta["ascii"] = ascii_shape(best_cells)
    return best_cells, meta


def main() -> None:
    root = Path(__file__).resolve().parent / "_unique_slots_v2"
    stems = sorted({p.name.split("_")[0] for p in root.glob("*_s0.png")}, key=int)
    hands = []
    details = []
    for stem in stems:
        ids = []
        notes = []
        for slot in range(3):
            path = root / f"{stem}_s{slot}.png"
            cells, meta = extract_cells(path)
            sid, all_ids = match_id(cells)
            meta["ascii"] = ascii_shape(cells)
            meta["match"] = sid
            meta["all_ids"] = all_ids
            details.append(meta)
            if sid is None:
                ids.append(-1)
                notes.append(f"s{slot}:{meta.get('error') or meta.get('ascii') or '?'}")
            else:
                ids.append(sid)
        entry = {"file": f"{stem}.jpg", "ids": ids}
        if notes:
            entry["notes"] = "; ".join(notes)
        hands.append(entry)

    out_path = Path(__file__).resolve().parent / "unique_hands_visual_auto.json"
    out_path.write_text(json.dumps({"hands": hands, "count": len(hands)}, indent=2), encoding="utf-8")
    Path(__file__).resolve().parent.joinpath("_unique_slots_v2_match_detail.json").write_text(
        json.dumps(details, indent=2), encoding="utf-8"
    )
    for h in hands:
        print(h["file"], h["ids"], h.get("notes", ""))
    print("unmatched", sum(1 for h in hands if -1 in h["ids"]))


if __name__ == "__main__":
    main()
