from fastapi import APIRouter, Request

from app.limiter import limiter
from app.models.tarot_simple import TarotReadingRequest, TarotReadingResponse
from app.services.ai_service import generate_tarot_simple

router = APIRouter(prefix="/tarot", tags=["tarot"])


@router.post("/reading", response_model=TarotReadingResponse)
@limiter.limit("20/hour")
async def tarot_reading(request: Request, body: TarotReadingRequest) -> TarotReadingResponse:
    result = await generate_tarot_simple(body.cardIds, body.worry)
    return TarotReadingResponse(result=result)
