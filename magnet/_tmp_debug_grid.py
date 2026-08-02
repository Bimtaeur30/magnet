import sys
import numpy as np
from PIL import Image

sys.path.insert(0, r"D:\GameDev_C\Projects\Unity\2026-Supercent-Magnet\magnet")
from _tmp_extract_batch7_hands import (
    block_color_mask, find_hand_band, tight_bbox, find_peaks, mask_to_grid, match_id,
)

p = r"C:\Users\hwanj\OneDrive\바탕 화면\Blocks\Blocks2\Screenshot_20260802_221440_Block Blast!.jpg"
img = np.array(Image.open(p).convert("RGB"))
h, w = img.shape[:2]
y0, y1 = find_hand_band(img)
print("band", y0, y1, y1 - y0)
hand = img[y0:y1]
mask = block_color_mask(hand)
slot_w = w // 3
for slot in range(3):
    sm = mask[:, slot * slot_w : (slot + 1) * slot_w]
    bb = tight_bbox(sm)
    print(f"slot{slot} bbox", bb, "px", sm.sum())
    if bb:
        y0b, y1b, x0b, x1b = bb
        crop = sm[y0b : y1b + 1, x0b : x1b + 1]
        rp = find_peaks(crop.sum(axis=1))
        cp = find_peaks(crop.sum(axis=0))
        print(f"  peaks rows={len(rp)} cols={len(cp)}")
    grid = mask_to_grid(sm)
    print(f"  grid={None if grid is None else grid.shape} id={match_id(grid) if grid is not None else None}")
    if grid is not None:
        for row in grid.astype(int):
            print("   ", "".join("#" if c else "." for c in row))
