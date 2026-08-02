import numpy as np
from PIL import Image

p = r"C:\Users\hwanj\OneDrive\바탕 화면\Blocks\Blocks2\Screenshot_20260802_221440_Block Blast!.jpg"
img = np.array(Image.open(p).convert("RGB"))
hand = img[1760:1870]

def yellow_mask(rgb):
    r, g, b = rgb[:, :, 0].astype(float), rgb[:, :, 1].astype(float), rgb[:, :, 2].astype(float)
    return (r > 180) & (g > 160) & ((r + g) / 2 > b + 30)

def pink_mask(rgb):
    r, g, b = rgb[:, :, 0].astype(float), rgb[:, :, 1].astype(float), rgb[:, :, 2].astype(float)
    return (r > 165) & (r > g + 35) & (g > 100) & (g < 200) & (b < 210)

def block_mask(rgb):
    return yellow_mask(rgb) | pink_mask(rgb)

slot_w = 360
for slot in range(3):
    s = hand[:, slot * slot_w : (slot + 1) * slot_w]
    m = block_mask(s)
    Image.fromarray((m * 255).astype(np.uint8)).save(
        rf"D:\GameDev_C\Projects\Unity\2026-Supercent-Magnet\magnet\_tmp_bm_slot{slot}.png"
    )
