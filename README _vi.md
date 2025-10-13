# TableOrdering

Hệ thống quản lý gọi món theo kiến trúc phân tầng: Domain, Application (CQRS), Infrastructure (EF Core), Api (ASP.NET Core).

- Runtime: .NET 9
- ORM: EF Core (SQL Server)
- API: ASP.NET Core + Swagger
- Kiến trúc: CQRS nhẹ với Sender/Handlers

## Cấu trúc solution

- [TableOrdering.sln](TableOrdering.sln)
- API: [Api](Api)
  - Startup: [Api/Program.cs](Api/Program.cs)
  - Cấu hình: [Api/appsettings.json](Api/appsettings.json)
  - Swagger/OpenAPI (tự bật ở Development)
  - Health check DB: GET /health/db
- Application (CQRS, DTO, Mappings):
  - Abstractions: [`Application.Abstractions.ICommand`](Application/Abstractions/ICommand.cs), [`Application.Abstractions.IQuery`](Application/Abstractions/IQuery.cs), [`Application.Abstractions.ICommandHandler`](Application/Abstractions/ICommandHandler.cs), [`Application.Abstractions.IQueryHandler`](Application/Abstractions/IQueryHandler.cs), [`Application.Abstractions.IApplicationDbContext`](Application/Abstractions/IApplicationDbContext.cs)
  - Dtos: [Application/Dtos](Application/Dtos), ví dụ [`Application.Dtos.OrderDto`](Application/Dtos/OrderDto.cs)
  - Mappings: [`Application.Mappings.OrderMapper`](Application/Mappings/OrderMapper.cs)
  - Orders use-cases:
    - Start: [`Application.Orders.Commands.StartOrderCommand`](Application/Orders/Commands/StartOrderCommand.cs)
    - Add item: [`Application.Orders.Commands.AddItemCommand`](Application/Orders/Commands/AddItemCommand.cs)
    - Submit: [`Application.Orders.Commands.SubmitOrderCommand`](Application/Orders/Commands/SubmitOrderCommand.cs)
    - Pay: [`Application.Orders.Commands.PayOrderCommand`](Application/Orders/Commands/PayOrderCommand.cs)
  - DI: [Application/DependencyInjection.cs](Application/DependencyInjection.cs)
- Domain (Entities, ValueObjects, Events, Enums):
  - Entities:
    - [`Domain.Entities.Order`](Domain/Entities/Order.cs) + [`Domain.Entities.OrderItem`](Domain/Entities/OrderItem.cs)
    - [`Domain.Entities.MenuItem`](Domain/Entities/MenuItem.cs)
    - [`Domain.Entities.RestaurantTable`](Domain/Entities/RestaurantTable.cs)
    - [`Domain.Entities.Voucher`](Domain/Entities/Voucher.cs)
  - Value objects:
    - [`Domain.ValueObjects.Money`](Domain/ValueObjects/Money.cs)
    - [`Domain.ValueObjects.Quantity`](Domain/ValueObjects/Quantity.cs)
  - Enums:
    - [`Domain.Enums.OrderStatus`](Domain/Enums/OrderStatus.cs), [`Domain.Enums.TableStatus`](Domain/Enums/TableStatus.cs), [`Domain.Enums.PaymentMethod`](Domain/Enums/PaymentMethod.cs), [`Domain.Enums.DiscountType`](Domain/Enums/DiscountType.cs)
  - Events:
    - [`Domain.Events.OrderPlaced`](Domain/Events/OrderPlaced.cs), [`Domain.Events.OrderSubmitted`](Domain/Events/OrderSubmitted.cs), [`Domain.Events.OrderPaid`](Domain/Events/OrderPaid.cs)
  - Abstractions:
    - [`Domain.Abstractions.AggregateRoot<TId>`](Domain/Abstractions/AggregateRoot.cs), [`Domain.Abstractions.Entity<TId>`](Domain/Abstractions/Entity.cs), [`Domain.Abstractions.IDomainEvent`](Domain/Abstractions/IDomainEvent.cs)
  - Repositories contracts:
    - [`Domain.Repositories.IOrderRepository`](Domain/Repositories/IOrderRepository.cs), [`Domain.Repositories.IMenuItemRepository`](Domain/Repositories/IMenuItemRepository.cs), [`Domain.Repositories.ITableRepository`](Domain/Repositories/ITableRepository.cs), [`Domain.Repositories.IUnitOfWork`](Domain/Repositories/IUnitOfWork.cs)
