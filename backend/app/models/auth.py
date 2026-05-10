from typing import Optional
from pydantic import BaseModel


class AuthRequest(BaseModel):
    token: Optional[str] = None        # 액세스 토큰 (기존)
    code: Optional[str] = None         # OAuth 인가 코드
    redirect_uri: Optional[str] = None # code 방식일 때 필수


class AuthResponse(BaseModel):
    jwt: str
