# Запуск і конфігурація

```bash
dotnet run          # з кореня репозиторію; БД і міграції застосовуються автоматично
dotnet test         # 295 тестів (xUnit)
```

Змінні середовища: `DATABASE_URL` (Postgres), `PORT`, `FIREBASE_SERVICE_ACCOUNT_JSON` (вхід через Google), `GitHub__ClientSecret` (вхід через GitHub).
