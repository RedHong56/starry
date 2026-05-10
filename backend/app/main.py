from contextlib import asynccontextmanager
from typing import AsyncGenerator

from fastapi import FastAPI
from fastapi.middleware.cors import CORSMiddleware
from slowapi import _rate_limit_exceeded_handler
from slowapi.errors import RateLimitExceeded
from slowapi.middleware import SlowAPIMiddleware

from app.database import Base, engine
from app.limiter import limiter
from app.routers import reading
from app.routers import auth, horoscope, payment, tarot, user
from app.services.card_loader import load_cards


@asynccontextmanager
async def lifespan(app: FastAPI) -> AsyncGenerator[None, None]:
    async with engine.begin() as conn:
        await conn.run_sync(Base.metadata.create_all)
    load_cards()
    yield


app = FastAPI(
    title="Starry Tarot API",
    version="0.2.0",
    lifespan=lifespan,
)

app.state.limiter = limiter
app.add_exception_handler(RateLimitExceeded, _rate_limit_exceeded_handler)
app.add_middleware(SlowAPIMiddleware)

app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],
    allow_methods=["*"],
    allow_headers=["*"],
)

app.include_router(reading.router)                   # /reading  (legacy)
app.include_router(auth.router,      prefix="/api")  # /api/auth/{provider}
app.include_router(user.router,      prefix="/api")  # /api/user/me|consume|ad-reward
app.include_router(payment.router,   prefix="/api")  # /api/payment/purchase
app.include_router(tarot.router,     prefix="/api")  # /api/tarot/reading
app.include_router(horoscope.router, prefix="/api")  # /api/horoscope


@app.get("/health", tags=["health"])
async def health() -> dict[str, str]:
    return {"status": "ok"}
