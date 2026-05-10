from fastapi import APIRouter, Request

from app.limiter import limiter
from app.models.horoscope import HoroscopeRequest, HoroscopeResponse
from app.services.horoscope_service import get_horoscope

router = APIRouter(prefix="/horoscope", tags=["horoscope"])


@router.post("", response_model=HoroscopeResponse)
@limiter.limit("30/hour")
async def horoscope(request: Request, body: HoroscopeRequest) -> HoroscopeResponse:
    result = await get_horoscope(body.constellation)
    return HoroscopeResponse(result=result)
