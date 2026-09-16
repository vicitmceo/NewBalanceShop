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
| 6 | Domain-модель кошика/замовлення (`Order`, `OrderItem`) | Should | ✅ Done |
| 7 | API оформлення замовлення (`POST /api/orders`) | Must | ✅ Done |
| 8 | `CustomersController` — кабінет покупця (FR-10/FR-11) | Must | 🔲 To Do |
| 9 | Адміністрування користувачів — блокування/видалення (FR-12/FR-13) | Must | 🔲 To Do |
| 10 | Реєстрація/авторизація покупців (ASP.NET Core Identity/JWT) | Could | 🔲 To Do |
| 11 | Unit-тести (NUnit) для `ProductService`, `OrderService` | Should | 🔲 To Do |
| 12 | Кешування списку категорій/популярних товарів (`IMemoryCache`) | Could | 🔲 To Do |
| 13 | Логування (middleware обробки помилок + `ILogger`) | Should | 🔲 To Do |
| 14 | Документація API (Swagger/OpenAPI → HTML) | Must | 🔲 To Do |
| 15 | Frontend (React) | Should | 🔲 To Do |
| 16 | CI (GitHub Actions: build + test) | Could | 🔲 To Do |
| 17 | Деплой застосунку (для посилання на захисті) | Must | 🔲 To Do |

## Sprint "Catch-up" (14.09.2026) — наздоганяючий спринт

Мета: закрити прострочені тижні одним поштовхом — SRS, архітектура, робочий CRUD.

| Задача | Статус |
|---|---|
| Створити репозиторій команди | ✅ Done |
| SRS.md за прикладом іншої команди | ✅ Done |
| Clean Architecture + Repository pattern | ✅ Done |
| UML-діаграма класів (docs/ARCHITECTURE.md) | ✅ Done |
| CRUD `ProductsController` (перевірено складанням) | ✅ Done |

## Тиждень 3 — Sprint 2 (31.08 – 06.09.2026): Початок розробки

| Задача | Статус |
|---|---|
| Вступ до методології SCRUM: ролі Product Owner / Scrum Master / Development Team, обов'язки в команді | ✅ Done — [TEAM.md](TEAM.md) |
| Планування спринтів, ведення беклогу, канбан-дошка | ✅ Done — Jira Scrum board (посилання вище) |
| Базова структура проєкту ASP.NET Core (без поділу на 3–4 проєкти в солюшені — дозволено умовою) | ✅ Done — `Domain/Application/Infrastructure/Presentation` |
| Встановлення та налаштування інструментів (EF Core, SQL Server LocalDB) | ✅ Done |
| Реалізація початкових класів і модулів відповідно до архітектури (`Product`, `Category`, `Customer`, `Order`, репозиторії, `ProductService`) | ✅ Done |
| Планування та запуск наступного спринту (задачі нижче) | ✅ Done |

## Тиждень 4 — Sprint 3 (07.09 – 13.09.2026, дедлайн наздоганяючої здачі 08.09.2026)

| Задача | Статус |
|---|---|
| `POST /api/orders` — оформлення замовлення з кошика | ✅ Done (16.09.2026) |
| `Infrastructure/Repositories/CustomerRepository.cs` + `ICustomerRepository` | ✅ Done |
| `CustomersController` — кабінет покупця (FR-10/FR-11) | 🔲 To Do — задача на поточний тиждень |
| Адміністрування користувачів: блокування/видалення (FR-12/FR-13) | 🔲 To Do — задача на поточний тиждень |
| Unit-тести (NUnit + Moq) для `ProductService`, `OrderService` | 🔲 To Do — задача на поточний тиждень |
| Почати авторизацію покупців (ASP.NET Core Identity або JWT) | 🔲 To Do |
| Налаштувати канбан-дошку (Jira) для щотижневого трекінгу — додавати задачі на кожен тиждень наперед | ✅ Done, підтримується щотижня |

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
