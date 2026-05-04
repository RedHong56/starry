from pydantic_settings import BaseSettings, SettingsConfigDict


class Settings(BaseSettings):
    model_config = SettingsConfigDict(env_file=".env", env_file_encoding="utf-8")

    openai_api_key: str = ""
    openai_model: str = "gpt-4o-mini"
    use_mock: bool = False
    port: int = 8000

    jwt_secret: str = "change-me-in-production"
    jwt_expire_hours: int = 720  # 30 days
    free_coupon_interval_hours: int = 24


settings = Settings()
