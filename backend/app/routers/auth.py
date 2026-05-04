from fastapi import APIRouter, HTTPException, Path

from app.models.auth import AuthRequest, AuthResponse
from app.services import auth_service, user_service

router = APIRouter(prefix="/auth", tags=["auth"])

_ALLOWED_PROVIDERS = {"kakao", "google", "apple"}


@router.post("/{provider}", response_model=AuthResponse)
async def authenticate(
    request: AuthRequest,
    provider: str = Path(..., description="소셜 로그인 제공자 (kakao / google / apple)"),
) -> AuthResponse:
    if provider not in _ALLOWED_PROVIDERS:
        raise HTTPException(status_code=400, detail=f"지원하지 않는 provider: {provider}")

    try:
        social_sub, nickname = await auth_service.verify_social_token(provider, request.token)
    except PermissionError as e:
        raise HTTPException(status_code=401, detail=str(e)) from e

    user = user_service.get_or_create(provider, social_sub, nickname)
    token = auth_service.create_jwt(user.user_id)
    return AuthResponse(jwt=token)
