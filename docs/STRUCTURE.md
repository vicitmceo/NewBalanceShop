# New Balance Shop — Architecture

Формат за зразком викладача ([cinema hikes p43 architecture](https://gist.github.com/sunmeat/64abb14b88aca398afeacbd4354dab7a)).

> **Примітка:** поточна реалізація (тиждень наздоганяючого спринту) — спрощений однопроєктний варіант з папками
> `Domain/Application/Infrastructure/Presentation` (дозволено умовою завдання: "поки що можна БЕЗ розділу на 3-4
> проєкти в солюшені"). Дерево нижче — **цільова** структура, до якої проєкт мігрує на наступних тижнях, коли
> з'явиться реєстрація користувачів, кошик/checkout і React-фронтенд.

## ⚙️ Backend

```
NewBalanceShop.sln
│
├── src/
│   ├── NewBalanceShop.Domain/
│   │   ├── Entities/
│   │   │   ├── Catalog/
│   │   │   │   ├── Product.cs
│   │   │   │   ├── Category.cs
│   │   │   │   └── ProductVariant.cs        # Size, Color, Stock — окрема таблиця варіантів товару
│   │   │   │
│   │   │   ├── Users/
│   │   │   │   └── Customer.cs
│   │   │   │
│   │   │   └── Sales/
│   │   │       ├── Cart.cs
│   │   │       ├── CartItem.cs
│   │   │       ├── Order.cs
│   │   │       └── OrderItem.cs
│   │   │
│   │   ├── Enums/
│   │   │   └── OrderStatus.cs               # New, Confirmed, Shipped, Delivered, Cancelled
│   │   │
│   │   ├── Interfaces/                      # Порти (реалізуються в Infrastructure)
│   │   │   ├── Repositories/
│   │   │   │   ├── IRepository.cs           # generic<T>
│   │   │   │   ├── IProductRepository.cs
│   │   │   │   ├── ICategoryRepository.cs
│   │   │   │   ├── IOrderRepository.cs
│   │   │   │   └── ICartRepository.cs
│   │   │   └── IUnitOfWork.cs
│   │   │
│   │   └── Specifications/
│   │       ├── ProductByCategorySpecification.cs
│   │       └── ProductPriceRangeSpecification.cs
│   │
│   ├── NewBalanceShop.Application/
│   │   ├── Services/
│   │   │   ├── ProductService.cs            # FR-01…FR-06
│   │   │   ├── CategoryService.cs
│   │   │   ├── CartService.cs               # FR-07
│   │   │   ├── OrderService.cs              # FR-08
│   │   │   └── AuthService.cs               # реєстрація/логін покупця
│   │   │
│   │   ├── DTOs/
│   │   │   ├── Catalog/
│   │   │   │   ├── ProductDto.cs
│   │   │   │   └── CategoryDto.cs
│   │   │   ├── Sales/
│   │   │   │   ├── CartItemDto.cs
│   │   │   │   ├── OrderDto.cs
│   │   │   │   └── CreateOrderRequestDto.cs
│   │   │   └── Auth/
│   │   │       ├── RegisterRequestDto.cs
│   │   │       └── LoginRequestDto.cs
│   │   │
│   │   ├── Validators/
│   │   │   └── ProductCreateValidator.cs
│   │   │
│   │   └── Mappings/
│   │       └── ProductMapper.cs
│   │
│   ├── NewBalanceShop.Infrastructure/
│   │   ├── DependencyInjection.cs           # AddInfrastructure(IServiceCollection)
│   │   ├── Persistence/
│   │   │   ├── ShopDbContext.cs
│   │   │   ├── EntityTypeConfigurations/
│   │   │   │   ├── ProductEntityTypeConfiguration.cs
│   │   │   │   └── OrderEntityTypeConfiguration.cs
│   │   │   └── Migrations/
│   │   ├── Identity/
│   │   │   └── AppUser.cs                   # : IdentityUser<int> (наступний спринт)
│   │   └── Repositories/
│   │       ├── Repository.cs
│   │       ├── ProductRepository.cs
│   │       ├── CategoryRepository.cs
│   │       ├── OrderRepository.cs
│   │       └── UnitOfWork.cs
│   │
│   └── NewBalanceShop.WebApi/
│       ├── Controllers/
│       │   ├── ProductsController.cs
│       │   ├── CategoriesController.cs
│       │   ├── CartController.cs
│       │   ├── OrdersController.cs
│       │   ├── AuthController.cs
│       │   └── Admin/
│       │       └── AdminProductsController.cs
│       ├── Middleware/
│       │   └── ExceptionHandlingMiddleware.cs
│       └── Program.cs
│
└── tests/
    ├── NewBalanceShop.Domain.UnitTests/
    └── NewBalanceShop.Application.UnitTests/
```

**Поточна реалізація** (спрощена, один проєкт `NewBalanceShop.csproj`):

```
NewBalanceShop/
├── Domain/Entities/          # Product, Category, Customer, Order, OrderItem
├── Domain/Interfaces/         # IRepository<T>, IProductRepository, IOrderRepository
├── Application/Services/      # ProductService (реалізує IProductService)
├── Application/DTO/           # ProductDto
├── Infrastructure/Data/        # ShopDbContext
├── Infrastructure/Repositories/# Repository<T>, ProductRepository, OrderRepository
├── Presentation/Controllers/   # ProductsController (CRUD)
└── Program.cs
```

## 📃 Frontend (заплановано)

```
new-balance-shop-frontend/
│
├── public/
│   └── assets/
│
├── src/
│   ├── app/
│   │   ├── providers/
│   │   │   ├── AuthProvider.tsx
│   │   │   └── QueryProvider.tsx           # @tanstack/react-query
│   │   ├── router/
│   │   │   ├── index.tsx
│   │   │   └── ProtectedRoute.tsx
│   │   ├── store/
│   │   │   └── useCartStore.ts             # Zustand: кошик покупця
│   │   └── App.tsx
│   │
│   ├── layouts/
│   │   ├── MainLayout.tsx
│   │   └── AdminLayout.tsx
│   │
│   ├── pages/
│   │   ├── public/
│   │   │   ├── CatalogPage.tsx             # Грід товарів з фільтрацією (FR-01, FR-03)
│   │   │   └── ProductPage.tsx             # Картка товару (FR-02)
│   │   ├── auth/
│   │   │   ├── LoginPage.tsx
│   │   │   └── RegisterPage.tsx
│   │   ├── private/
│   │   │   ├── CartPage.tsx
│   │   │   └── OrdersPage.tsx
│   │   └── admin/
│   │       └── ProductsMgmtPage.tsx        # CRUD товарів
│   │
│   ├── features/
│   │   ├── catalog/
│   │   │   ├── api/                        # fetchProducts, fetchCategories
│   │   │   └── components/                 # ProductFilterBar, ProductGrid
│   │   ├── cart/
│   │   │   ├── api/
│   │   │   └── components/                 # CartItemRow, CartSummary
│   │   ├── checkout/
│   │   │   ├── api/                        # createOrder
│   │   │   └── components/                 # CheckoutForm
│   │   └── admin/
│   │       ├── api/
│   │       └── components/                 # ProductsTable, ProductEditModal
│   │
│   ├── shared/
│   │   ├── api/
│   │   │   └── axiosClient.ts
│   │   ├── types/
│   │   │   └── dtos.ts                     # копії ProductDto/OrderDto з C#
│   │   └── ui/
│   │       └── ProductCard/
│   │
│   └── main.tsx
│
├── .env
├── package.json
└── vite.config.ts
```

## 🗄️ Database

```
NewBalanceShop Database (SQL Server)
│
├── Identity/                                # заплановано (наступний спринт)
│   └── AspNetUsers
│       ├── Id            int         [PK]
│       ├── Email         varchar256  UNIQUE
│       ├── FullName      varchar256
│       └── PasswordHash  text
│
├── Catalog/
│   ├── Categories
│   │   ├── Id      int         [PK]
│   │   └── Name    varchar100  UNIQUE
│   │
│   └── Products
│       ├── Id           int             [PK]
│       ├── Name         varchar150
│       ├── Brand        varchar80       DEFAULT 'New Balance'
│       ├── CategoryId   int             → FK Categories
│       ├── Size         varchar20
│       ├── Color        varchar40
│       ├── Price        decimal(10,2)
│       ├── Stock        int
│       ├── ImageUrl     varchar300
│       └── Description  varchar1000
│
└── Sales/
    ├── Orders
    │   ├── Id           int           [PK]
    │   ├── CustomerId   int           → FK AspNetUsers
    │   ├── CreatedAt    timestamptz
    │   ├── Status       enum (OrderStatus)
    │   └── TotalPrice   decimal(10,2)
    │
    └── OrderItems
        ├── Id         int             [PK]
        ├── OrderId    int             → FK Orders
        ├── ProductId  int             → FK Products
        ├── Quantity   int
        └── UnitPrice  decimal(10,2)
```

**Поточна реалізація:** таблиці `Categories`, `Products`, `Customers`, `Orders`, `OrderItems` уже створюються через
EF Core міграцію (`NewBalanceShop/Migrations/InitialCreate`) — без окремої таблиці `AspNetUsers`/Identity, замість
неї спрощена `Customers` (буде замінено на ASP.NET Core Identity в наступному спринті).
