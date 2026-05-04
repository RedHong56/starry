from pydantic import BaseModel


class HoroscopeRequest(BaseModel):
    constellation: str


class HoroscopeResponse(BaseModel):
    result: str
