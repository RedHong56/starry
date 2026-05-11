from pydantic import BaseModel, Field


class TarotReadingRequest(BaseModel):
    cardIds: list[int] = Field(..., min_length=1, max_length=10)
    worry: str = Field(..., min_length=1, max_length=500)
    language: str = "ko"


class TarotReadingResponse(BaseModel):
    result: str
