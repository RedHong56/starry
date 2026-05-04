from pydantic import BaseModel


class AuthRequest(BaseModel):
    token: str


class AuthResponse(BaseModel):
    jwt: str
