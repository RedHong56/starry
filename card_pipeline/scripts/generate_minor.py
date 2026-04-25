"""
마이너 아르카나 56장 자동 생성 스크립트
ComfyUI 로컬 서버 사용 (Flux Schnell GGUF)

실행 전 ComfyUI가 http://127.0.0.1:8188 에서 실행 중이어야 합니다.
"""
import json
import random
import time
import requests
from pathlib import Path

COMFYUI_URL = "http://127.0.0.1:8188"
ROOT = Path(__file__).parent.parent

STYLE_BASE = (
    "vintage art nouveau illustration style, "
    "alphonse mucha inspired, "
    "muted burgundy purple gold antique color palette, "
    "aged parchment paper texture, "
    "hand-drawn etching technique with elegant linework, "
    "mystical warm atmosphere, candlelit feel, "
    "vertical portrait composition, centered figure, "
    "no border, no frame, no text, no letters, no numbers, no banner"
)

OUTPUT_DIR = ROOT / "output" / "raw"
OUTPUT_DIR.mkdir(parents=True, exist_ok=True)

DATA_FILE = ROOT / "data" / "minor_arcana.json"
FAILED_FILE = ROOT / "data" / "failed_cards.json"
WORKFLOW_FILE = ROOT / "data" / "workflow_flux_schnell.json"


def check_comfyui():
    try:
        requests.get(f"{COMFYUI_URL}/system_stats", timeout=3)
        return True
    except requests.exceptions.ConnectionError:
        return False


def load_workflow() -> dict:
    with open(WORKFLOW_FILE, "r", encoding="utf-8") as f:
        return json.load(f)


def queue_prompt(workflow: dict) -> str:
    payload = {"prompt": workflow}
    response = requests.post(f"{COMFYUI_URL}/prompt", json=payload, timeout=10)
    response.raise_for_status()
    return response.json()["prompt_id"]


def wait_for_completion(prompt_id: str, timeout: int = 300) -> dict:
    start = time.time()
    while time.time() - start < timeout:
        time.sleep(2)
        history = requests.get(f"{COMFYUI_URL}/history/{prompt_id}", timeout=10).json()
        if prompt_id in history:
            return history[prompt_id]
    raise TimeoutError(f"생성 시간 초과 ({timeout}초)")


def download_image(img_info: dict) -> bytes:
    params = {
        "filename": img_info["filename"],
        "subfolder": img_info.get("subfolder", ""),
        "type": img_info.get("type", "output"),
    }
    response = requests.get(f"{COMFYUI_URL}/view", params=params, timeout=30)
    response.raise_for_status()
    return response.content


def generate_card(card: dict, retry_count: int = 0, max_retries: int = 2) -> bool:
    full_prompt = f"art nouveau illustration of {card['description']}, {STYLE_BASE}"

    print(f"  [{card['id']:02d}] {card['name']} 생성 중...", end=" ", flush=True)

    try:
        workflow = load_workflow()
        workflow["4"]["inputs"]["text"] = full_prompt
        workflow["8"]["inputs"]["seed"] = random.randint(0, 2**32 - 1)

        prompt_id = queue_prompt(workflow)
        result = wait_for_completion(prompt_id)

        # 출력 이미지 찾기
        for node_output in result["outputs"].values():
            if "images" in node_output:
                img_data = download_image(node_output["images"][0])
                filename = f"{card['id']:02d}_{card['filename']}.png"
                (OUTPUT_DIR / filename).write_bytes(img_data)
                print("완료")
                return True

        raise RuntimeError("출력 이미지 없음")

    except Exception as e:
        print(f"실패: {str(e)[:60]}")
        if retry_count < max_retries:
            print(f"     재시도 ({retry_count + 1}/{max_retries})...")
            time.sleep(3)
            return generate_card(card, retry_count + 1, max_retries)
        return False


def main():
    if not check_comfyui():
        raise SystemExit(
            "ComfyUI가 실행되지 않았습니다.\n"
            "ComfyUI 폴더에서 'python main.py' 를 먼저 실행하세요."
        )

    src = FAILED_FILE if FAILED_FILE.exists() else DATA_FILE
    if src == FAILED_FILE:
        print("failed_cards.json 발견 — 실패 카드만 재생성합니다.\n")

    with open(src, "r", encoding="utf-8") as f:
        cards = json.load(f)

    total = len(cards)
    print(f"{'='*60}")
    print(f"마이너 아르카나 {total}장 생성 시작")
    print(f"출력 폴더: {OUTPUT_DIR.resolve()}")
    print(f"{'='*60}\n")

    success_count = 0
    failed_cards = []

    for i, card in enumerate(cards, 1):
        print(f"[{i}/{total}]", end=" ")

        filename = f"{card['id']:02d}_{card['filename']}.png"
        if (OUTPUT_DIR / filename).exists():
            print(f"  [{card['id']:02d}] {card['name']} — 이미 존재, 건너뜀")
            success_count += 1
            continue

        if generate_card(card):
            success_count += 1
        else:
            failed_cards.append(card)

    print(f"\n{'='*60}")
    print(f"완료!")
    print(f"  성공: {success_count}/{total}")

    if failed_cards:
        print(f"  실패: {len(failed_cards)}장")
        for card in failed_cards:
            print(f"    - [{card['id']:02d}] {card['name']}")
        FAILED_FILE.write_text(
            json.dumps(failed_cards, ensure_ascii=False, indent=2), encoding="utf-8"
        )
        print(f"\n  실패 목록 저장됨: {FAILED_FILE}")
        print(f"  재실행하면 실패한 카드만 다시 시도합니다.")
    else:
        if FAILED_FILE.exists():
            FAILED_FILE.unlink()
        print(f"  모든 카드 생성 완료!")

    print(f"{'='*60}\n")


if __name__ == "__main__":
    main()
