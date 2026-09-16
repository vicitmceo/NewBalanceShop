# Product Backlog & Sprint Planning — New Balance Shop

Методологія: Scrum. Ролі — див. [TEAM.md](TEAM.md).

**Канбан-дошка (Jira):** https://newbalance-team1.atlassian.net/jira/software/projects/SCRUM/boards/1/backlog

**Контекст:** репозиторій команди був відсутній, і за попередні тижні (8–11 за журналом викладача) нараховувались штрафні бали за відсутність здач. Цей документ і супутній код — наздоганяючий випуск, що закриває SRS та архітектуру одним спринтом.

## Product Backlog (MoSCoW)

| # | Задача | Пріоритет | Статус |
|---|---|---|---|
| 1 | SRS документ | Must | ✅ Done |
| 2 | Clean Architecture (Presentation/Application/Domain/Infrastructure) | Must | ✅ Done |
| 3 | Repository pattern (`IRepository<T>`, `IProductRepository`, `IOrderRepository`) + EF Core | Must | ✅ Done |
| 4 | UML-діаграма класів з обґрунтуванням | Must | ✅ Done |
| 5 | CRUD товарів через REST API | Must | ✅ Done |
| 6 | Domain-модель кошика/замовлення (`Order`, `OrderItem`) | Should | ✅ Done (модель), API — заплановано |
| 7 | Реєстрація/авторизація покупців | Could | 🔲 To Do |
| 8 | API для оформлення замовлення (`POST /api/orders`) | Must | 🔲 To Do (наступний тиждень) |
| 9 | Unit-тести (NUnit) для `ProductService` | Should | 🔲 To Do |
| 10 | Frontend (React) | Should | 🔲 To Do |
| 11 | CI (GitHub Actions: build + test) | Could | 🔲 To Do |

## Sprint "Catch-up" (14.09.2026) — наздоганяючий спринт

Мета: закрити прострочені тижні одним поштовхом — SRS, архітектура, робочий CRUD.

| Задача | Статус |
|---|---|
| Створити репозиторій команди | ✅ Done |
| SRS.md за прикладом іншої команди | ✅ Done |
| Clean Architecture + Repository pattern | ✅ Done |
| UML-діаграма класів (docs/ARCHITECTURE.md) | ✅ Done |
| CRUD `ProductsController` (перевірено складанням) | ✅ Done |

## Наступний спринт (з 15.09.2026)

- Реалізувати `POST /api/orders` (оформлення замовлення з кошика) — ✅ Done (16.09.2026)
- `CustomersController` — кабінет покупця (FR-10/FR-11) та адміністрування користувачів (FR-12/FR-13)
- Додати unit-тести для `ProductService`, `OrderService` (NUnit + Moq, за зразком з іншого курсового проєкту команди)
- Почати авторизацію покупців (ASP.NET Core Identity або JWT)
- Налаштувати канбан-дошку (GitHub Projects) для щотижневого трекінгу

## Обов'язкові пункти до захисту (5 жовтня 2026, за методичкою)

Джерело: https://gist.github.com/sunmeat/27caafa2ff5b637879cd95e3de9c3081

| # | Пункт | Статус |
|---|---|---|
| 1 | Працездатна програма без помилок компіляції/виконання | ✅ Тримається (build перевіряється щокоміту) |
| 2 | Презентація PowerPoint/Keynote, 7–15 слайдів | 🔲 To Do |
| 3 | Відеоролик до 2 хв з демонстрацією роботи | 🔲 To Do |
| 4 | Звіт про командну роботу з Git (гілки, merge) | 🔲 To Do |
| 5 | Звіт про роботу за Scrum (дати спринтів, беклог, канбан, діаграми) | 🟡 Частково (беклог і Jira-дошка є, діаграм бракує) |
| 6 | NUnit-звіт про тестування | 🔲 To Do |
| 7 | HTML-документація API (Swagger/OpenAPI) | 🔲 To Do |
| 8 | Посилання на GitHub-репозиторій | ✅ https://github.com/vicitmceo/NewBalanceShop |
| 9 | Посилання на задеплоєний сайт | 🔲 To Do |
