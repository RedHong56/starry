from fastapi import APIRouter, Depends, HTTPException
from sqlalchemy.ext.asyncio import AsyncSession

from app.database import User
from app.models.user import PurchaseRequest, PurchaseResponse
from app.routers.deps import get_current_user, get_db
from app.services import user_service
from app.services.payment_service import verify_google_purchase
from app.services.user_service import COIN_PRODUCTS

router = APIRouter(prefix="/payment", tags=["payment"])


@router.post("/purchase", response_model=PurchaseResponse)
async def purchase(
    request: PurchaseRequest,
    current_user: User = Depends(get_current_user),
    db: AsyncSession = Depends(get_db),
) -> PurchaseResponse:
    coins = COIN_PRODUCTS.get(request.productId)
    if coins is None:
        raise HTTPException(status_code=400, detail=f"알 수 없는 상품: {request.productId}")

    ok = await verify_google_purchase(request.productId, request.receipt)
    if not ok:
        raise HTTPException(status_code=402, detail="영수증 검증 실패")

    await user_service.add_coins(db, current_user, coins)
    return PurchaseResponse(ok=True, coins=current_user.coins)
