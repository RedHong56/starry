from urllib.parse import urlencode

from fastapi import APIRouter, Depends, HTTPException, Path
from fastapi.responses import RedirectResponse
from sqlalchemy.ext.asyncio import AsyncSession

from app.config import settings
from app.models.auth import AuthRequest, AuthResponse
from app.routers.deps import get_db
from app.services import auth_service, user_service

router = APIRouter(prefix="/auth", tags=["auth"])

_ALLOWED_PROVIDERS = {"kakao", "google", "apple"}

_KAKAO_REDIRECT_URI  = f"{settings.base_url}/api/auth/kakao/callback"
_GOOGLE_REDIRECT_URI = f"{settings.base_url}/api/auth/google/callback"


@router.get("/kakao/callback")
async def kakao_callback(code: str, db: AsyncSession = Depends(get_db)):
    """카카오 OAuth 브라우저 콜백 — JWT를 딥링크로 앱에 전달"""
    try:
        social_sub, nickname = await auth_service.verify_social_code("kakao", code, _KAKAO_REDIRECT_URI)
    except PermissionError as e:
        return RedirectResponse(url=f"starry://auth?error={e}")

    user = await user_service.get_or_create(db, "kakao", social_sub, nickname)
    token = auth_service.create_jwt(user.user_id)
    return RedirectResponse(url=f"starry://auth?jwt={token}")


@router.get("/google")
async def google_login():
    """Google OAuth 시작 — 브라우저를 Google 로그인 페이지로 리다이렉트"""
    params = {
        "client_id": settings.google_client_id,
        "redirect_uri": _GOOGLE_REDIRECT_URI,
        "response_type": "code",
        "scope": "openid email profile",
    }
    return RedirectResponse(url="https://accounts.google.com/o/oauth2/v2/auth?" + urlencode(params))


@router.get("/google/callback")
async def google_callback(code: str, db: AsyncSession = Depends(get_db)):
    """Google OAuth 브라우저 콜백 — JWT를 딥링크로 앱에 전달"""
    try:
        social_sub, nickname = await auth_service.verify_social_code("google", code, _GOOGLE_REDIRECT_URI)
    except PermissionError as e:
        return RedirectResponse(url=f"starry://auth?error={e}")

    user = await user_service.get_or_create(db, "google", social_sub, nickname)
    token = auth_service.create_jwt(user.user_id)
    return RedirectResponse(url=f"starry://auth?jwt={token}")


@router.post("/{provider}", response_model=AuthResponse)
async def authenticate(
    request: AuthRequest,
    provider: str = Path(..., description="소셜 로그인 제공자 (google / apple)"),
    db: AsyncSession = Depends(get_db),
) -> AuthResponse:
    if provider not in _ALLOWED_PROVIDERS:
        raise HTTPException(status_code=400, detail=f"지원하지 않는 provider: {provider}")

    try:
        if request.token:
            social_sub, nickname = await auth_service.verify_social_token(provider, request.token)
        else:
            raise HTTPException(status_code=400, detail="token 필요")
    except PermissionError as e:
        raise HTTPException(status_code=401, detail=str(e)) from e

    user = await user_service.get_or_create(db, provider, social_sub, nickname)
    token = auth_service.create_jwt(user.user_id)
    return AuthResponse(jwt=token)
