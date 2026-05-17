# Starry Backend

Starry 타로 앱의 백엔드 서버입니다. FastAPI + OpenAI API를 사용해 타로 점괘와 별자리 운세를 생성합니다.

## 요구사항

- Python 3.11+
- [uv](https://docs.astral.sh/uv/) 패키지 매니저
- PostgreSQL

## 설치

```bash
cd backend

# 가상환경 생성 및 의존성 설치
uv sync

# 개발 의존성 포함 설치
uv sync --extra dev
```

## 환경변수 설정

```bash
cp .env.example .env
```

| 변수 | 기본값 | 설명 |
|------|--------|------|
| `OPENAI_API_KEY` | (필수) | OpenAI API 키 |
| `OPENAI_MODEL` | `gpt-4o-mini` | 사용할 OpenAI 모델 |
| `DATABASE_URL` | `postgresql+asyncpg://starry:starry@localhost:5432/starry` | PostgreSQL 연결 URL |
| `JWT_SECRET` | `change-me-in-production` | JWT 서명 시크릿 (프로덕션 필수 변경) |
| `JWT_EXPIRE_HOURS` | `720` | JWT 만료 시간 (기본 30일) |
| `BASE_URL` | `http://localhost:8000` | 서버 베이스 URL (OAuth 콜백 URI 생성에 사용) |
| `FREE_COUPON_INTERVAL_HOURS` | `24` | 무료 쿠폰 재충전 간격 (시간) |
| `KAKAO_APP_KEY` | `` | 카카오 REST API 키 |
| `KAKAO_CLIENT_SECRET` | `` | 카카오 클라이언트 시크릿 |
| `GOOGLE_CLIENT_ID` | `` | Google OAuth 클라이언트 ID |
| `GOOGLE_CLIENT_SECRET` | `` | Google OAuth 클라이언트 시크릿 |
| `GOOGLE_PACKAGE_NAME` | `` | Android 앱 패키지명 (IAP 검증용) |
| `GOOGLE_SERVICE_ACCOUNT_JSON` | `` | Google Play 서비스 계정 JSON 전체 (한 줄) |
| `USE_MOCK` | `false` | `true`로 설정 시 AI API 호출 없이 목 응답 반환 |
| `PORT` | `8000` | 서버 포트 |

> `GOOGLE_SERVICE_ACCOUNT_JSON`은 서비스 계정 키 JSON 파일 내용을 한 줄로 압축해 저장하세요.  
> `private_key` 필드의 개행이 `\n` 이스케이프 시퀀스 형태인지 반드시 확인하세요.

## 실행

```bash
# 프로덕션
uv run uvicorn app.main:app --host 0.0.0.0 --port 8000

# 개발 (자동 리로드)
uv run uvicorn app.main:app --reload --port 8000

# 목 모드 (API 키 없이 테스트)
USE_MOCK=true uv run uvicorn app.main:app --reload
```

## API 문서

서버 실행 후 브라우저에서 확인:
- Swagger UI: http://localhost:8000/docs
- ReDoc: http://localhost:8000/redoc

## 테스트

```bash
uv run pytest -v
```

## 주요 엔드포인트

| 메서드 | 경로 | 인증 | 설명 |
|--------|------|------|------|
| `GET` | `/health` | - | 서버 상태 확인 |
| `GET` | `/api/auth/kakao/callback` | - | 카카오 OAuth 콜백 → JWT 딥링크 |
| `GET` | `/api/auth/google` | - | Google OAuth 시작 |
| `GET` | `/api/auth/google/callback` | - | Google OAuth 콜백 → JWT 딥링크 |
| `POST` | `/api/auth/{provider}` | - | 소셜 토큰으로 JWT 발급 (google / apple) |
| `GET` | `/api/user/me` | JWT | 내 정보 조회 (코인·무료쿠폰) |
| `POST` | `/api/user/consume` | JWT | 별가루 또는 무료 쿠폰 1회 차감 |
| `POST` | `/api/payment/purchase` | JWT | IAP 영수증 검증 후 코인 지급 |
| `POST` | `/api/tarot/reading` | JWT | 타로 점괘 생성 (OpenAI) |
| `POST` | `/api/horoscope` | - | 별자리 운세 조회 (30회/시간 제한) |

### 소셜 로그인 흐름

```
앱 → 브라우저 오픈 → /api/auth/{provider} 콜백
  → JWT 생성 → starry://auth?jwt=<token> 딥링크
  → 앱에서 딥링크 수신 → 로컬 저장
```

### POST /api/tarot/reading 예시

**요청:**
```json
{
  "cardIds": [0, 56, 21],
  "worry": "요즘 직장 생활이 너무 힘들어요.",
  "lang": "ko"
}
```

**응답:**
```
과거·현재·미래 카드를 종합한 한국어 타로 점괘 텍스트 (400~500자)
```

### POST /api/payment/purchase 예시

**요청:**
```json
{
  "productId": "stardust_30",
  "receipt": "<Unity IAP 영수증 JSON>"
}
```

**응답:**
```json
{
  "ok": true,
  "coins": 30
}
```

## IAP 상품 목록

| productId | 별가루 | 가격 |
|-----------|--------|------|
| `stardust_10` | 10개 | ₩1,200 |
| `stardust_30` | 30개 | ₩3,300 |
| `stardust_70` | 70개 | ₩6,600 |
