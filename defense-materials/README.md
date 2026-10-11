# Матеріали до захисту курсового проєкту — New Balance Shop (Team 1)

Скріни, схеми й інфографіка для слайдів презентації, а також сценарії доповіді й демо. Кожна підпапка відповідає пункту з умови ДЗ. Слайди мають розмір 1600×900 і готові до вставки.

| Папка | Пункт умови | Файли |
|---|---|---|
| `00-first-slide` | Перший слайд: команда, проєкт, учасники, ролі, контакти, QR | `first-slide.png` |
| `01-why-project` | Навіщо проєкт: проблема, рішення, ЦА | `problem-solution-audience.png`, `personas.png` |
| `02-srs-and-plan` | SRS та план роботи | `srs-requirements.png`, `weekly-plan.png` |
| `03-ux-research` | UI Kit, конкуренти, Empathy Map, CJM, інформаційна архітектура | `ui-kit.png`, `benchmark-newbalance.png`, `empathy-map.png`, `customer-journey-map.png`, `information-architecture.png` |
| `04-architecture` | Архітектура (фронт, бекенд, БД): діаграма класів і розміщення | `system-architecture-and-deployment.png`, `class-diagram.png`, скріни структури проєкту |
| `05-clean-architecture` | Чиста архітектура: проєкти та зв'язки | `projects-and-references.png` |
| `06-patterns` | Використані патерни | `patterns.png` |
| `07-database` | БД: СУБД, ER-діаграма, обґрунтування | `er-diagram.png`, `postgres-tables-screenshot.png` |
| `08-metrics` | Ключові метрики | `key-metrics.png` |
| `09-team-and-scrum` | Git, внесок, Scrum | `git-branches-and-contribution.png`, `scrum-sprints-and-backlog.png` |
| `10-testing-and-logging` | Testing & Logging | `testing-and-logging.png` |
| `11-tools-and-workflow` | Tools & Workflow з логотипами | `tools-and-workflow.png` |
| `12-documentation` | README, DocFX, Scalar | `documentation.png`, `scalar-api-docs.png`, `swagger-ui-render.png` |
| `13-auth-and-security` | Authentication / Authorization, паролі | `auth-and-security.png` |
| `14-portfolio-value` | Портфоліо-цінність | `portfolio-value.png` |
| `15-contacts-and-key-idea` | Контакти, QR, ключова думка | `contacts-qr.png`, `key-idea.png` |
| `16-live-ui-screenshots` | Скріни живого сайту й документації API | 5 файлів |
| `17-scripts` | Сценарій доповіді (10 хв) і демо (5 хв) | `talk-script-10min.md`, `demo-script-5min.md` |
| `NewBalanceShop-presentation.pptx` | **Готова презентація**: 15 слайдів доповіді + 3 запасні в додатку, у нотатках текст доповіді | один файл |
| `_src` | Вихідні HTML слайдів і скрипт `render.ps1` для повторного рендера | |

## Ключові цифри (виміряно 11.10.2026)

- 145 тестів (102 unit + 43 integration), всі проходять; покриття рядків 86,7%
- 12 з 13 функціональних вимог SRS реалізовано повністю
- 29 REST-ендпоінтів, 30 комітів (бекенд 24 + фронтенд 6), 3 тижні розробки

## Чого ще не вистачає (чесно)

- Скрін Jira-дошки (потрібен вхід в Atlassian-акаунт команди).
- Результати UX-дослідження: персони, Empathy Map і CJM складено командою на основі сценарію використання, а не опитування користувачів.
- DocFX на GitHub Pages не налаштовано; README.md застарів (згадує SQL Server і лише ендпоінти товарів).
- Внесок Team Lead у Git не видно (0 комітів): доповнити слайд `09-team-and-scrum` вручну.
- Немає CI (GitHub Actions) і окремого логування бізнес-подій.
- Форм логіну/реєстрації та адмін-панелі у React немає, демо йде через Swagger.
- Ризики безпеки зі слайда `13-auth-and-security` відкриті (BOLA, CRUD товарів без ролі, CORS, публічний пароль сід-адміна).

## Як перерендерити слайди

```powershell
.\_src\render.ps1            # усі слайди
.\_src\render.ps1 -Filter '09*'   # лише за маскою
```

Потрібен Google Chrome (headless). QR-коди й іконки на слайдах 00, 11, 15 підтягуються з CDN, тому потрібен інтернет.
