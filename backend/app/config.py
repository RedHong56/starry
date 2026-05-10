from pydantic_settings import BaseSettings, SettingsConfigDict


class Settings(BaseSettings):
    model_config = SettingsConfigDict(env_file=".env", env_file_encoding="utf-8")

    openai_api_key: str = ""
    openai_model: str = "gpt-4o-mini"
    use_mock: bool = False
    port: int = 8000

    kakao_app_key: str = ""        # REST API 키
    kakao_client_secret: str = ""
    base_url: str = "http://localhost:8000"

    database_url: str = "postgresql+asyncpg://starry:starry@localhost:5432/starry"

    jwt_secret: str = "change-me-in-production"
    jwt_expire_hours: int = 720  # 30 days
    free_coupon_interval_hours: int = 24


settings = Settings()
