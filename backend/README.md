# Backend – TableOrdering (.NET 10)

Backend .NET 10 cho hệ thống gọi món: API (ASP.NET Core), Application (CQRS), Domain (DDD), Infrastructure (EF Core, Identity, SignalR).

## Dự án

- Api: src/Api (Program.cs, Controllers, JWT, SignalR)
- Application: src/Application (Commands/Queries, DTO, Mapping, Services, Abstractions)
- Domain: src/Domain (Entities, ValueObjects, Events, Repositories contracts)
- Infrastructure: src/Infrastructure (DbContext, EF configs, Repositories, Identity, SignalR adapters)
- Tests: UnitTests (KDS handlers,…)

## Chạy

```sh
dotnet restore
dotnet build ../TableOrdering.sln
dotnet run --project src/Api/Api.csproj
```

Swagger hiển thị ở port cấu hình (launchSettings).  

Test:
```sh
dotnet test UnitTests/UnitTests.csproj
```

## DB (EF Core)

```sh
dotnet tool install --global dotnet-ef

dotnet ef migrations add InitialCreate -s src/Api/Api.csproj -p src/Infrastructure/Infrastructure.csproj -o Persistence/Migrations
dotnet ef database update -s src/Api/Api.csproj -p src/Infrastructure/Infrastructure.csproj
```

## JWT / Identity

- Cấu hình: appsettings*.json (JWT section)
- Extensions: src/Api/Extensions/JwtExtensions.cs, IdentityExtensions.cs
- Options: src/Api/Options/JwtOptions.cs

## SignalR (KDS)

- Hub: src/Api/Hubs/KdsHub.cs
- DI tiện ích: src/Api/DependencyInjection/KdsServiceCollectionExtensions.cs
- Domain events → Notifier: hạ tầng triển khai trong Infrastructure/SignalR

## Domain Events

Dispatcher & Handler abstractions: Application/Abstractions. Infrastructure thực thi publish (tuỳ chỉnh).

## Nâng cấp lên .NET 10

- TargetFramework: net10.0 trong *.csproj
- Kiểm tra package cập nhật (EF Core, ASP.NET Core)
- Rebuild & chạy test

## Quy ước

- Không logic nghiệp vụ trong Controller: dùng Commands/Queries.
- ValueObject bảo toàn bất biến (tiền tệ, số lượng…).
- UnitOfWork qua DbContext + SaveChangesAsync.

## Bảo mật

- Luôn đặt JWT secret bằng biến môi trường khi production.
- Thêm rate limit nếu mở public endpoints.

## TODO gợi ý

- Thêm seeding chuẩn (HostedService)
- Thêm Observability (OpenTelemetry)
- Thêm Integration Tests