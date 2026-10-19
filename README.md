# New Balance Shop

> Інтернет-магазин спортивного одягу та взуття New Balance: каталог, кошик, замовлення, кабінет покупця та адмін-функції. Курсовий командний проєкт Team 1 на ASP.NET Core 9 із Clean Architecture.

[🌐 Live Demo](https://newbalance.cc.cd)

| | Посилання |
|---|---|
| 🌐 Задеплоєний фронтенд | https://newbalance.cc.cd |
| 📘 Документація коду (DocFX) | https://vicitmceo.github.io/NewBalanceShop/code/ |
| 📚 Документація ендпоінтів (Scalar) | https://vicitmceo.github.io/NewBalanceShop/api/ |
| 🧪 Swagger UI на продакшні | https://newbalanceshop-api.onrender.com/swagger |
| 🗂️ Сторінка документації | https://vicitmceo.github.io/NewBalanceShop/ |

Репозиторій фронтенду: https://github.com/vicitmceo/newbalanceshop-frontend

## 📖 About

Покупець переглядає каталог за розділами Чоловікам / Жінкам / Дітям / Розпродаж, додає товари в кошик і оформлює замовлення без обов'язкової реєстрації. Зареєстрований користувач має кабінет: профіль та історію замовлень. Адміністратор бачить список користувачів, може блокувати й видаляти їх. Інтерфейс доступний трьома мовами: українська, англійська, французька.

Проблема, яку розв'язує проєкт: складно швидко знайти й купити потрібну пару, часто змушують реєструватися, кошик зникає. Деталі, персони й UX-матеріали: [defense-materials](defense-materials/README.md).

## ✨ Features

- Каталог товарів із категоріями (кросівки, одяг, аксесуари) і навігацією як на newbalance.com: Чоловікам / Жінкам / Дітям / Розпродаж
- Товар має стать (`men`, `women`, `kids`, `unisex`), тип (`Running`, `Lifestyle`, `Jackets & Vests`, `Socks`), доступні розміри, галерею зображень і стару ціну для розпродажу
- Фільтри каталогу: стать, тип, розпродаж, категорія, бренд, діапазон цін, розмір, пошук за назвою, брендом і типом; сортування за ціною, назвою й новизною
- Список категорій кешується в пам'яті сервера
- Кошик у сесії (не пишеться в БД до оформлення): позиція визначається парою «товар + розмір»
- Оформлення замовлення з перевіркою залишку на складі й розрахунком суми
- Реєстрація, вхід, вихід, поточний користувач
- Вхід через Google (Firebase) і GitHub (OAuth)
- Кабінет покупця: перегляд і редагування профілю, історія замовлень
- Адмін-функції: список користувачів, блокування, видалення, керування товарами й статусами замовлень
- Інтерфейс UA / EN / FR
- 295 автотестів, покриття коду 93,4%

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

Товари живуть у таблиці `Products`: назва, бренд, стать (`Gender`), тип (`ProductType`), категорія, розміри через кому, колір, ціна, стара ціна (`OldPrice`, `null` — не на розпродажі), залишок, головне зображення, галерея (`ExtraImages`) й опис. Для наявних рядків міграція виставляє `Gender = "unisex"`, порожній тип і відсутність знижки.

Початкові дані в `ShopDbContext` містять лише 3 категорії, службових покупців і стартові товари для локального запуску. Живий каталог наповнюється адміністратором через API або скриптом [scripts/seed_catalog.py](scripts/seed_catalog.py):

```bash
NBS_ADMIN_EMAIL=... NBS_ADMIN_PASSWORD=... \
  python3 scripts/seed_catalog.py catalog.json https://newbalanceshop-api.onrender.com [--dry-run]
```

Скрипт читає JSON-масив товарів, входить адміністратором і приводить каталог у відповідність до файлу: наявні товари оновлює на місці за зростанням `id` (щоб посилання зі старих замовлень лишилися чинними), решту створює, зайві видаляє, а якщо в них є замовлення, обнуляє залишок. Повторний запуск із тим самим файлом нічого не змінює (збіг за назвою й `imageUrl`).

`GET /api/Products` приймає необов'язкові фільтри, які можна комбінувати:

| Параметр | Значення |
|---|---|
| `gender` | `men` або `women` (разом із товарами `unisex`), `kids` (лише дитячі) |
| `type` | точний тип товару без урахування регістру, наприклад `Running` |
| `sale` | `true` залишає лише товари зі старою ціною |
| `categoryId`, `brand`, `minPrice`, `maxPrice`, `size` | категорія, бренд, діапазон цін, розмір |
| `q` | підрядок у назві, бренді, типі, описі чи кольорі без урахування регістру |
| `sort` | `price_asc`, `price_desc`, `name`, `new` (новіші спочатку) |

Категорії віддає `GET /api/Categories`; список кешується в `IMemoryCache` на 5 хвилин.

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
| Фронтенд | Vercel | https://newbalance.cc.cd (також https://newbalanceshop.vercel.app) |
| Документація | GitHub Pages (папка `docs/`) | https://vicitmceo.github.io/NewBalanceShop/ |

Деплой автоматичний: пуш у `master` запускає збирання на Render, GitHub Pages публікує `docs/`. Безкоштовний Render «засинає», перший запит може тривати до хвилини.

## 📸 Screenshots

| Каталог | Мобільна версія | Французька |
|---|---|---|
| ![Каталог](defense-materials/16-live-ui-screenshots/01-frontend-catalog-desktop.png) | ![Мобільна](defense-materials/16-live-ui-screenshots/02-frontend-catalog-mobile.png) | ![FR](defense-materials/16-live-ui-screenshots/03-frontend-french.png) |

Документація ендпоінтів (Scalar): ![Scalar](defense-materials/16-live-ui-screenshots/04-api-docs-scalar.png)

Swagger UI на продакшні: ![Swagger](defense-materials/12-documentation/swagger-ui-render.png)

Документація коду (DocFX): ![DocFX](defense-materials/12-documentation/docfx-code-docs.png)

## 🧪 Testing

```bash
dotnet test                                    # 295 тестів
dotnet test --collect:"XPlat Code Coverage"    # покриття
```

- `NewBalanceShop.Tests.Unit`: 190 тестів (сервіси, мапери, валідація DTO, контролери, middleware, `PasswordHasher`) на xUnit і Moq
- `NewBalanceShop.Tests.Integration`: 105 тестів повного HTTP-конвеєра й репозиторіїв (`WebApplicationFactory`, EF InMemory)
- Покриття рядків: 93,4% без згенерованих міграцій (Infrastructure 90,2%, Application 99,8%, Domain 88,6%, Web API 89,8%)
- Звіт: [docs/test_report.html](docs/test_report.html) (знімок від 30.09.2026)

## 🔒 Security

- Паролі: PBKDF2 із сіллю, відкритий текст ніде не зберігається
- ID покупця береться лише із сесії, а не з параметрів запиту: замовлення, кошик і профіль захищені від BOLA/IDOR; чужі дані бачить лише власник або адміністратор
- Створення, зміна й видалення товарів, список усіх замовлень і зміна їхнього статусу, керування користувачами: 401 без входу, 403 без ролі адміністратора
- CORS: білий список origin-ів (продакшн-домени фронтенду й локальні dev-адреси) разом із cookie, без «дозволити всіх»
- Ліміт запитів на `register`, `login`, `google` і `github/exchange` (10 на хвилину з однієї IP) проти підбору паролів
- Унікальний індекс на email покупця
- SQL-ін'єкції: EF Core параметризує запити; XSS: React екранує вивід
- Секрети (БД, Firebase, GitHub) лише в змінних середовища
- Глобальний middleware повертає клієнту загальну помилку без стека, деталі лише в лог

Сесії зберігаються в пам'яті процесу, тому після перезапуску API користувачам доведеться увійти знову.

## 📚 API

Повна документація HTTP-ендпоінтів: **https://vicitmceo.github.io/NewBalanceShop/api/** (Scalar), живий Swagger: https://newbalanceshop-api.onrender.com/swagger.

| Група | Ендпоінти |
|---|---|
| Products | `GET /api/Products` (фільтри `gender`, `type`, `sale`, `categoryId`, `q`, `brand`, `minPrice`, `maxPrice`, `size`, `sort`), `GET /api/Products/{id}`; `POST /api/Products`, `PUT/DELETE /api/Products/{id}` лише для адміністратора |
| Categories | `GET /api/Categories` |
| Cart | `GET/POST /api/Cart`, `PUT/DELETE /api/Cart/{productId}?size=`, `POST /api/Cart/checkout` |
| Orders | `GET/POST /api/Orders`, `GET /api/Orders/{id}`, `GET /api/Orders/all` і `PATCH /api/Orders/{id}/status` (адміністратор) |
| Auth | `POST /api/Auth/register`, `login`, `google`, `logout`, `GET /api/Auth/me` |
| Customers | `GET /api/Customers`, `GET/PUT /api/Customers/{id}`, `GET /api/Customers/{id}/orders`, `PATCH /api/Customers/{id}/block`, `DELETE /api/Customers/{id}` |
| GitHub | `GET /api/auth/github/login`, `callback`, `me`, `POST /api/auth/github/exchange`, `POST logout` |

Документація коду (класи й методи): **https://vicitmceo.github.io/NewBalanceShop/code/**.

## 🗺️ Roadmap

- [x] Закрити ризики безпеки: ID покупця з сесії, перевірка ролі для CRUD товарів, білий список CORS, ліміт на вхід, унікальний email
- [x] Фільтри каталогу: бренд, ціна, розмір, стать, тип, розпродаж (FR-03)
- [ ] Форми входу й реєстрації та адмін-панель у React
- [ ] CI: GitHub Actions (збирання й тести)
- [ ] Логування бізнес-подій
- [ ] Сесії в Redis

## 👨‍💻 Authors

| Учасник | Роль |
|---|---|
| Панчук Вадим Владиславович | Team Lead |
| Ноцколіні Рафаїл Львович | Full-stack розробник: бекенд, фронтенд, DevOps, тести |

Команда Team 1, Jira: https://newbalance-team1.atlassian.net/jira/software/projects/SCRUM/boards/1/backlog

## 📄 License

Навчальний курсовий проєкт. Усі права залишаються за авторами; ліцензію на повторне використання не надано.
