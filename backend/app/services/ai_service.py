import json
import logging
from typing import Any

from fastapi import HTTPException
from openai import AsyncOpenAI

from app.config import settings
from app.models.reading import CardInput, CardInterpretation, ReadingResponse
from app.services.card_loader import get_card

logger = logging.getLogger(__name__)


def _mock_response(cards: list[CardInput]) -> ReadingResponse:
    stubs = [
        "새로운 시작의 에너지가 감돌고 있습니다. 두려움 없이 첫걸음을 내딛으세요.",
        "내면의 갈등을 나타냅니다. 먼저 자신의 마음을 들여다보세요.",
        "여정의 완성이 가까워졌습니다. 지금까지의 노력이 곧 빛을 발할 것입니다.",
    ]
    return ReadingResponse(
        reading=(
            "당신의 현재 상황은 변화와 성장의 시기에 놓여 있습니다. "
            "과거의 경험을 발판 삼아 새로운 출발을 준비하고, "
            "내면의 직관을 믿고 한 발씩 나아가세요."
        ),
        card_interpretations=[
            CardInterpretation(card_id=c.id, interpretation=stubs[i % len(stubs)])
            for i, c in enumerate(cards)
        ],
    )


def _build_prompt(concern: str, birth_date: str, cards: list[CardInput]) -> str:
    card_lines: list[str] = []
    for i, c in enumerate(cards, 1):
        card_data = get_card(c.id)
        if card_data is None:
            raise HTTPException(status_code=422, detail=f"Card id {c.id} not found in deck")

        direction = "역방향(Reversed)" if c.is_reversed else "정방향(Upright)"
        direction_key = "reversed" if c.is_reversed else "upright"
        keywords = ", ".join(card_data["keywords"][direction_key])
        meaning = card_data["meaning"][direction_key]

        card_lines.append(
            f"카드 {i}: {card_data['displayName']} ({direction})\n"
            f"  키워드: {keywords}\n"
            f"  의미: {meaning}"
        )

    return f"""당신은 한국어로 타로 점괘를 해석하는 전문 타로 리더입니다.
아래 정보를 바탕으로 깊이 있는 타로 점괘를 작성하세요.

[질문자 정보]
- 생년월일: {birth_date}
- 고민: {concern}

[뽑힌 카드]
{chr(10).join(card_lines)}

[응답 형식 - 반드시 유효한 JSON만 출력]
{{
  "reading": "세 카드를 종합한 전체 점괘 (한국어, 200~400자)",
  "card_interpretations": [
    {{"card_id": <id>, "interpretation": "이 카드가 질문자의 고민과 어떻게 연결되는지 해석 (50~100자)"}},
    {{"card_id": <id>, "interpretation": "..."}},
    {{"card_id": <id>, "interpretation": "..."}}
  ]
}}

주의사항:
- 반드시 JSON만 출력하고 마크다운 코드블록(```) 없이 출력하세요.
- card_id 값은 입력된 카드 id를 그대로 사용하세요: {[c.id for c in cards]}
- 모든 텍스트는 한국어로 작성하세요.
- 희망적이고 건설적인 방향으로 해석하세요."""


def _parse_response(text: str, cards: list[CardInput]) -> ReadingResponse:
    text = text.strip()
    if text.startswith("```"):
        text = text.split("\n", 1)[-1]
        if text.endswith("```"):
            text = text.rsplit("```", 1)[0]

    try:
        data: dict[str, Any] = json.loads(text)
        return ReadingResponse(
            reading=data["reading"],
            card_interpretations=[
                CardInterpretation(card_id=item["card_id"], interpretation=item["interpretation"])
                for item in data["card_interpretations"]
            ],
        )
    except (json.JSONDecodeError, KeyError, TypeError) as e:
        logger.warning("OpenAI 응답 파싱 실패, fallback 사용: %s", e)
        return ReadingResponse(
            reading=text if len(text) < 1000 else "점괘 해석을 준비 중입니다. 잠시 후 다시 시도해주세요.",
            card_interpretations=[
                CardInterpretation(card_id=c.id, interpretation="카드 해석을 준비 중입니다.")
                for c in cards
            ],
        )


async def generate_reading(concern: str, birth_date: str, cards: list[CardInput]) -> ReadingResponse:
    if settings.use_mock:
        return _mock_response(cards)

    if not settings.openai_api_key:
        raise HTTPException(status_code=503, detail="OPENAI_API_KEY가 설정되지 않았습니다.")

    client = AsyncOpenAI(api_key=settings.openai_api_key)
    prompt = _build_prompt(concern, birth_date, cards)

    try:
        completion = await client.chat.completions.create(
            model=settings.openai_model,
            messages=[{"role": "user", "content": prompt}],
            temperature=0.8,
            max_tokens=1024,
        )
        return _parse_response(completion.choices[0].message.content or "", cards)
    except Exception as e:
        logger.error("OpenAI API 호출 실패: %s", e)
        raise HTTPException(
            status_code=503,
            detail=f"OpenAI API 호출에 실패했습니다: {type(e).__name__}",
        ) from e
