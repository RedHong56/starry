from __future__ import annotations

import uuid
from datetime import datetime, timedelta, timezone
from typing import Optional

from sqlalchemy import select
from sqlalchemy.ext.asyncio import AsyncSession

from app.config import settings
from app.database import User

COIN_PRODUCTS: dict[str, int] = {
    "coins_10": 10,
    "coins_30": 30,
    "coins_60": 60,
}


async def get_or_create(db: AsyncSession, provider: str, social_sub: str, nickname: str = "") -> User:
    result = await db.execute(
        select(User).where(User.provider == provider, User.social_sub == social_sub)
    )
    user = result.scalar_one_or_none()
    if user is None:
        uid = str(uuid.uuid4())
        user = User(
            user_id=uid,
            provider=provider,
            social_sub=social_sub,
            nickname=nickname or f"Star_{uid[:6]}",
        )
        db.add(user)
        await db.commit()
        await db.refresh(user)
    return user


async def get_by_id(db: AsyncSession, user_id: str) -> Optional[User]:
    result = await db.execute(select(User).where(User.user_id == user_id))
    return result.scalar_one_or_none()


async def maybe_refresh_coupon(db: AsyncSession, user: User) -> None:
    if not user.has_free_coupon and user.free_coupon_refresh_at:
        if datetime.now(timezone.utc) >= user.free_coupon_refresh_at:
            user.has_free_coupon = True
            user.free_coupon_refresh_at = None
            await db.commit()


async def consume(db: AsyncSession, user: User) -> bool:
    await maybe_refresh_coupon(db, user)
    if user.has_free_coupon:
        user.has_free_coupon = False
        user.free_coupon_refresh_at = datetime.now(timezone.utc) + timedelta(
            hours=settings.free_coupon_interval_hours
        )
        await db.commit()
        return True
    if user.coins > 0:
        user.coins -= 1
        await db.commit()
        return True
    return False


async def ad_reward(db: AsyncSession, user: User) -> None:
    user.coins += 1
    await db.commit()


async def add_coins(db: AsyncSession, user: User, coins: int) -> None:
    user.coins += coins
    await db.commit()


def free_coupon_refresh_iso(user: User) -> Optional[str]:
    if user.free_coupon_refresh_at is None:
        return None
    return user.free_coupon_refresh_at.isoformat()
