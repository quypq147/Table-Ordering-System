# Backend – TableOrdering

Backend .NET cho hệ thống gọi món, tổ chức theo kiến trúc phân tầng: Api (ASP.NET Core), Application (CQRS), Domain (thuần DDD), Infrastructure (EF Core, DI, repositories).

- .NET SDK: 9.x
- Web: ASP.NET Core
- Data: EF Core (SQL Server hoặc provider khác tùy cấu hình)
- Auth: ASP.NET Core Identity + JWT (Extensions/Options trong API)

## Cấu trúc thư mục

- Solution gốc: [TableOrdering.sln](../TableOrdering.sln)
- Backend root: [backend](./)
  - API: [src/Api](src/Api)
    - Project: [src/Api/Api.csproj](src/Api/Api.csproj), launch: [src/Api/Properties/launchSettings.json](src/Api/Properties/launchSettings.json)
    - Entry: [src/Api/Program.cs](src/Api/Program.cs)
    - Config: [src/Api/appsettings.json](src/Api/appsettings.json), [src/Api/appsettings.Development.json](src/Api/appsettings.Development.json)
    - Test HTTP: [src/Api/Api.http](src/Api/Api.http)
    - Controllers: [src/Api/Controllers](src/Api/Controllers)
      - [AuthController.cs](src/Api/Controllers/AuthController.cs)
      - [CategoriesController.cs](src/Api/Controllers/CategoriesController.cs)
      - [MenuItemsController.cs](src/Api/Controllers/MenuItemsController.cs)
      - [OrdersController.cs](src/Api/Controllers/OrdersController.cs)
      - [TablesController.cs](src/Api/Controllers/TablesController.cs)
      - [UsersController.cs](src/Api/Controllers/UsersController.cs)
      - Public: [src/Api/Controllers/Public](src/Api/Controllers/Public)
    - Extensions & Options:
      - [Extensions/IdentityExtensions.cs](src/Api/Extensions/IdentityExtensions.cs)
      - [Extensions/JwtExtensions.cs](src/Api/Extensions/JwtExtensions.cs)
      - [Options/JwtOptions.cs](src/Api/Options/JwtOptions.cs)
  - Application: [src/Application](src/Application)
    - Project: [src/Application/Application.csproj](src/Application/Application.csproj)
    - DI: [src/Application/DependencyInjection.cs](src/Application/DependencyInjection.cs)
    - Abstractions: [src/Application/Abstractions](src/Application/Abstractions)
      - [IApplicationDbContext.cs](src/Application/Abstractions/IApplicationDbContext.cs)
      - [ICommand.cs](src/Application/Abstractions/ICommand.cs)
      - [ICommandHandler.cs](src/Application/Abstractions/ICommandHandler.cs)
      - [IQuery.cs](src/Application/Abstractions/IQuery.cs)
      - [IQueryHandler.cs](src/Application/Abstractions/IQueryHandler.cs)
    - DTOs: [src/Application/Dtos](src/Application/Dtos)
      - [CategoryDto.cs](src/Application/Dtos/CategoryDto.cs), [MenuItemDtos.cs](src/Application/Dtos/MenuItemDtos.cs), [OrderDto.cs](src/Application/Dtos/OrderDto.cs), [TableDtos.cs](src/Application/Dtos/TableDtos.cs)
    - Mappings: [src/Application/Mappings](src/Application/Mappings)
      - [CategoryMapper.cs](src/Application/Mappings/CategoryMapper.cs), [MenuItemMapper.cs](src/Application/Mappings/MenuItemMapper.cs), [OrderMapper.cs](src/Application/Mappings/OrderMapper.cs), [TableMapper.cs](src/Application/Mappings/TableMapper.cs)
    - Services: [src/Application/Services/ShortFriendlyOrderCodeGenerator.cs](src/Application/Services/ShortFriendlyOrderCodeGenerator.cs)
    - CQRS (Commands/Queries theo module):
      - Categories: [src/Application/Categories](src/Application/Categories)
      - MenuItems: [src/Application/MenuItems](src/Application/MenuItems)
      - Orders: [src/Application/Orders](src/Application/Orders)
      - Tables: [src/Application/Tables](src/Application/Tables)
      - Public use-cases: [src/Application/Public](src/Application/Public) (Cart, Categories, Menu, Tables)
    - Helper: [src/Application/Common/CQRS](src/Application/Common/CQRS)
    - Hợp đồng phát sinh: [src/Application/IOrderCodeGenerator.cs](src/Application/IOrderCodeGenerator.cs)
  - Domain: [src/Domain](src/Domain)
    - Project: [src/Domain/Domain.csproj](src/Domain/Domain.csproj)
    - Abstractions: [src/Domain/Abstractions](src/Domain/Abstractions)
      - [AggregateRoot.cs](src/Domain/Abstractions/AggregateRoot.cs), [Entity.cs](src/Domain/Abstractions/Entity.cs), [IDomainEvent.cs](src/Domain/Abstractions/IDomainEvent.cs), [ValueObject.cs](src/Domain/Abstractions/ValueObject.cs)
    - Entities: [src/Domain/Entities](src/Domain/Entities)
    - Enums: [src/Domain/Enums](src/Domain/Enums)
    - Events: [src/Domain/Events](src/Domain/Events)
    - Exceptions: [src/Domain/Exceptions](src/Domain/Exceptions)
    - Repositories (contracts): [src/Domain/Repositories](src/Domain/Repositories)
    - ValueObjects: [src/Domain/ValueObjects](src/Domain/ValueObjects)
  - Infrastructure: [src/Infrastructure](src/Infrastructure)
    - Project: [src/Infrastructure/Infrastructure.csproj](src/Infrastructure/Infrastructure.csproj)
    - DI: [src/Infrastructure/DependencyInjection](src/Infrastructure/DependencyInjection)
    - Identity: [src/Infrastructure/Identity](src/Infrastructure/Identity)
    - Persistence (DbContext, configurations, migrations): [src/Infrastructure/Persistence](src/Infrastructure/Persistence)
    - Repositories (EF Core): [src/Infrastructure/Repositories](src/Infrastructure/Repositories)
  - Unit tests: [UnitTests](UnitTests)
    - Project: [UnitTests/UnitTests.csproj](UnitTests/UnitTests.csproj)

