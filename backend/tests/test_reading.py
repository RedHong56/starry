import pytest
from fastapi.testclient import TestClient

import app.config as cfg

cfg.settings.use_mock = True

from app.main import app

client = TestClient(app)


def test_health():
    response = client.get("/health")
    assert response.status_code == 200
    assert response.json() == {"status": "ok"}


def test_reading_mock():
    payload = {
        "concern": "직장 생활이 힘들어요",
        "birth_date": "1995-03-15",
        "cards": [
            {"id": 0, "is_reversed": False},
            {"id": 21, "is_reversed": False},
            {"id": 56, "is_reversed": True},
        ],
    }
    response = client.post("/reading", json=payload)
    assert response.status_code == 200

    data = response.json()
    assert "reading" in data
    assert len(data["card_interpretations"]) == 3
    assert [ci["card_id"] for ci in data["card_interpretations"]] == [0, 21, 56]


def test_reading_invalid_card_count():
    payload = {
        "concern": "테스트",
        "birth_date": "1990-01-01",
        "cards": [{"id": 0, "is_reversed": False}],
    }
    assert client.post("/reading", json=payload).status_code == 422


def test_reading_invalid_card_id():
    payload = {
        "concern": "테스트",
        "birth_date": "1990-01-01",
        "cards": [
            {"id": 0, "is_reversed": False},
            {"id": 78, "is_reversed": False},
            {"id": 1, "is_reversed": False},
        ],
    }
    assert client.post("/reading", json=payload).status_code == 422


def test_reading_invalid_date_format():
    payload = {
        "concern": "테스트",
        "birth_date": "19950315",
        "cards": [
            {"id": 0, "is_reversed": False},
            {"id": 1, "is_reversed": False},
            {"id": 2, "is_reversed": False},
        ],
    }
    assert client.post("/reading", json=payload).status_code == 422


def test_reading_no_api_key_returns_503():
    cfg.settings.use_mock = False
    cfg.settings.openai_api_key = ""

    response = client.post(
        "/reading",
        json={
            "concern": "테스트",
            "birth_date": "1990-01-01",
            "cards": [
                {"id": 0, "is_reversed": False},
                {"id": 1, "is_reversed": False},
                {"id": 2, "is_reversed": False},
            ],
        },
    )
    assert response.status_code == 503

    cfg.settings.use_mock = True
