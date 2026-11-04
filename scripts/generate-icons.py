"""從同一個圖形產生 MSIX 所需的各尺寸圖示。

圖形取自「兩個重疊視窗」的意象，呼應 Windows 遊戲設定中的 Game Bar 圖示；
不複製系統圖示。執行前安裝 Pillow：python -m pip install pillow。
發行包直接使用已提交的 PNG，終端使用者不需要 Python。
"""

from pathlib import Path

from PIL import Image, ImageDraw


ASSETS = Path(__file__).resolve().parents[1] / "cmdpal-extension" / "MsGamebarHandlerExtension" / "Assets"
INK = "#14243B"
BLUE = "#4CC9F0"
WHITE = "#F5FAFF"


def mark(size: int) -> Image.Image:
    """先放大繪製，再縮小，讓 24 px 小圖示的圓角和斜線保持清晰。"""
    scale = 4
    canvas = Image.new("RGBA", (size * scale, size * scale), (0, 0, 0, 0))
    draw = ImageDraw.Draw(canvas)

    def box(coords: tuple[int, int, int, int]) -> tuple[int, int, int, int]:
        return tuple(round(value * size * scale / 100) for value in coords)

    def width(value: int) -> int:
        return max(1, round(value * size * scale / 100))

    draw.rounded_rectangle(box((4, 4, 96, 96)), radius=width(22), fill=INK)
    # 後窗與前窗的疊放，對應參考圖中的 Game Bar 視窗輪廓。
    draw.rounded_rectangle(box((21, 28, 65, 67)), radius=width(7), outline=BLUE, width=width(6))
    draw.rounded_rectangle(box((38, 39, 80, 76)), radius=width(7), outline=WHITE, width=width(6))
    # 留一個亮色端點，縮到 Command Palette 的小尺寸時仍可辨識方向。
    draw.ellipse(box((67, 49, 73, 55)), fill=BLUE)
    return canvas.resize((size, size), Image.Resampling.LANCZOS)


def tile(width: int, height: int, symbol: int) -> Image.Image:
    canvas = Image.new("RGBA", (width, height), INK)
    icon = mark(symbol)
    canvas.alpha_composite(icon, ((width - symbol) // 2, (height - symbol) // 2))
    return canvas


def main() -> None:
    ASSETS.mkdir(parents=True, exist_ok=True)
    sizes = {
        "StoreLogo.png": 50,
        "Square44x44Logo.targetsize-24_altform-unplated.png": 24,
        "Square44x44Logo.scale-200.png": 88,
        "Square150x150Logo.scale-200.png": 300,
        "LockScreenLogo.scale-200.png": 48,
    }
    for name, size in sizes.items():
        mark(size).save(ASSETS / name, optimize=True)
    tile(620, 300, 190).save(ASSETS / "Wide310x150Logo.scale-200.png", optimize=True)
    tile(1240, 600, 300).save(ASSETS / "SplashScreen.scale-200.png", optimize=True)


if __name__ == "__main__":
    main()
