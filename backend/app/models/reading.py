from pydantic import BaseModel, Field


class CardInput(BaseModel):
    id: int = Field(..., ge=0, le=77, description="카드 ID (0~77)", examples=[0])
    is_reversed: bool = Field(..., description="역방향 여부", examples=[False])


class ReadingRequest(BaseModel):
    concern: str = Field(
        ...,
        min_length=1,
        max_length=500,
        description="사용자의 고민",
        examples=["요즘 직장 생활이 너무 힘들어요. 앞으로 어떻게 해야 할까요?"],
    )
    birth_date: str = Field(
        ...,
        pattern=r"^\d{4}-\d{2}-\d{2}$",
        description="생년월일 (YYYY-MM-DD)",
        examples=["1995-03-15"],
    )
    cards: list[CardInput] = Field(
        ...,
        min_length=3,
        max_length=3,
        description="선택된 카드 3장",
        examples=[[{"id": 0, "is_reversed": False}, {"id": 56, "is_reversed": True}, {"id": 21, "is_reversed": False}]],
    )


class CardInterpretation(BaseModel):
    card_id: int = Field(..., description="카드 ID")
    interpretation: str = Field(..., description="카드 해석 (한국어)")


class ReadingResponse(BaseModel):
    reading: str = Field(..., description="전체 점괘 텍스트 (한국어)")
    card_interpretations: list[CardInterpretation] = Field(
        ..., description="카드별 개별 해석"
    )
