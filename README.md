# Starry — AI 타로 점괘 모바일 애플리케이션

> 3D 타로술사 캐릭터와 AI 해석 엔진이 결합된 모바일 타로 앱

## 프로젝트 소개

Starry는 사용자의 고민을 입력받아 과거-현재-미래 3장의 타로카드를 기반으로
AI가 개인 맞춤형 점괘를 제공하는 모바일 애플리케이션입니다.
소셜 로그인 후 별가루(코인)를 획득하거나 구매해 타로를 열람할 수 있으며,
별자리 운세 기능도 함께 제공합니다.

## 기술 스택

| 영역 | 기술 |
|---|---|
| 클라이언트 | Unity 2022.3 LTS (C#) |
| 백엔드 | FastAPI 0.2, Python 3.11 |
| AI | OpenAI GPT-4o mini |
| 데이터베이스 | PostgreSQL (SQLAlchemy + asyncpg) |
| 인증 | Kakao / Google / Apple OAuth → JWT |
| 결제 | Unity IAP + Google Play 서버 검증 |
| 광고 | Google AdMob (Rewarded Ad) |
| 인프라 | Railway (백엔드), Google Play (Android) |

## 프로젝트 구조

```
starry/
├── card_pipeline/   # 78장 타로카드 AI 자동 생성 파이프라인
├── backend/         # FastAPI 서버 (인증·결제·타로·운세)
├── client/          # Unity 모바일 앱 (Android)
├── assets/          # 3D 에셋 원본
└── docs/            # 기획 및 기술 문서
```

## 주요 기능

### 타로 리딩
- 78장 타로카드 중 3장 드로우 (과거·현재·미래)
- OpenAI GPT-4o mini 기반 개인 맞춤형 점괘 (응답 약 3초)
- 한국어 / 영어 다국어 지원

### 별자리 운세
- 12개 별자리별 일일 운세 제공
- 요청 레이트 리밋 적용 (30회/시간)

### 인증 · 사용자
- Kakao / Google / Apple 소셜 로그인 → JWT 딥링크 전달 (`starry://auth?jwt=...`)
- 24시간마다 무료 쿠폰 자동 지급
- 별가루(코인) 잔액 조회 · 차감

### 수익 모델
- **Rewarded Ad**: 광고 시청으로 타로 1회 무료 이용
- **IAP**: 별가루 패키지 구매 (₩1,200 / ₩3,300 / ₩6,600)
  - Google Play 서버-투-서버 영수증 검증 (서비스 계정 API)
  - `PurchaseProcessingResult.Pending` 패턴으로 중복 지급 방지

### 카드 생성 파이프라인
- AI 이미지 생성 + 자동 합성으로 78장 제작 (수작업 8시간 → 30분)

## 배포 현황

- Android: Google Play 비공개 테스트 진행 중
- 백엔드: Railway에 배포, PostgreSQL 연동

## 블로그 포스트

- [AI로 타로카드 78장 자동 생성 파이프라인 구축기]
- [Unity IAP + Google Play 서버 검증 구현기]
- [모바일 앱 소셜 로그인 딥링크 연동 (Kakao · Google · Apple)]

## 다운로드

- [Google Play](링크) (비공개 테스트)

## 라이선스

MIT License
