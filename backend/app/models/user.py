from pydantic import BaseModel


class UserMeResponse(BaseModel):
    userId: str
    nickname: str
    coins: int
    hasFreeCoupon: bool
    freeCouponRefreshAt: str | None = None


class ConsumeResponse(BaseModel):
    ok: bool
    coins: int
    hasFreeCoupon: bool
    freeCouponRefreshAt: str | None = None


class PurchaseRequest(BaseModel):
    productId: str
    receipt: str


class PurchaseResponse(BaseModel):
    ok: bool
    coins: int
