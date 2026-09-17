import re
from pathlib import Path

from PIL import Image


SPRITE_ROOT = Path("Assets/_MemberWorkspace/JTH/Graphics/Sprites")
SHEETS = [
    (SPRITE_ROOT / "WaterBalloonBlocks.png", 4, 2),
    (SPRITE_ROOT / "StoneBlocks.png", 4, 2),
    (SPRITE_ROOT / "PaperBlocks.png", 4, 2),
    (SPRITE_ROOT / "SlimeBlocks.png", 4, 2),
    (SPRITE_ROOT / "WaterDropBlocks.png", 3, 1),
    (SPRITE_ROOT / "HoneycombBlocks.png", 3, 1),
    *[
        (SPRITE_ROOT / "Themes" / f"{name}Blocks.png", 4, 2)
        for name in "Ice Lava Cloud Hologram Galaxy Chocolate Candy Wood Fabric Ink".split()
    ],
]

SPRITE_RECT = re.compile(
    r"(?P<head>    - serializedVersion: 2\r?\n"
    r"      name: (?P<name>[^\r\n]+)\r?\n"
    r"      rect:\r?\n"
    r"        serializedVersion: 2\r?\n)"
    r"        x: [-0-9.]+\r?\n"
    r"        y: [-0-9.]+\r?\n"
    r"        width: [-0-9.]+\r?\n"
    r"        height: [-0-9.]+"
)


def trim_sheet(path: Path, columns: int, rows: int) -> None:
    image = Image.open(path).convert("RGBA")
    width, height = image.size
    cell_width, cell_height = width // columns, height // rows
    rects = []

    sprite_count = columns * rows
    for index in range(sprite_count):
        column, row = index % columns, index // columns
        left, top = column * cell_width, row * cell_height
        alpha = image.crop(
            (left, top, left + cell_width, top + cell_height)
        ).getchannel("A")
        bounds = alpha.getbbox()
        if bounds is None:
            raise ValueError(f"Empty sprite cell {index}: {path}")

        x0, y0, x1, y1 = bounds
        rects.append(
            (
                left + x0,
                height - (top + y1),
                x1 - x0,
                y1 - y0,
            )
        )

    meta_path = Path(str(path) + ".meta")
    source = meta_path.read_text(encoding="utf-8")
    updated = 0

    def replace(match: re.Match) -> str:
        nonlocal updated
        name = match.group("name")
        index_match = re.search(r"_(\d+)$", name)
        if index_match is None:
            return match.group(0)
        index = int(index_match.group(1))
        if index >= len(rects):
            return match.group(0)
        x, y, rect_width, rect_height = rects[index]
        updated += 1
        return (
            match.group("head")
            + f"        x: {x}\n"
            + f"        y: {y}\n"
            + f"        width: {rect_width}\n"
            + f"        height: {rect_height}"
        )

    result = SPRITE_RECT.sub(replace, source)
    if updated != sprite_count:
        raise ValueError(
            f"Expected {sprite_count} sprite rects, updated {updated}: {meta_path}"
        )
    meta_path.write_text(result, encoding="utf-8", newline="\n")
    print(f"{path}: {rects}")


for sheet, columns, rows in SHEETS:
    trim_sheet(sheet, columns, rows)
