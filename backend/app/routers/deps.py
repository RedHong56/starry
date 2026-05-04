from fastapi import Header, HTTPException
from jwt import ExpiredSignatureError, InvalidTokenError

from app.services import auth_service, user_service
from app.services.user_service import UserRecord


async def get_current_user(authorization: str | None = Header(default=None)) -> UserRecord:
    if not authorization or not authorization.startswith("Bearer "):
        raise HTTPException(status_code=401, detail="Authorization 헤더가 없거나 형식이 올바르지 않습니다.")
    token = authorization[len("Bearer "):]
    try:
        payload = auth_service.decode_jwt(token)
    except ExpiredSignatureError:
        raise HTTPException(status_code=401, detail="토큰이 만료되었습니다.")
    except InvalidTokenError:
        raise HTTPException(status_code=401, detail="유효하지 않은 토큰입니다.")

    user = user_service.get_by_id(payload["sub"])
    if user is None:
        raise HTTPException(status_code=401, detail="사용자를 찾을 수 없습니다.")

    user_service.maybe_refresh_coupon(user)
    return user
