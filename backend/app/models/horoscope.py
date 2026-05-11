from pydantic import BaseModel


class HoroscopeRequest(BaseModel):
    constellation: str
    language: str = "ko"


class HoroscopeResponse(BaseModel):
    result: str
