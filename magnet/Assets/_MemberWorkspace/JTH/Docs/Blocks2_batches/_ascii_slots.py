#!/usr/bin/env python3
from pathlib import Path
import numpy as np
from PIL import Image

STEMS = [
    "1994", "1996", "1998", "2002", "2016", "2018", "2020", "2022", "2024",
    "2026", "2028", "2030", "2032", "2036", "2038", "2040", "2042", "2044",
    "2046", "2076", "2078", "2080", "2082", "2084", "2086", "2092", "2095",
    "2097", "2099", "2101", "2103", "2105", "2107", "2109",
]


def lag_of(line: np.ndarray) -> int | None:
    x = line.astype(np.float64) - line.mean()
    best = None
    bv = -1e18
    for lag in range(18, min(40, len(x) // 2)):
        v = float(np.dot(x[:-lag], x[lag:]))
        if v > bv:
            bv = v
            best = lag
    return best


def show(path: Path) -> None:
    arr = np.asarray(Image.open(path).convert("RGB")).astype(np.float32)
    border = np.concatenate([arr[0, :], arr[-1, :], arr[:, 0], arr[:, -1]], 0)
    bg = np.median(border, 0)
    dist = np.linalg.norm(arr - bg, axis=2)
    if dist.max() < 35:
        print(path.name, "EMPTY/LOW", "distmax", round(float(dist.max()), 1))
        return
    thr = max(45.0, float(np.percentile(dist, 70)))
    m = dist > thr
    if m.sum() < 80:
        m = dist > 35
    if m.sum() < 20:
        print(path.name, "EMPTY")
        return
    ys, xs = np.where(m)
    x0, x1 = int(xs.min()), int(xs.max())
    y0, y1 = int(ys.min()), int(ys.max())
    crop = m[y0 : y1 + 1, x0 : x1 + 1]
    h, w = crop.shape
    cols = 12
    rows = max(1, int(round(cols * h / w)))
    grid = np.zeros((rows, cols))
    for r in range(rows):
        for c in range(cols):
            ra = int(r * h / rows)
            rb = max(ra + 1, int((r + 1) * h / rows))
            ca = int(c * w / cols)
            cb = max(ca + 1, int((c + 1) * w / cols))
            grid[r, c] = crop[ra:rb, ca:cb].mean()
    note = ""
    if h <= 40 and w >= h * 1.6:
        line = (
            0.299 * arr[(y0 + y1) // 2, x0 : x1 + 1, 0]
            + 0.587 * arr[(y0 + y1) // 2, x0 : x1 + 1, 1]
            + 0.114 * arr[(y0 + y1) // 2, x0 : x1 + 1, 2]
        )
        best = lag_of(line)
        n = round(w / best) if best else "?"
        note = f" HORIZ~{n} lag={best}"
    if w <= 40 and h >= w * 1.6:
        line = (
            0.299 * arr[y0 : y1 + 1, (x0 + x1) // 2, 0]
            + 0.587 * arr[y0 : y1 + 1, (x0 + x1) // 2, 1]
            + 0.114 * arr[y0 : y1 + 1, (x0 + x1) // 2, 2]
        )
        best = lag_of(line)
        n = round(h / best) if best else "?"
        note = f" VERT~{n} lag={best}"
    print(f"== {path.name} {w}x{h}{note}")
    for r in range(rows):
        print("".join("#" if grid[r, c] > 0.45 else (":" if grid[r, c] > 0.2 else ".") for c in range(cols)))


def main() -> None:
    root = Path(__file__).resolve().parent / "_unique_slots_v2"
    for s in STEMS:
        for i in range(3):
            show(root / f"{s}_s{i}.png")
        print()


if __name__ == "__main__":
    main()