- Infrastructure (EF Core, Repositories, DbContext):
  - DbContext: [`Infrastructure.Persistence.TableOrderingDbContext`](Infrastructure/Persistence/TableOrderingDbContext.cs)
  - Configurations: 
    - [`OrderConfiguration`](Infrastructure/Persistence/Configurations/OrderConfiguration.cs), [`MenuItemConfiguration`](Infrastructure/Persistence/Configurations/MenuItemConfiguration.cs), [`RestaurantTableConfiguration`](Infrastructure/Persistence/Configurations/RestaurantTableConfiguration.cs), [`VoucherConfiguration`](Infrastructure/Persistence/Configurations/VoucherConfiguration.cs)
  - Repositories:
    - [`Infrastructure.Repositories.OrderRepository`](Infrastructure/Repositories/OrderRepository.cs), [`Infrastructure.Repositories.MenuItemRepository`](Infrastructure/Repositories/MenuItemRepository.cs), [`Infrastructure.Repositories.TableRepository`](Infrastructure/Repositories/TableRepository.cs), [`Infrastructure.Repositories.UnitOfWork`](Infrastructure/Repositories/UnitOfWork.cs)
  - DI:
    - Được API dùng: [`Infrastructure.DependencyInjection.ServiceCollectionExtensions.AddInfrastructure`](Infrastructure/DependencyInjection/ServiceCollectionExtensions.cs)
    - Tùy chọn (có Seeder hosted service): [`Infrastructure.DependencyInjection`](Infrastructure/DependencyInjection.cs)
  - Seeder: [`Infrastructure.Persistence.DbSeeder`](Infrastructure/Persistence/DbSeeder.cs)
- UnitTests: [UnitTests](UnitTests) (khởi tạo, chưa có test)
- Git ignore: [.gitignore](.gitignore)

## Domain chính

- Order lifecycle: Draft → Submitted → InProgress → Ready → Served → Paid → Cancelled
  - Submit: [`Domain.Entities.Order.Submit`](Domain/Entities/Order.cs)
  - Tiến trình: [`Order.MarkInProgress`](Domain/Entities/Order.cs), [`Order.MarkReady`](Domain/Entities/Order.cs), [`Order.MarkServed`](Domain/Entities/Order.cs)
  - Thanh toán: [`Order.Pay(Money amount, string method)`](Domain/Entities/Order.cs) kiểm tra trạng thái (Submitted/Ready/Served), kiểm tra tiền tệ và số tiền phải đúng bằng Total, raise [`Domain.Events.OrderPaid`](Domain/Events/OrderPaid.cs)
  - Hủy: [`Order.Cancel`](Domain/Entities/Order.cs) cấm khi đã Paid
- Tính tổng: [`Order.Total`](Domain/Entities/Order.cs) cộng LineTotal của Items theo currency đồng nhất
- Dòng món: [`Domain.Entities.OrderItem`](Domain/Entities/OrderItem.cs) có Id (int) để EF OwnsMany, `LineTotal = UnitPrice * Quantity`, đổi số lượng qua `ChangeQuantity`
- Value objects:
  - [`Money`](Domain/ValueObjects/Money.cs): đảm bảo currency thống nhất khi so sánh/cộng trừ, làm tròn 2 chữ số
  - [`Quantity`](Domain/ValueObjects/Quantity.cs): số lượng > 0

## Application (CQRS)

