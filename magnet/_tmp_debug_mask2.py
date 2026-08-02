import numpy as np
from PIL import Image

p = r"C:\Users\hwanj\OneDrive\바탕 화면\Blocks\Blocks2\Screenshot_20260802_221440_Block Blast!.jpg"
img = np.array(Image.open(p).convert("RGB"))
hand = img[1760:1870]
slot = hand[:, :360]
r, g, b = slot[:, :, 0].astype(float), slot[:, :, 1].astype(float), slot[:, :, 2].astype(float)
pink = (r > 150) & (r > g + 20) & (g > 80) & (b < 220)

# column profile for rows 5-55 (piece area)
sub = pink[5:55, :]
cc = sub.sum(axis=0)
for i in range(len(cc)):
    if cc[i] > 20:
        print(f"col {i}: {cc[i]}")

print("--- yellow mask on slot1 ---")
slot1 = hand[:, 360:720]
r, g, b = slot1[:, :, 0].astype(float), slot1[:, :, 1].astype(float), slot1[:, :, 2].astype(float)
yellow = (r > 180) & (g > 160) & ((r + g) / 2 > b + 30)
cc = yellow.sum(axis=0)
active = np.where(cc > 50)[0]
print("active cols", active[0], active[-1], "width", active[-1]-active[0])
rc = yellow.sum(axis=1)
active_r = np.where(rc > 50)[0]
print("active rows", active_r[0], active_r[-1], "height", active_r[-1]-active_r[0])
