import json
from pathlib import Path
from typing import Any

_cache: dict[str, dict[int, dict[str, Any]]] = {}

_DATA_FILES = {
    "ko": Path(__file__).parent.parent / "data" / "cards.json",
    "en": Path(__file__).parent.parent / "data" / "cards_en.json",
}


def load_cards(lang: str = "ko") -> dict[int, dict[str, Any]]:
    key = lang if lang in _DATA_FILES else "ko"
    if key not in _cache:
        with _DATA_FILES[key].open(encoding="utf-8") as f:
            raw = json.load(f)
        _cache[key] = {card["id"]: card for card in raw["cards"]}
    return _cache[key]


def get_card(card_id: int, lang: str = "ko") -> dict[str, Any] | None:
    return load_cards(lang).get(card_id)
