"""Build labeled contact sheets from fictional native UI preview captures."""
import sys
from pathlib import Path
from PIL import Image, ImageDraw, ImageFont

root = Path(sys.argv[1])
font = ImageFont.truetype("C:/Windows/Fonts/segoeui.ttf", 16)
for theme in ("Light", "Dark"):
    files = sorted(root.glob(f"{theme}-*.png"))
    for page in range(0, len(files), 6):
        batch = files[page:page + 6]
        cell_w, cell_h = 460, 830
        canvas = Image.new("RGB", (cell_w * 3, cell_h * 2), "#E6E8EB")
        draw = ImageDraw.Draw(canvas)
        for i, path in enumerate(batch):
            x, y = (i % 3) * cell_w, (i // 3) * cell_h
            draw.text((x + 10, y + 8), path.stem, font=font, fill="#20242B")
            with Image.open(path) as im:
                im.thumbnail((cell_w - 20, cell_h - 40))
                canvas.paste(im.convert("RGB"), (x + 10, y + 34))
        canvas.save(root / f"sheet-{theme}-{page // 6 + 1}.png")
