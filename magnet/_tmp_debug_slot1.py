import sys
import numpy as np
from PIL import Image

sys.path.insert(0, r"D:\GameDev_C\Projects\Unity\2026-Supercent-Magnet\magnet")
from _tmp_extract_batch7_hands import (
    block_color_mask, find_hand_band, tight_bbox, sample_grid, match_id, CELL_COUNTS,
)

p = r"C:\Users\hwanj\OneDrive\바탕 화면\Blocks\Blocks2\Screenshot_20260802_221440_Block Blast!.jpg"
img = np.array(Image.open(p).convert("RGB"))
y0, y1 = find_hand_band(img)
mask = block_color_mask(img[y0:y1])
slot = mask[:, 360:720]
bb = tight_bbox(slot)
print("band", y1 - y0, "bbox", bb)
y0b, y1b, x0b, x1b = bb
crop = slot[y0b : y1b + 1, x0b : x1b + 1]
print("crop", crop.shape, "fill", crop.mean())
Image.fromarray((crop * 255).astype(np.uint8)).save(
    r"D:\GameDev_C\Projects\Unity\2026-Supercent-Magnet\magnet\_tmp_crop1.png"
)

matches = []
for nrows in range(1, 6):
    for ncols in range(1, 6):
        grid = sample_grid(crop, nrows, ncols)
        filled = int(grid.sum())
        pid = match_id(grid)
        if pid and filled == CELL_COUNTS[pid]:
            matches.append((filled, nrows, ncols, pid, grid))
matches.sort(key=lambda x: (-x[0], -x[1] * x[2]))
for m in matches[:8]:
    print("cells", m[0], "shape", m[1], m[2], "id", m[3])
