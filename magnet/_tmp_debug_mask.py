import numpy as np
from PIL import Image

p = r"C:\Users\hwanj\OneDrive\바탕 화면\Blocks\Blocks2\Screenshot_20260802_221440_Block Blast!.jpg"
img = np.array(Image.open(p).convert("RGB"))
hand = img[1760:1870]
slot = hand[:, :360]
r, g, b = slot[:, :, 0].astype(float), slot[:, :, 1].astype(float), slot[:, :, 2].astype(float)
pink = (r > 150) & (r > g + 20) & (g > 80) & (b < 220)
rc = pink.sum(axis=1)
for i in range(len(rc)):
    if rc[i] > 100:
        bar = "X" * int(rc[i] / 200)
        print(f"row {i}: {rc[i]} {bar}")