## Luồng phụ thuộc

Api → Application → Domain  
Api → Infrastructure → Domain  
Application chỉ tham chiếu Domain; Infrastructure hiện thực IRepositories/DbContext và được đăng ký qua DI.

## Cấu hình & chạy

- Cấu hình kết nối DB và JWT:
  - ConnectionStrings, JWT… trong [src/Api/appsettings.json](src/Api/appsettings.json) và biến thể dev [src/Api/appsettings.Development.json](src/Api/appsettings.Development.json).
  - Mở rộng cấu hình tại [src/Api/Extensions/JwtExtensions.cs](src/Api/Extensions/JwtExtensions.cs), [src/Api/Extensions/IdentityExtensions.cs](src/Api/Extensions/IdentityExtensions.cs), options [src/Api/Options/JwtOptions.cs](src/Api/Options/JwtOptions.cs).

- Build & run API:
```sh
dotnet restore
dotnet build ../TableOrdering.sln
dotnet run --project src/Api/Api.csproj
```

- Chạy test:
```sh
dotnet test UnitTests/UnitTests.csproj
```

## Cơ sở dữ liệu (EF Core)

- Tạo migration và cập nhật DB:
```sh
# Cài dotnet-ef nếu chưa có
dotnet tool install --global dotnet-ef

# Tạo migration (output trong Infrastructure/Persistence/Migrations)
dotnet ef migrations add InitialCreate -s src/Api/Api.csproj -p src/Infrastructure/Infrastructure.csproj -o Persistence/Migrations

# Cập nhật DB
dotnet ef database update -s src/Api/Api.csproj -p src/Infrastructure/Infrastructure.csproj
```

Gợi ý:
- Đặt seeding (nếu có) trong Infrastructure và gọi khi khởi động API sau khi migrate.
- Sử dụng biến môi trường để override cấu hình prod (JWT secrets, connection string).

## API sơ lược

Các controller sẵn có:
- Auth: [AuthController](src/Api/Controllers/AuthController.cs)
- Users: [UsersController](src/Api/Controllers/UsersController.cs)
- Categories: [CategoriesController](src/Api/Controllers/CategoriesController.cs)
- Menu items: [MenuItemsController](src/Api/Controllers/MenuItemsController.cs)
- Orders: [OrdersController](src/Api/Controllers/OrdersController.cs)
- Tables: [TablesController](src/Api/Controllers/TablesController.cs)
- Public endpoints: [Controllers/Public](src/Api/Controllers/Public)

Thử nhanh bằng [Api.http](src/Api/Api.http) trong VS Code/IDE.

## Quy ước phát triển

- Domain thuần C#: không phụ thuộc framework.
- Application dùng CQRS (Commands/Queries) + DTO + Mappings; tránh truy cập EF trực tiếp.
- Infrastructure hiện thực DbContext/Repositories + DI.
- Api là composition root: đăng ký DI, middleware, auth, swagger; expose endpoints.

## Ghi chú

- Các file cấu hình môi trường dev/local đã nằm trong [.gitignore](../.gitignore).
- Xem thêm README ở root: [../README.md](../README.md).