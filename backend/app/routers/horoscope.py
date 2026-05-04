from fastapi import APIRouter

from app.models.horoscope import HoroscopeRequest, HoroscopeResponse
from app.services.horoscope_service import get_horoscope

router = APIRouter(prefix="/horoscope", tags=["horoscope"])


@router.post("", response_model=HoroscopeResponse)
async def horoscope(request: HoroscopeRequest) -> HoroscopeResponse:
    result = await get_horoscope(request.constellation)
    return HoroscopeResponse(result=result)
