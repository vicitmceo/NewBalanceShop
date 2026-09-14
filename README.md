# New Balance Shop

Курсовий командний проєкт (Team 1) — інтернет-магазин спортивного одягу та взуття New Balance.

## Документація

- [TEAM.md](TEAM.md) — команда, ролі
- [SRS.md](SRS.md) — Software Requirements Specification
- [BACKLOG.md](BACKLOG.md) — Product Backlog і план спринтів
- [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md) — Clean Architecture, UML-діаграма класів

## Стек

ASP.NET Core Web API (.NET 9), EF Core, SQL Server (LocalDB), Clean Architecture (Presentation / Application / Domain / Infrastructure), Repository pattern.

## Запуск

Потрібен SQL Server LocalDB. Рядок підключення — `appsettings.json` → `ConnectionStrings:ShopDb`.

```bash
dotnet run
```

БД і таблиці створюються автоматично при першому запуску (`db.Database.Migrate()`), з тестовими даними (3 товари, 3 категорії).

## Ендпоінти

| Метод | URL | Опис |
|---|---|---|
| GET | `/api/products` | Список товарів (опційно `?categoryId=`) |
| GET | `/api/products/{id}` | Один товар |
| POST | `/api/products` | Створити товар |
| PUT | `/api/products/{id}` | Оновити товар |
| DELETE | `/api/products/{id}` | Видалити товар |
