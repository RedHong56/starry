import logging
from datetime import datetime, timedelta, timezone

import jwt

from app.config import settings

logger = logging.getLogger(__name__)


def create_jwt(user_id: str) -> str:
    now = datetime.now(timezone.utc)
    payload = {
        "sub": user_id,
        "iat": now,
        "exp": now + timedelta(hours=settings.jwt_expire_hours),
    }
    return jwt.encode(payload, settings.jwt_secret, algorithm="HS256")


def decode_jwt(token: str) -> dict:
    return jwt.decode(token, settings.jwt_secret, algorithms=["HS256"])


async def verify_social_token(provider: str, token: str) -> tuple[str, str]:
    """Return (social_sub, nickname). Uses mock in use_mock mode."""
    if settings.use_mock:
        return token[:24], "테스트유저"

    if provider == "kakao":
        return await _verify_kakao(token)
    if provider == "google":
        return await _verify_google(token)
    if provider == "apple":
        return await _verify_apple(token)

    raise ValueError(f"Unknown provider: {provider}")


async def _verify_kakao(token: str) -> tuple[str, str]:
    import httpx

    async with httpx.AsyncClient() as client:
        resp = await client.get(
            "https://kapi.kakao.com/v2/user/me",
            headers={"Authorization": f"Bearer {token}"},
        )
    if resp.status_code != 200:
        raise PermissionError(f"Kakao token invalid: {resp.status_code}")
    data = resp.json()
    sub = str(data["id"])
    nickname = data.get("kakao_account", {}).get("profile", {}).get("nickname", "")
    return sub, nickname


async def _verify_google(token: str) -> tuple[str, str]:
    import httpx

    async with httpx.AsyncClient() as client:
        resp = await client.get(
            "https://oauth2.googleapis.com/userinfo",
            headers={"Authorization": f"Bearer {token}"},
        )
    if resp.status_code != 200:
        raise PermissionError(f"Google token invalid: {resp.status_code}")
    data = resp.json()
    return data["sub"], data.get("name", "")


async def _verify_apple(token: str) -> tuple[str, str]:
    # Apple Sign-In requires verifying a JWT signed by Apple's private key.
    # Full implementation needs apple-auth library or manual JWKS verification.
    # For now fall back to treating the token as the sub (stub).
    logger.warning("Apple token verification is not fully implemented; using token as sub.")
    return token[:24], ""
