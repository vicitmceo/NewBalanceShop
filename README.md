# New Balance Shop

> Інтернет-магазин спортивного одягу та взуття New Balance: каталог, кошик, замовлення, кабінет покупця та адмін-функції. Курсовий командний проєкт Team 1 на ASP.NET Core 9 із Clean Architecture.

[🌐 Live Demo](https://newbalanceshop-frontend.vercel.app)

| | Посилання |
|---|---|
| 🌐 Задеплоєний фронтенд | https://newbalanceshop-frontend.vercel.app |
| 📘 Документація коду (DocFX) | https://vicitmceo.github.io/NewBalanceShop/code/ |
| 📚 Документація ендпоінтів (Scalar) | https://vicitmceo.github.io/NewBalanceShop/api/ |
| 🧪 Swagger UI на продакшні | https://newbalanceshop-api.onrender.com/swagger |
| 🗂️ Сторінка документації | https://vicitmceo.github.io/NewBalanceShop/ |

Репозиторій фронтенду: https://github.com/vicitmceo/newbalanceshop-frontend

## 📖 About

Покупець переглядає каталог, додає товари в кошик і оформлює замовлення без обов'язкової реєстрації. Зареєстрований користувач має кабінет: профіль та історію замовлень. Адміністратор бачить список користувачів, може блокувати й видаляти їх. Інтерфейс доступний трьома мовами: українська, англійська, французька.

Проблема, яку розв'язує проєкт: складно швидко знайти й купити потрібну пару, часто змушують реєструватися, кошик зникає. Деталі, персони й UX-матеріали: [defense-materials](defense-materials/README.md).

## ✨ Features

- Каталог товарів із категоріями (кросівки, одяг, аксесуари)
- Кошик у сесії (не пишеться в БД до оформлення)
- Оформлення замовлення з перевіркою залишку на складі й розрахунком суми
- Реєстрація, вхід, вихід, поточний користувач
- Вхід через Google (Firebase) і GitHub (OAuth)
- Кабінет покупця: перегляд і редагування профілю, історія замовлень
- Адмін-функції: список користувачів, блокування, видалення
- Інтерфейс UA / EN / FR
- 145 автотестів, покриття коду 86,7%

## 🎨 Design

Інтерфейс у стилі бренду: біле тло, чорний і червоний `#cf0a2c`, шрифт Helvetica Neue. Картки товарів із типографськими плитками замість фото (прав на офіційні фото бренду немає). UI Kit, CJM, Empathy Map: [defense-materials/03-ux-research](defense-materials/03-ux-research).

## 🏗️ Architecture

```
Presentation (NewBalanceShop) ──► Application ──► Domain ◄── Infrastructure
```

| Проєкт | Відповідальність |
|---|---|
| `NewBalanceShop.Domain` | Сутності, інтерфейси репозиторіїв. Без залежностей |
| `NewBalanceShop.Application` | Сервіси, DTO, мапери, `PasswordHasher` |
| `NewBalanceShop.Infrastructure` | `ShopDbContext` (EF Core), репозиторії, міграції |
| `NewBalanceShop` | REST-контролери, DI, CORS, сесії |

Патерни: Repository, Dependency Injection, Service Layer, DTO + Mapper, Middleware, Session State. Діаграми: [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md), [defense-materials/04-architecture](defense-materials/04-architecture). Код: [документація DocFX](https://vicitmceo.github.io/NewBalanceShop/code/).

## 🛠️ Tech Stack

| Шар | Технології |
|---|---|
| Бекенд | .NET 9, ASP.NET Core Web API, EF Core 9, Npgsql |
| БД | PostgreSQL (Render) |
| Фронтенд | React, Vite, i18next, Firebase SDK |
| Тести | xUnit, Moq, WebApplicationFactory, EF InMemory, coverlet |
| Документація | XML-коментарі, DocFX, Scalar, Swashbuckle |
| Деплой | Docker, Render (API + БД), Vercel (фронтенд), GitHub Pages (документація) |

## 🔐 Authentication

Три способи входу, результат один: ID покупця в серверній сесії (HttpOnly cookie, `SameSite=None; Secure`, таймаут 30 хв).

1. Email і пароль: `POST /api/Auth/register`, `POST /api/Auth/login`
2. Google: фронтенд отримує Firebase ID-токен, `POST /api/Auth/google` перевіряє його на сервері
3. GitHub: OAuth authorization code flow, `/api/auth/github/login`

Пароль зберігається як PBKDF2-HMAC-SHA256: сіль 16 байт, 100 000 ітерацій, формат `ітерації.сіль.хеш`, порівняння у сталий за часом спосіб. Для входу через Google пароль не зберігається.

## 🛍️ Product Catalog

Товари живуть у таблиці `Products` (назва, бренд, категорія, розмір, колір, ціна, залишок). Початкові дані: 3 категорії й 3 товари (seed у `ShopDbContext`). Каталог віддає `GET /api/Products` (опційно `?categoryId=`), фронтенд показує його картками.

## 🐳 Docker

```bash
docker build -t newbalanceshop-api .
docker run -p 8080:8080 -e DATABASE_URL="postgres://user:pass@host:5432/newbalanceshop" newbalanceshop-api
```

`Dockerfile` двоетапний: збирання на `dotnet/sdk:9.0`, запуск на `dotnet/aspnet:9.0`.

## ⚙️ Configuration

| Змінна / ключ | Призначення |
|---|---|
| `ConnectionStrings:ShopDb` | Рядок підключення до Postgres для локального запуску (`appsettings.json`) |
| `DATABASE_URL` | URL Postgres на Render, має пріоритет над `ShopDb` |
| `PORT` | Порт, який слухає API (Render виставляє сам) |
| `FIREBASE_SERVICE_ACCOUNT_JSON` | JSON службового облікового запису Firebase для входу через Google |
| `GitHub__ClientSecret` | Секрет OAuth-застосунку GitHub (не зберігається в репозиторії) |
| `Frontend:Url` | Адреса фронтенду для редіректу після входу через GitHub |

Секрети передаються змінними середовища, у репозиторії їх немає.

## 🚀 Getting Started

Потрібні .NET 9 SDK і PostgreSQL.

```bash
git clone https://github.com/vicitmceo/NewBalanceShop.git
cd NewBalanceShop
# створіть БД newbalanceshop у локальному Postgres і за потреби змініть ConnectionStrings:ShopDb
dotnet run
```

Міграції застосовуються автоматично при старті (`db.Database.Migrate()`). Swagger: `/swagger`. Фронтенд запускається окремо з репозиторію [newbalanceshop-frontend](https://github.com/vicitmceo/newbalanceshop-frontend) (`npm install && npm run dev`).

## 📦 Build

```bash
dotnet build                                   # усе рішення
dotnet publish NewBalanceShop.csproj -c Release -o publish
./docfx/build.sh                               # перегенерувати сайт документації коду в docs/code
```

## 🌐 Deployment

| Частина | Платформа | Адреса |
|---|---|---|
| API + PostgreSQL | Render (Docker, `render.yaml`) | https://newbalanceshop-api.onrender.com |
| Фронтенд | Vercel | https://newbalanceshop-frontend.vercel.app |
| Документація | GitHub Pages (папка `docs/`) | https://vicitmceo.github.io/NewBalanceShop/ |

Деплой автоматичний: пуш у `master` запускає збирання на Render, GitHub Pages публікує `docs/`. Безкоштовний Render «засинає», перший запит може тривати до хвилини.

## 📸 Screenshots

| Каталог | Мобільна версія | Французька |
|---|---|---|
| ![Каталог](defense-materials/16-live-ui-screenshots/01-frontend-catalog-desktop.png) | ![Мобільна](defense-materials/16-live-ui-screenshots/02-frontend-catalog-mobile.png) | ![FR](defense-materials/16-live-ui-screenshots/03-frontend-french.png) |

Документація ендпоінтів: ![Scalar](defense-materials/16-live-ui-screenshots/04-api-docs-scalar.png)

## 🧪 Testing

```bash
dotnet test                                    # 145 тестів
dotnet test --collect:"XPlat Code Coverage"    # покриття
```

- `NewBalanceShop.Tests.Unit`: 102 тести (сервіси, мапери, контролери, `PasswordHasher`) на xUnit і Moq
- `NewBalanceShop.Tests.Integration`: 43 тести повного HTTP-конвеєра (`WebApplicationFactory`, EF InMemory)
- Покриття рядків: 86,7% (Infrastructure 95,8%, Application 90,2%, Domain 84,6%, Web API 82,0%)
- Звіт: [docs/test_report.html](docs/test_report.html)

## 🔒 Security

- Паролі: PBKDF2 із сіллю, відкритий текст ніде не зберігається
- Адмін-ендпоінти `CustomersController`: 401 без входу, 403 без ролі адміністратора; профіль і історію бачить лише власник або адміністратор
- SQL-ін'єкції: EF Core параметризує запити; XSS: React екранує вивід
- Секрети (БД, Firebase, GitHub) лише в змінних середовища

Відомі обмеження (у плані виправлення): ID покупця в `/api/Orders` і `checkout` береться з параметра запиту, CRUD товарів не перевіряє роль, CORS дозволяє будь-який origin із cookie, сесії зберігаються в пам'яті процесу.

## 📚 API

Повна документація HTTP-ендпоінтів: **https://vicitmceo.github.io/NewBalanceShop/api/** (Scalar), живий Swagger: https://newbalanceshop-api.onrender.com/swagger.

| Група | Ендпоінти |
|---|---|
| Products | `GET/POST /api/Products`, `GET/PUT/DELETE /api/Products/{id}` |
| Cart | `GET/POST /api/Cart`, `DELETE /api/Cart/{productId}`, `POST /api/Cart/checkout` |
| Orders | `GET/POST /api/Orders`, `GET /api/Orders/{id}` |
| Auth | `POST /api/Auth/register`, `login`, `google`, `logout`, `GET /api/Auth/me` |
| Customers | `GET /api/Customers`, `GET/PUT /api/Customers/{id}`, `GET /api/Customers/{id}/orders`, `PATCH /api/Customers/{id}/block`, `DELETE /api/Customers/{id}` |
| GitHub | `GET /api/auth/github/login`, `callback`, `me`, `POST logout` |

Документація коду (класи й методи): **https://vicitmceo.github.io/NewBalanceShop/code/**.

## 🗺️ Roadmap

- [ ] Форми входу й реєстрації та адмін-панель у React
- [ ] Закрити ризики безпеки: ID покупця з сесії, перевірка ролі для CRUD товарів, білий список CORS
- [ ] Фільтр за брендом, ціною й розміром (FR-03)
- [ ] CI: GitHub Actions (збирання й тести)
- [ ] Логування бізнес-подій, глобальна обробка помилок
- [ ] Сесії в Redis, унікальний індекс на email

## 👨‍💻 Authors

| Учасник | Роль |
|---|---|
| Панчук Вадим Владиславович | Team Lead |
| Ноцколіні Рафаїл Львович | Full-stack розробник: бекенд, фронтенд, DevOps, тести |

Команда Team 1, Jira: https://newbalance-team1.atlassian.net/jira/software/projects/SCRUM/boards/1/backlog

## 📄 License

Навчальний курсовий проєкт. Усі права залишаються за авторами; ліцензію на повторне використання не надано.
