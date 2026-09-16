# Архітектура системи — New Balance Shop

Проєктування виконано за планом підготовки до захисту: https://gist.github.com/sunmeat/fd6d78db7e5298d3c1ee6378f4880a4d — архітектура враховує вимоги [SRS.md](../SRS.md), нижче наведено структуру backend/frontend/БД (скріни) та UML-діаграму класів з обґрунтуванням.

## 0. Скріни структури проєкту

**Backend** (реалізовано):

![Backend structure](architecture/structure-backend.png)

**Frontend** (заплановано, наступна ітерація):

![Frontend structure](architecture/structure-frontend.png)

**База даних** (SQL Server, EF Core Migrations):

![Database schema](architecture/structure-database.png)

## 1. Шари Clean Architecture

- **Presentation** (`Presentation/Controllers/`, `Program.cs`) — HTTP-шар, ApiController повертає JSON.
- **Application** (`Application/`) — сервіси (`ProductService`), DTO (`ProductDto`), мапери (`ProductMapper`).
- **Domain** (`Domain/`) — сутності (`Product`, `Category`, `Customer`, `Order`, `OrderItem`) та контракти репозиторіїв (`IRepository<T>`, `IProductRepository`, `IOrderRepository`) — без залежностей від інших шарів.
- **Infrastructure** (`Infrastructure/`) — `ShopDbContext` (EF Core → SQL Server), `Repository<T>` та похідні (`ProductRepository`, `OrderRepository`).

Напрямок залежностей: `Presentation → Application → Domain ← Infrastructure`.
Domain нічого не знає про EF Core чи ASP.NET Core — залежності інвертовано через інтерфейси репозиторіїв.

## 2. UML-діаграма класів

```mermaid
classDiagram
    direction LR

    class ProductsController {
        -IProductService productService
        +GetAll(categoryId) ActionResult
        +GetById(id) ActionResult
        +Create(dto) ActionResult
        +Update(id, dto) IActionResult
        +Delete(id) IActionResult
    }

    class IProductService {
        <<interface>>
        +GetAllAsync() List~ProductDto~
        +GetByCategoryAsync(categoryId) List~ProductDto~
        +GetByIdAsync(id) ProductDto
        +CreateAsync(dto) ProductDto
        +UpdateAsync(id, dto) bool
        +DeleteAsync(id) bool
    }

    class ProductService {
        -IProductRepository productRepository
        +GetAllAsync() List~ProductDto~
        +CreateAsync(dto) ProductDto
        +UpdateAsync(id, dto) bool
        +DeleteAsync(id) bool
    }

    class ProductDto {
        +int Id
        +string Name
        +string Brand
        +int CategoryId
        +decimal Price
        +int Stock
    }

    class IRepository~T~ {
        <<interface>>
        +GetAllAsync() List~T~
        +GetByIdAsync(id) T
        +AddAsync(entity) void
        +Update(entity) void
        +Remove(entity) void
        +SaveChangesAsync() void
    }

    class IProductRepository {
        <<interface>>
        +GetByCategoryAsync(categoryId) List~Product~
    }

    class Repository~T~ {
        #ShopDbContext Db
        +GetAllAsync() List~T~
        +AddAsync(entity) void
        +SaveChangesAsync() void
    }

    class ProductRepository {
        +GetByCategoryAsync(categoryId) List~Product~
    }

    class ShopDbContext {
        +DbSet~Product~ Products
        +DbSet~Category~ Categories
        +DbSet~Order~ Orders
    }

    class Product {
        +int Id
        +string Name
        +string Brand
        +int CategoryId
        +decimal Price
        +int Stock
    }

    class Order {
        +int Id
        +int CustomerId
        +OrderStatus Status
        +decimal TotalPrice
    }

    ProductsController --> IProductService : DI
    ProductService ..|> IProductService : реалізує
    ProductService --> IProductRepository : CRUD товарів
    ProductService --> ProductDto : повертає

    IProductRepository --|> IRepository~T~ : розширює
    ProductRepository ..|> IProductRepository : реалізує
    ProductRepository --|> Repository~T~ : успадковує
    Repository~T~ --> ShopDbContext : звертається до БД

    ShopDbContext "1" --> "*" Product : DbSet
    ShopDbContext "1" --> "*" Order : DbSet
```

## 3. Чому саме така структура

- **Repository pattern**: `ProductService` не знає про EF Core — працює лише з `IProductRepository`. Якщо БД зміниться (наприклад, на PostgreSQL чи іншу ORM), достатньо переписати `ProductRepository`, сервіс і контролер лишаться незмінними.
- **DTO замість прямої серіалізації Domain-сутностей**: `ProductDto` — явний контракт API, захищає доменну модель від випадкових змін формату відповіді.
- **Generic `IRepository<T>` + специфічні інтерфейси**: `IProductRepository`/`IOrderRepository` розширюють спільний контракт власними методами (`GetByCategoryAsync`, `GetByCustomerAsync`), уникаючи дублювання базових CRUD-операцій.
