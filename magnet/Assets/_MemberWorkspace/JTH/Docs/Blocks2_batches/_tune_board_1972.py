from pathlib import Path

import numpy as np
from PIL import Image

p = Path(r"C:\Users\hwanj\OneDrive\바탕 화면\Blocks\Unique\1972.jpg")
im = Image.open(p).convert("RGB")
im = im.resize((im.width * 2, im.height * 2), Image.Resampling.NEAREST)
a = np.array(im)
h, w = a.shape[:2]

expect = [
    "..#....#",
    "#..####.",
    "#######.",
    "#######.",
    "##......",
    "#####.#.",
    "#####.##",
    "..###...",
]


def eval_box(y0f, y1f, x0f, x1f):
    y0, y1 = int(h * y0f), int(h * y1f)
    x0, x1 = int(w * x0f), int(w * x1f)
    board = a[y0:y1, x0:x1]
    lum = board.mean(axis=2)
    bh, bw = lum.shape
    vals = []
    cells = []
    for r in range(8):
        for c in range(8):
            cy = int((r + 0.5) * bh / 8)
            cx = int((c + 0.5) * bw / 8)
            hy = max(1, int(bh / 8 * 0.15))
            hx = max(1, int(bw / 8 * 0.15))
            patch = lum[max(0, cy - hy) : cy + hy + 1, max(0, cx - hx) : cx + hx + 1]
            val = float(patch.mean()) if patch.size else 0.0
            vals.append(val)
            cells.append((r, c, val))
    vals_a = np.array(vals)
    best_t, best_s = 0.0, -1.0
    for t in np.linspace(vals_a.min() + 1, vals_a.max() - 1, 40):
        below = vals_a <= t
        w0 = below.mean()
        w1 = 1.0 - w0
        if w0 < 0.05 or w1 < 0.05:
            continue
        m0 = vals_a[below].mean()
        m1 = vals_a[~below].mean()
        s = w0 * w1 * (m0 - m1) ** 2
        if s > best_s:
            best_s = s
            best_t = float(t)
    occ = np.zeros((8, 8), dtype=bool)
    for r, c, v in cells:
        occ[r, c] = v > best_t
    got = ["".join("#" if occ[r, c] else "." for c in range(8)) for r in range(8)]
    match = sum(
        1
        for i in range(8)
        for j in range(8)
        if (got[i][j] == "#") == (expect[i][j] == "#")
    )
    return match, got, best_t


best = None
for y0f in [0.215, 0.22, 0.225]:
    for y1f in [0.66, 0.67, 0.675, 0.68, 0.685]:
        for x0f in [0.09, 0.10, 0.11]:
            for x1f in [0.89, 0.90, 0.91]:
                m, got, t = eval_box(y0f, y1f, x0f, x1f)
                if best is None or m > best[0]:
                    best = (m, y0f, y1f, x0f, x1f, got, t)

print("best", best[0], "/64", best[1:5], "t", round(best[6], 1))
for row in best[5]:
    print(row)
print("expect:")
for row in expect:
    print(row)
