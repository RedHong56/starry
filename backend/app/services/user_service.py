"""In-memory user store. Replace with a real database for production."""
from __future__ import annotations

import uuid
from dataclasses import dataclass, field
from datetime import datetime, timedelta, timezone
from typing import Optional

from app.config import settings

COIN_PRODUCTS: dict[str, int] = {
    "coins_10": 10,
    "coins_30": 30,
    "coins_60": 60,
}


@dataclass
class UserRecord:
    user_id: str
    nickname: str
    coins: int = 0
    has_free_coupon: bool = True
    free_coupon_refresh_at: Optional[datetime] = field(default=None)


_by_id: dict[str, UserRecord] = {}
_by_social: dict[str, str] = {}  # "provider:sub" -> user_id


def get_or_create(provider: str, social_sub: str, nickname: str = "") -> UserRecord:
    key = f"{provider}:{social_sub}"
    if key not in _by_social:
        uid = str(uuid.uuid4())
        user = UserRecord(user_id=uid, nickname=nickname or f"Star_{uid[:6]}")
        _by_id[uid] = user
        _by_social[key] = uid
    return _by_id[_by_social[key]]


def get_by_id(user_id: str) -> Optional[UserRecord]:
    return _by_id.get(user_id)


def maybe_refresh_coupon(user: UserRecord) -> None:
    if not user.has_free_coupon and user.free_coupon_refresh_at:
        if datetime.now(timezone.utc) >= user.free_coupon_refresh_at:
            user.has_free_coupon = True
            user.free_coupon_refresh_at = None


def consume(user: UserRecord) -> bool:
    maybe_refresh_coupon(user)
    if user.has_free_coupon:
        user.has_free_coupon = False
        user.free_coupon_refresh_at = datetime.now(timezone.utc) + timedelta(
            hours=settings.free_coupon_interval_hours
        )
        return True
    if user.coins > 0:
        user.coins -= 1
        return True
    return False


def ad_reward(user: UserRecord) -> None:
    user.coins += 1


def add_coins(user: UserRecord, coins: int) -> None:
    user.coins += coins


def free_coupon_refresh_iso(user: UserRecord) -> Optional[str]:
    if user.free_coupon_refresh_at is None:
        return None
    return user.free_coupon_refresh_at.isoformat()
