"""
raw 카드 이미지에 template_frame_front.png 프레임을 합성하는 스크립트

- 입력:  output/raw/*.png
- 프레임: output/template_frame_front.png  (RGBA 투명 PNG 권장)
- 출력:  output/framed/*.png
"""
from pathlib import Path
from PIL import Image

ROOT = Path(__file__).parent.parent
RAW_DIR = ROOT / "output" / "raw"
FRAME_PATH = ROOT / "output" / "template_frame_front.png"
OUT_DIR = ROOT / "output" / "framed"


def apply_frame(card_path: Path, frame: Image.Image) -> Image.Image:
    card = Image.open(card_path).convert("RGBA")

    # 카드 크기를 프레임에 맞게 조정
    if card.size != frame.size:
        card = card.resize(frame.size, Image.LANCZOS)

    # 프레임을 카드 위에 합성
    composite = card.copy()
    composite.paste(frame, (0, 0), mask=frame)
    return composite


def main():
    if not FRAME_PATH.exists():
        raise FileNotFoundError(f"프레임 파일 없음: {FRAME_PATH}")

    raw_files = sorted(RAW_DIR.glob("*.png"))
    if not raw_files:
        raise FileNotFoundError(f"raw 이미지 없음: {RAW_DIR}")

    OUT_DIR.mkdir(parents=True, exist_ok=True)

    frame = Image.open(FRAME_PATH).convert("RGBA")

    print(f"{'='*55}")
    print(f"프레임 합성 시작 — {len(raw_files)}장")
    print(f"프레임 크기: {frame.size[0]}x{frame.size[1]}")
    print(f"출력 폴더:  {OUT_DIR.resolve()}")
    print(f"{'='*55}\n")

    for i, card_path in enumerate(raw_files, 1):
        out_path = OUT_DIR / card_path.name
        result = apply_frame(card_path, frame)
        result.save(out_path, format="PNG")
        print(f"[{i:>2}/{len(raw_files)}] {card_path.name} → {out_path.name}")

    print(f"\n완료! {len(raw_files)}장 저장됨: {OUT_DIR.resolve()}\n")


if __name__ == "__main__":
    main()
