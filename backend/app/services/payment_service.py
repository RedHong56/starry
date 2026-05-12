from __future__ import annotations

import asyncio
import json
import logging

from app.config import settings

logger = logging.getLogger(__name__)


def _parse_receipt(receipt_json: str) -> tuple[str, str]:
    """Unity IAP 영수증에서 (productId, purchaseToken) 추출."""
    receipt = json.loads(receipt_json)
    payload = json.loads(receipt["Payload"])
    purchase_data = json.loads(payload["json"])
    return purchase_data["productId"], purchase_data["purchaseToken"]


def _verify_sync(product_id: str, purchase_token: str) -> bool:
    from google.oauth2 import service_account
    from googleapiclient.discovery import build

    creds = service_account.Credentials.from_service_account_info(
        json.loads(settings.google_service_account_json),
        scopes=["https://www.googleapis.com/auth/androidpublisher"],
    )
    service = build("androidpublisher", "v3", credentials=creds)
    result = (
        service.purchases()
        .products()
        .get(
            packageName=settings.google_package_name,
            productId=product_id,
            token=purchase_token,
        )
        .execute()
    )
    # purchaseState: 0 = Purchased
    return result.get("purchaseState") == 0


async def verify_google_purchase(product_id: str, receipt_json: str) -> bool:
    """Google Play 영수증을 서버-투-서버로 검증. False면 코인 지급 거부."""
    if not settings.google_service_account_json or not settings.google_package_name:
        logger.warning("[payment] Google 서비스 계정 미설정 — 검증 스킵 (개발 환경)")
        return True

    try:
        _, purchase_token = _parse_receipt(receipt_json)
        return await asyncio.to_thread(_verify_sync, product_id, purchase_token)
    except Exception as e:
        logger.error(f"[payment] 영수증 검증 실패: {e}")
        return False
