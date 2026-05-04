import logging
from datetime import date

from fastapi import HTTPException
from openai import AsyncOpenAI

from app.config import settings

logger = logging.getLogger(__name__)

_MOCK: dict[str, str] = {
    "Aries":       "오늘은 새로운 도전을 시작하기 좋은 날입니다. 용기를 내어 첫걸음을 내딛어 보세요.",
    "Taurus":      "안정과 편안함을 추구하는 오늘, 소소한 기쁨 속에서 큰 행복을 발견하게 됩니다.",
    "Gemini":      "다양한 아이디어가 넘치는 하루입니다. 주변 사람들과의 소통이 행운을 불러올 것입니다.",
    "Cancer":      "감수성이 풍부해지는 날입니다. 사랑하는 사람과 시간을 보내면 마음이 충전됩니다.",
    "Leo":         "빛나는 하루가 기다리고 있습니다. 자신감을 갖고 나아가면 원하는 것을 이룰 수 있습니다.",
    "Virgo":       "세심한 주의가 빛을 발하는 날입니다. 꼼꼼하게 준비한 일들이 좋은 결과를 맺을 것입니다.",
    "Libra":       "균형 잡힌 하루를 보내세요. 조화로운 관계가 당신에게 긍정적인 에너지를 줄 것입니다.",
    "Scorpio":     "직관이 날카로운 날입니다. 깊이 있는 통찰로 복잡한 문제를 해결하는 실마리를 찾게 됩니다.",
    "Sagittarius": "모험심이 자극받는 하루입니다. 새로운 경험에 도전하면 뜻밖의 즐거움을 만날 수 있습니다.",
    "Capricorn":   "꾸준한 노력이 결실을 맺는 날입니다. 목표를 향해 한 발씩 나아가면 반드시 성공합니다.",
    "Aquarius":    "독창적인 아이디어로 주목받는 날입니다. 자신만의 방식으로 세상에 긍정적인 변화를 줄 수 있습니다.",
    "Pisces":      "풍부한 상상력이 빛을 발하는 하루입니다. 창의적인 활동에 집중하면 놀라운 결과가 나타납니다.",
}

_cache: dict[tuple[str, str], str] = {}


async def get_horoscope(constellation: str) -> str:
    today = date.today().isoformat()
    cache_key = (constellation.lower(), today)

    if cache_key in _cache:
        return _cache[cache_key]

    if settings.use_mock:
        result = _MOCK.get(constellation, f"오늘 {constellation}자리에는 별들의 축복이 함께합니다.")
        _cache[cache_key] = result
        return result

    if not settings.openai_api_key:
        raise HTTPException(status_code=503, detail="OPENAI_API_KEY가 설정되지 않았습니다.")

    client = AsyncOpenAI(api_key=settings.openai_api_key)
    prompt = (
        f"당신은 한국어로 별자리 운세를 해석하는 전문 점성술사입니다.\n"
        f"오늘({today}) {constellation}자리의 운세를 한국어로 100~200자 내외로 작성하세요.\n"
        f"희망적이고 건설적인 방향으로 해석하고, 순수한 텍스트만 출력하세요."
    )

    try:
        completion = await client.chat.completions.create(
            model=settings.openai_model,
            messages=[{"role": "user", "content": prompt}],
            temperature=0.7,
            max_tokens=300,
        )
        result = completion.choices[0].message.content or "오늘의 운세를 준비 중입니다."
        _cache[cache_key] = result
        return result
    except Exception as e:
        logger.error("운세 AI 호출 실패: %s", e)
        raise HTTPException(status_code=503, detail=f"운세 호출 실패: {type(e).__name__}") from e
