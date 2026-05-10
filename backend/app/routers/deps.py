from typing import AsyncGenerator

from fastapi import Depends, Header, HTTPException
from jwt import ExpiredSignatureError, InvalidTokenError
from sqlalchemy.ext.asyncio import AsyncSession

from app.database import User, async_session
from app.services import auth_service, user_service


async def get_db() -> AsyncGenerator[AsyncSession, None]:
    async with async_session() as session:
        yield session


async def get_current_user(
    authorization: str | None = Header(default=None),
    db: AsyncSession = Depends(get_db),
) -> User:
    if not authorization or not authorization.startswith("Bearer "):
        raise HTTPException(status_code=401, detail="Authorization 헤더가 없거나 형식이 올바르지 않습니다.")
    token = authorization[len("Bearer "):]
    try:
        payload = auth_service.decode_jwt(token)
    except ExpiredSignatureError:
        raise HTTPException(status_code=401, detail="토큰이 만료되었습니다.")
    except InvalidTokenError:
        raise HTTPException(status_code=401, detail="유효하지 않은 토큰입니다.")

    user = await user_service.get_by_id(db, payload["sub"])
    if user is None:
        raise HTTPException(status_code=401, detail="사용자를 찾을 수 없습니다.")

    await user_service.maybe_refresh_coupon(db, user)
    return user
