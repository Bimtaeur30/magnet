from pathlib import Path
import sys

import numpy as np
from PIL import Image, ImageDraw, ImageFilter


def strongest_edge(profile, start, end, reverse=False):
    segment = profile[start:end]
    if reverse:
        segment = segment[::-1]
    index = int(np.argmax(segment))
    return end - 1 - index if reverse else start + index


def normalize(path: Path):
    source = Image.open(path).convert("RGBA")
    width, height = source.size
    cell_w, cell_h = width // 4, height // 2
    output = Image.new("RGBA", (2048, 1024), (0, 0, 0, 0))

    for row in range(2):
        for col in range(4):
            cell = source.crop((col * cell_w, row * cell_h, (col + 1) * cell_w, (row + 1) * cell_h))
            rgb = np.asarray(cell.convert("RGB"), dtype=np.float32)
            luminance = rgb.mean(axis=2)
            gx = np.abs(np.diff(luminance, axis=1)).mean(axis=0)
            gy = np.abs(np.diff(luminance, axis=0)).mean(axis=1)

            left = strongest_edge(gx, int(cell_w * .03), int(cell_w * .30))
            right = strongest_edge(gx, int(cell_w * .70), int(cell_w * .97), reverse=True)
            top = strongest_edge(gy, int(cell_h * .05), int(cell_h * .38))
            bottom = strongest_edge(gy, int(cell_h * .62), int(cell_h * .95), reverse=True)

            # Keep a small safety inset so no generated backdrop survives around the tile.
            left += 2
            top += 2
            right -= 1
            bottom -= 1
            crop = cell.crop((left, top, right + 1, bottom + 1))
            cw, ch = crop.size

            mask = Image.new("L", (cw, ch), 0)
            radius = max(8, int(min(cw, ch) * .075))
            ImageDraw.Draw(mask).rounded_rectangle((1, 1, cw - 2, ch - 2), radius=radius, fill=255)
            mask = mask.filter(ImageFilter.GaussianBlur(1.1))
            crop.putalpha(mask)

            scale = min(460 / cw, 460 / ch)
            resized = crop.resize((max(1, round(cw * scale)), max(1, round(ch * scale))), Image.Resampling.LANCZOS)
            x = col * 512 + (512 - resized.width) // 2
            y = row * 512 + (512 - resized.height) // 2
            output.alpha_composite(resized, (x, y))

    output.save(path)
    print(f"normalized {path}: {source.size} -> {output.size}")


if __name__ == "__main__":
    for argument in sys.argv[1:]:
        normalize(Path(argument))
