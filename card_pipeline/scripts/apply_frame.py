"""
raw 카드 이미지에 template_frame_front.png 프레임을 합성하는 스크립트

- 입력:  output/raw/*.png
- 프레임: output/template_frame_front.png  (RGBA, raw보다 큰 사이즈)
- 출력:  output/framed/*.png  (긴 변 768 이하, 비율 유지, optimize=True)
"""
from pathlib import Path
from PIL import Image

ROOT = Path(__file__).parent.parent
RAW_DIR = ROOT / "output" / "raw"
FRAME_PATH = ROOT / "output" / "template_frame_front.png"
OUT_DIR = ROOT / "output" / "framed"

MAX_SIDE = 768     # 긴 변 기준 다운스케일 (비율 유지)


def _scaled_size(w: int, h: int) -> tuple[int, int]:
    if max(w, h) <= MAX_SIDE:
        return (w, h)
    scale = MAX_SIDE / max(w, h)
    return (round(w * scale), round(h * scale))


def apply_frame(card_path: Path, frame: Image.Image) -> Image.Image:
    card = Image.open(card_path).convert("RGBA")

    # 프레임 크기 캔버스에 카드를 중앙 배치
    canvas = Image.new("RGBA", frame.size, (0, 0, 0, 0))
    x = (frame.size[0] - card.size[0]) // 2
    y = (frame.size[1] - card.size[1]) // 2
    canvas.paste(card, (x, y))

    # 프레임을 위에 합성
    canvas.paste(frame, (0, 0), mask=frame)

    # 비율 유지 다운스케일
    target = _scaled_size(*canvas.size)
    if canvas.size != target:
        canvas = canvas.resize(target, Image.LANCZOS)

    return canvas


def main():
    if not FRAME_PATH.exists():
        raise FileNotFoundError(f"프레임 파일 없음: {FRAME_PATH}")

    raw_files = sorted(RAW_DIR.glob("*.png"))
    if not raw_files:
        raise FileNotFoundError(f"raw 이미지 없음: {RAW_DIR}")

    OUT_DIR.mkdir(parents=True, exist_ok=True)

    frame = Image.open(FRAME_PATH).convert("RGBA")
    out_w, out_h = _scaled_size(*frame.size)

    print(f"{'='*55}")
    print(f"프레임 합성 시작 - {len(raw_files)}장")
    print(f"프레임 크기: {frame.size[0]}x{frame.size[1]}  →  출력: {out_w}x{out_h}")
    print(f"포맷: PNG  (optimize=True)")
    print(f"출력 폴더:  {OUT_DIR.resolve()}")
    print(f"{'='*55}\n")

    for i, card_path in enumerate(raw_files, 1):
        out_path = OUT_DIR / (card_path.stem + ".png")
        result = apply_frame(card_path, frame)
        result.save(out_path, format="PNG", optimize=True)
        print(f"[{i:>2}/{len(raw_files)}] {card_path.name} → {out_path.name}")

    print(f"\n완료! {len(raw_files)}장 저장됨: {OUT_DIR.resolve()}\n")


if __name__ == "__main__":
    main()

