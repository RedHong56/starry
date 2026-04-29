import json
from pathlib import Path
from typing import Any

_cards_cache: dict[int, dict[str, Any]] | None = None

DATA_PATH = Path(__file__).parent.parent / "data" / "cards.json"


def load_cards() -> dict[int, dict[str, Any]]:
    global _cards_cache
    if _cards_cache is not None:
        return _cards_cache

    with DATA_PATH.open(encoding="utf-8") as f:
        raw = json.load(f)

    _cards_cache = {card["id"]: card for card in raw["cards"]}
    return _cards_cache


def get_card(card_id: int) -> dict[str, Any] | None:
    return load_cards().get(card_id)