Các use-case được triển khai qua Command + Handler:
- Bắt đầu đơn: [`Application.Orders.Commands.StartOrderCommand`](Application/Orders/Commands/StartOrderCommand.cs)
- Thêm món: [`Application.Orders.Commands.AddItemCommand`](Application/Orders/Commands/AddItemCommand.cs)
- Xác nhận đơn: [`Application.Orders.Commands.SubmitOrderCommand`](Application/Orders/Commands/SubmitOrderCommand.cs)
- Thanh toán: [`Application.Orders.Commands.PayOrderCommand`](Application/Orders/Commands/PayOrderCommand.cs) truyền đủ `Amount/Currency/Method` vào domain `Order.Pay`

Mapping sang DTO: [`Application.Mappings.OrderMapper`](Application/Mappings/OrderMapper.cs)

Đăng ký CQRS/Sender: [Application/DependencyInjection.cs](Application/DependencyInjection.cs)

## Persistence

- DbContext: [`TableOrderingDbContext`](Infrastructure/Persistence/TableOrderingDbContext.cs) chứa DbSet cho Orders/MenuItems/Tables/Vouchers
- Mappings/Conversion:
  - OwnsMany OrderItems trong [`OrderConfiguration`](Infrastructure/Persistence/Configurations/OrderConfiguration.cs)
  - `Money` → decimal(18,2) và `Quantity` → int
- Repositories + UoW: xem [`Infrastructure.Repositories`](Infrastructure/Repositories)

Seeder mẫu: [`DbSeeder`](Infrastructure/Persistence/DbSeeder.cs) thêm MenuItems/Tables. Lưu ý:
- Nhánh DI mà API đang dùng ([`Infrastructure.DependencyInjection.ServiceCollectionExtensions`](Infrastructure/DependencyInjection/ServiceCollectionExtensions.cs)) chưa tự chạy seeding.
- Có biến thể DI khác có HostedService seeding trong [`Infrastructure/DependencyInjection.cs`](Infrastructure/DependencyInjection.cs). Muốn bật seeding tự động, có thể:
  - Chuyển using trong API sang `using Infrastructure;` và gọi `AddInfrastructure(builder.Configuration);` (biến thể này đăng ký `SeedHostedService`)
  - Hoặc tự gọi `DbSeeder.SeedAsync` sau khi migrate.

API hiện chạy migrate startup tại [Api/Program.cs](Api/Program.cs).

## Thiết lập & chạy

Yêu cầu:
- .NET SDK 9
- SQL Server chạy local (hoặc chỉnh lại connection string)

1) Cấu hình DB
- Mở [Api/appsettings.json](Api/appsettings.json) và chỉnh "ConnectionStrings:DefaultConnection" cho phù hợp.

2) Tạo migration và cập nhật DB
- Nếu chưa có migration, tạo và apply (đặt migration ở Infrastructure):
```sh
dotnet ef migrations add InitialCreate -s Api/Api.csproj -p Infrastructure/Infrastructure.csproj -o Persistence/Migrations
dotnet ef database update -s Api/Api.csproj -p Infrastructure/Infrastructure.csproj
```

3) Build & run
```sh
dotnet restore
dotnet build
dotnet run --project Api/Api.csproj
```

4) Truy cập
- Swagger: https://localhost:5001/swagger (port thực tế theo output)
- Health DB: GET /health/db

## API Controllers

Hiện các controller mẫu trong [Api/Controller](Api/Controller) là stub (trả về View). Bạn có thể triển khai Web API endpoints gọi CQRS `ISender` để dùng các use-case trong Application.

## Ghi chú

- Phân hệ MenuItem/Table đã có Repository và EF config; thêm endpoints theo nhu cầu
- UnitTests project đã có sẵn, cần bổ sung test cho Domain/Application
- `.gitignore` đã cấu hình cho .NET/VS/EF: xem [.gitignore](.gitignore)

## License

MIT (tùy chỉnh theo nhu cầu dự án)