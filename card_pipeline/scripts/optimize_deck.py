from pathlib import Path
from PIL import Image

ROOT = Path(__file__).parent.parent
DECK_IMG_PATH = ROOT / "output" / "bake_card_deck.png"
OUT_IMG_PATH = ROOT / "output" / "bake_card_deck_opt.png"

MAX_SIDE = 768

def _scaled_size(w: int, h: int) -> tuple[int, int]:
    if max(w, h) <= MAX_SIDE:
        return (w, h)
    scale = MAX_SIDE / max(w, h)
    return (round(w * scale), round(h * scale))

def main():
    if not DECK_IMG_PATH.exists():
        print(f"파일을 찾을 수 없습니다: {DECK_IMG_PATH}")
        return

    print("이미지 최적화 시작...")
    img = Image.open(DECK_IMG_PATH).convert("RGBA")
    
    # 1. 해상도 리사이즈
    target_size = _scaled_size(*img.size)
    if img.size != target_size:
        print(f"크기 변경: {img.size} -> {target_size}")
        img = img.resize(target_size, Image.LANCZOS)
    
    # 2. PNG 최적화 저장
    img.save(OUT_IMG_PATH, format="PNG", optimize=True)
    print(f"최적화 완료! 저장 위치: {OUT_IMG_PATH}")

if __name__ == "__main__":
    main()
