from pathlib import Path

from PIL import Image


ROOT = Path("Assets/_MemberWorkspace/JTH/Graphics/Sprites/Themes")
THEMES = "Ice Lava Cloud Hologram Galaxy Chocolate Candy Wood Fabric Ink".split()
CELL = 512
TARGET = 420


def normalize_cell(cell: Image.Image) -> Image.Image:
    alpha = cell.getchannel("A")
    bounds = alpha.getbbox()
    if bounds is None:
        return cell

    tile = cell.crop(bounds)
    tile = tile.resize((TARGET, TARGET), Image.Resampling.LANCZOS)
    output = Image.new("RGBA", (CELL, CELL), (0, 0, 0, 0))
    offset = ((CELL - TARGET) // 2, (CELL - TARGET) // 2)
    output.alpha_composite(tile, offset)
    return output


for theme in THEMES:
    path = ROOT / f"{theme}Blocks.png"
    sheet = Image.open(path).convert("RGBA")
    if sheet.size != (CELL * 4, CELL * 2):
        raise ValueError(f"Unexpected sheet size: {path} {sheet.size}")

    normalized = Image.new("RGBA", sheet.size, (0, 0, 0, 0))
    for row in range(2):
        for column in range(4):
            box = (
                column * CELL,
                row * CELL,
                (column + 1) * CELL,
                (row + 1) * CELL,
            )
            normalized.alpha_composite(
                normalize_cell(sheet.crop(box)),
                (column * CELL, row * CELL),
            )

    normalized.save(path, optimize=True)
    print(path)
