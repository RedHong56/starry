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
    """액세스 토큰으로 검증. Return (social_sub, nickname)."""
    if settings.use_mock:
        return token[:24], "테스트유저"

    if provider == "kakao":
        return await _verify_kakao(token)
    if provider == "google":
        return await _verify_google(token)
    if provider == "apple":
        return await _verify_apple(token)

    raise ValueError(f"Unknown provider: {provider}")


async def verify_social_code(provider: str, code: str, redirect_uri: str) -> tuple[str, str]:
    """OAuth 인가 코드로 검증. Return (social_sub, nickname)."""
    if settings.use_mock:
        return code[:24], "테스트유저"

    if provider == "kakao":
        return await _exchange_kakao_code(code, redirect_uri)

    raise ValueError(f"코드 방식 미지원 provider: {provider}")


async def _exchange_kakao_code(code: str, redirect_uri: str) -> tuple[str, str]:
    import httpx

    data = {
        "grant_type": "authorization_code",
        "client_id": settings.kakao_app_key,
        "redirect_uri": redirect_uri,
        "code": code,
    }
    if settings.kakao_client_secret:
        data["client_secret"] = settings.kakao_client_secret

    async with httpx.AsyncClient() as client:
        token_resp = await client.post(
            "https://kauth.kakao.com/oauth/token",
            data=data,
            headers={"Content-Type": "application/x-www-form-urlencoded"},
        )
    if token_resp.status_code != 200:
        raise PermissionError(f"Kakao code exchange failed: {token_resp.status_code} {token_resp.text}")

    access_token = token_resp.json()["access_token"]
    return await _verify_kakao(access_token)


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
