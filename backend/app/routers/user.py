from fastapi import APIRouter, Depends, HTTPException

from app.models.user import AdRewardResponse, ConsumeResponse, UserMeResponse
from app.routers.deps import get_current_user
from app.services import user_service
from app.services.user_service import UserRecord

router = APIRouter(prefix="/user", tags=["user"])


@router.get("/me", response_model=UserMeResponse)
async def get_me(current_user: UserRecord = Depends(get_current_user)) -> UserMeResponse:
    return UserMeResponse(
        userId=current_user.user_id,
        nickname=current_user.nickname,
        coins=current_user.coins,
        hasFreeCoupon=current_user.has_free_coupon,
        freeCouponRefreshAt=user_service.free_coupon_refresh_iso(current_user),
    )


@router.post("/consume", response_model=ConsumeResponse)
async def consume(current_user: UserRecord = Depends(get_current_user)) -> ConsumeResponse:
    ok = user_service.consume(current_user)
    if not ok:
        raise HTTPException(status_code=402, detail="코인과 무료 쿠폰이 모두 소진되었습니다.")
    return ConsumeResponse(
        ok=True,
        coins=current_user.coins,
        hasFreeCoupon=current_user.has_free_coupon,
        freeCouponRefreshAt=user_service.free_coupon_refresh_iso(current_user),
    )


@router.post("/ad-reward", response_model=AdRewardResponse)
async def ad_reward(current_user: UserRecord = Depends(get_current_user)) -> AdRewardResponse:
    user_service.ad_reward(current_user)
    return AdRewardResponse(ok=True, coins=current_user.coins)
