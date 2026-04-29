from fastapi import APIRouter

from app.models.reading import ReadingRequest, ReadingResponse
from app.services.ai_service import generate_reading

router = APIRouter(prefix="/reading", tags=["reading"])


@router.post("", response_model=ReadingResponse)
async def create_reading(request: ReadingRequest) -> ReadingResponse:
    return await generate_reading(
        concern=request.concern,
        birth_date=request.birth_date,
        cards=request.cards,
    )
