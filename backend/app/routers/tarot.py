from fastapi import APIRouter

from app.models.tarot_simple import TarotReadingRequest, TarotReadingResponse
from app.services.ai_service import generate_tarot_simple

router = APIRouter(prefix="/tarot", tags=["tarot"])


@router.post("/reading", response_model=TarotReadingResponse)
async def tarot_reading(request: TarotReadingRequest) -> TarotReadingResponse:
    result = await generate_tarot_simple(request.cardIds, request.worry)
    return TarotReadingResponse(result=result)
