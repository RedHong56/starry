# Starry Backend

Starry 타로 앱의 백엔드 서버입니다. FastAPI + Gemini API를 사용해 타로 점괘를 생성합니다.

## 요구사항

- Python 3.11+
- [uv](https://docs.astral.sh/uv/) 패키지 매니저

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

`.env` 파일을 열어 `API_KEY`를 입력하세요.

| 변수 | 기본값 | 설명 |
|------|--------|------|
| `GEMINI_API_KEY` | (필수) | Gemini API 키 |
| `GEMINI_MODEL` | `gemini-2.0-flash-exp` | 사용할 Gemini 모델 |
| `PORT` | `8000` | 서버 포트 |
| `USE_MOCK` | `false` | `true`로 설정 시 실제 API 호출 없이 목 응답 반환 |

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

| 메서드 | 경로 | 설명 |
|--------|------|------|
| `GET` | `/health` | 서버 상태 확인 |
| `POST` | `/reading` | 타로 점괘 생성 |

### POST /reading 예시

**요청:**
```json
{
  "concern": "요즘 직장 생활이 너무 힘들어요. 앞으로 어떻게 해야 할까요?",
  "birth_date": "1995-03-15",
  "cards": [
    {"id": 0, "is_reversed": false},
    {"id": 56, "is_reversed": true},
    {"id": 21, "is_reversed": false}
  ]
}
```

**응답:**
```json
{
  "reading": "전체 점괘 텍스트 (한국어)",
  "card_interpretations": [
    {"card_id": 0, "interpretation": "카드 해석"},
    {"card_id": 56, "interpretation": "..."},
    {"card_id": 21, "interpretation": "..."}
  ]
}
```
