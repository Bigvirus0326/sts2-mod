#!/usr/bin/env python3
"""
모드 폴더의 art/ 원본 그림(GPT 등이 만든 것)을 게임이 쓰는 크기·위치로 변환합니다.
(아래 경로는 mods/<모드이름>/ 기준, SpireMod 자리에 모드 이름이 들어감)

  art/mod_image.png        -> SpireMod/mod_image.png                      (420x420)
  art/cards/<id>.png       -> SpireMod/images/card_portraits/big/<id>.png (1000x760)
                              SpireMod/images/card_portraits/<id>.png     (250x190)
  art/relics/<id>.png      -> SpireMod/images/relics/big/<id>.png         (256x256)
                              SpireMod/images/relics/<id>.png             (94x94)
                              SpireMod/images/relics/<id>_outline.png     (94x94, 자동 생성 흰 실루엣)
  art/powers/<id>.png      -> SpireMod/images/powers/big/<id>.png         (256x256)
                              SpireMod/images/powers/<id>.png             (64x64)
  art/potions/<id>.png     -> SpireMod/images/potions/<id>.png            (원본 비율 유지, 최대 256)
  art/other/**             -> SpireMod/images/other/** (그대로 복사)

원본은 어떤 크기·비율이든 됩니다. 카드 그림은 가운데 기준으로 잘라 맞추고,
유물/파워/포션은 투명 여백을 정리한 뒤 정사각형 안에 맞춥니다.
파일 이름은 소문자 영어·숫자·밑줄(snake_case)로 정규화됩니다.

사용:  python3 tools/process_art.py mods/SpireMod          (변환)
       python3 tools/process_art.py mods/SpireMod --check  (규칙 위반만 검사, CI용)
"""
from __future__ import annotations

import re
import shutil
import sys
from pathlib import Path

try:
    from PIL import Image, ImageFilter, ImageOps
except ImportError:  # pragma: no cover
    sys.exit("Pillow가 필요합니다:  pip install pillow")

ROOT = ART = MOD = IMG = Path()  # main()에서 설정
EXTS = {".png", ".jpg", ".jpeg", ".webp"}


def slug(name: str) -> str:
    s = re.sub(r"([a-z0-9])([A-Z])", r"\1_\2", name)
    s = re.sub(r"[^0-9a-zA-Z]+", "_", s).strip("_").lower()
    return s or "unnamed"


def load(path: Path) -> Image.Image:
    im = Image.open(path)
    im = ImageOps.exif_transpose(im)
    return im.convert("RGBA")


def cover(im: Image.Image, size: tuple[int, int]) -> Image.Image:
    """비율 유지하며 꽉 채운 뒤 가운데를 잘라냄 (카드 그림용)."""
    return ImageOps.fit(im, size, Image.LANCZOS, centering=(0.5, 0.45))


def contain_square(im: Image.Image, side: int, pad: float = 0.06) -> Image.Image:
    """투명 여백을 잘라내고 정사각형 캔버스 가운데에 맞춤 (아이콘용)."""
    bbox = im.getchannel("A").getbbox()
    if bbox:
        im = im.crop(bbox)
    inner = max(1, round(side * (1 - 2 * pad)))
    im = ImageOps.contain(im, (inner, inner), Image.LANCZOS)
    canvas = Image.new("RGBA", (side, side), (0, 0, 0, 0))
    canvas.alpha_composite(im, ((side - im.width) // 2, (side - im.height) // 2))
    return canvas


def outline(icon: Image.Image, grow: int = 2) -> Image.Image:
    """아이콘 모양을 살짝 키운 흰색 실루엣 (유물 테두리 이미지)."""
    a = icon.getchannel("A").point(lambda v: 255 if v > 24 else 0)
    a = a.filter(ImageFilter.MaxFilter(grow * 2 + 1))
    white = Image.new("RGBA", icon.size, (255, 255, 255, 0))
    white.putalpha(a)
    return white


def save(im: Image.Image, path: Path) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    im.save(path, optimize=True)


def sources(folder: Path):
    if not folder.is_dir():
        return []
    return sorted(p for p in folder.iterdir() if p.suffix.lower() in EXTS and not p.name.startswith("."))


def main(mod_dir: Path, check_only: bool) -> int:
    global ROOT, ART, MOD, IMG
    ROOT = mod_dir.resolve()
    ART = ROOT / "art"
    MOD = ROOT / ROOT.name
    IMG = MOD / "images"
    if not (ROOT / f"{ROOT.name}.csproj").exists():
        sys.exit(f"{ROOT} 는 모드 폴더가 아닙니다 ({ROOT.name}.csproj 없음)")
    problems: list[str] = []
    made = 0

    # 이름 충돌 검사
    for sub in ("cards", "relics", "powers", "potions"):
        seen: dict[str, Path] = {}
        for p in sources(ART / sub):
            key = slug(p.stem)
            if key in seen:
                problems.append(f"{ROOT.name}/art/{sub}: '{p.name}' 와 '{seen[key].name}' 가 같은 이름({key})이 됩니다.")
            seen[key] = p

    if check_only:
        for msg in problems:
            print("문제:", msg)
        return 1 if problems else 0

    if IMG.exists():
        shutil.rmtree(IMG)

    mi = ART / "mod_image.png"
    if mi.exists():
        save(contain_square(load(mi), 420, pad=0), MOD / "mod_image.png")
        made += 1

    for p in sources(ART / "cards"):
        im, n = load(p), slug(p.stem)
        save(cover(im, (1000, 760)), IMG / "card_portraits/big" / f"{n}.png")
        save(cover(im, (250, 190)), IMG / "card_portraits" / f"{n}.png")
        made += 1

    for p in sources(ART / "relics"):
        im, n = load(p), slug(p.stem)
        small = contain_square(im, 94)
        save(contain_square(im, 256), IMG / "relics/big" / f"{n}.png")
        save(small, IMG / "relics" / f"{n}.png")
        save(outline(small), IMG / "relics" / f"{n}_outline.png")
        made += 1

    for p in sources(ART / "powers"):
        im, n = load(p), slug(p.stem)
        save(contain_square(im, 256), IMG / "powers/big" / f"{n}.png")
        save(contain_square(im, 64, pad=0.03), IMG / "powers" / f"{n}.png")
        made += 1

    for p in sources(ART / "potions"):
        im, n = load(p), slug(p.stem)
        bbox = im.getchannel("A").getbbox()
        if bbox:
            im = im.crop(bbox)
        save(ImageOps.contain(im, (256, 256), Image.LANCZOS), IMG / "potions" / f"{n}.png")
        made += 1

    other = ART / "other"
    if other.is_dir():
        for p in other.rglob("*"):
            if p.is_file() and not p.name.startswith("."):
                dst = IMG / "other" / p.relative_to(other)
                dst.parent.mkdir(parents=True, exist_ok=True)
                shutil.copy2(p, dst)
                made += 1

    for msg in problems:
        print("경고:", msg)
    print(f"[{ROOT.name}] 그림 {made}개 변환 완료")
    return 0


if __name__ == "__main__":
    args = [a for a in sys.argv[1:] if not a.startswith("--")]
    if len(args) != 1:
        sys.exit("사용법: python3 tools/process_art.py mods/<모드이름> [--check]")
    sys.exit(main(Path(args[0]), "--check" in sys.argv))
