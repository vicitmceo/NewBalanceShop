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
| `17-scripts` | Доповідь 5 хв (Рафаїл, відкриває Вадим), демо 5 хв (читає Вадим, мишею керує Рафаїл), питання й відповіді; російською | `talk-script-10min.md`, `demo-script-5min.md` |
| `NewBalanceShop-presentation.pptx` | **Готова презентація**: 8 слайдів доповіді (5 хв) + 19 запасних + фінальний «Ваші питання?»; посилання клікабельні в додатку, у нотатках текст доповіді | один файл |
| `_src` | Вихідні HTML слайдів і скрипт `render.ps1` для повторного рендера | |

## Ключові цифри (виміряно 19.10.2026)

- 295 тестів (190 unit + 105 integration), всі проходять; покриття рядків 93,4%
- 13 з 13 функціональних вимог SRS реалізовано
- 34 REST-ендпоїнти, 114 комітів (бекенд 71 + фронтенд 43), 5 тижнів розробки
- GitHub Actions CI в обох репозиторіях (зелений); OWASP ZAP baseline: 0 High, 4 Medium, 4 Low, заголовки додано
- Jira: 8 спринтів по одному тижню, 44 задачі, 33 виконано
- 70 товарів, UA/EN/FR, вхід: пароль і Google

## Чесні обмеження

- Внесок Team Lead (Панчук Вадим) у Git не видно (0 комітів), його роль: планування, Jira, документи.
- Безкоштовна БД Render діє до ~18.11.2026 (міграція запланована); сесії в пам'яті процесу.
- Фото й назви товарів з newbalance.com: навчальний макет, не афілійований з брендом; оплати немає.
- Статуси в Jira відстають від коду; персони, Empathy Map і CJM складено без опитування користувачів.

## Сценарії

`17-scripts/talk-script-10min.md`, `17-scripts/demo-script-5min.md`, `17-scripts/qa-teacher-questions.md`.

## Як перерендерити слайди

```powershell
.\_src\render.ps1            # усі слайди
.\_src\render.ps1 -Filter '09*'   # лише за маскою
```

Потрібен Google Chrome (headless). QR-коди й іконки на слайдах 00, 11, 15 підтягуються з CDN, тому потрібен інтернет.
