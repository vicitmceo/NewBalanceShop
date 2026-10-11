# Архітектура

Рішення складається з чотирьох проєктів. Залежності спрямовані до `Domain`.

```
Presentation (NewBalanceShop) ──► Application ──► Domain ◄── Infrastructure
```

| Проєкт | Відповідальність |
|---|---|
| `NewBalanceShop.Domain` | Сутності (`Product`, `Category`, `Customer`, `Order`, `OrderItem`) та інтерфейси репозиторіїв. Без залежностей. |
| `NewBalanceShop.Application` | Сервіси сценаріїв (`ProductService`, `OrderService`, `AuthService`, `CustomerService`), DTO, мапери, `PasswordHasher`. |
| `NewBalanceShop.Infrastructure` | `ShopDbContext` (EF Core + PostgreSQL), репозиторії, міграції. |
| `NewBalanceShop` (Presentation) | REST-контролери, `Program.cs` з DI, CORS і сесіями. |

## Патерни

Repository, Dependency Injection, Service Layer, DTO + Mapper, Middleware Pipeline, Session State. Деталі: `defense-materials/06-patterns`.

## Безпека

Паролі зберігаються як PBKDF2-HMAC-SHA256 із сіллю (`PasswordHasher`). Вхід можливий за паролем, через Google (Firebase) і GitHub OAuth. Адмін-ендпоінти `CustomersController` перевіряють роль і повертають 401 або 403.
