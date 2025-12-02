# TableOrdering

Monorepo hệ thống gọi món gồm Backend (.NET) và Clients (.NET, MVC/Web, MAUI). Kiến trúc: Domain (DDD), Application (CQRS), Infrastructure (EF Core), Api (ASP.NET Core). Đã nâng cấp .NET 10.

- Runtime: .NET 10 (SDK 10.x)
- Data: EF Core
- API: ASP.NET Core + Swagger
- Auth: Identity + JWT
- Realtime: SignalR (KDS)
- CI: GitHub Actions

## Cấu trúc tổng quan

- Solution chính: TableOrdering.sln
- Backend: backend/
  - Api: backend/src/Api
  - Application: backend/src/Application
  - Domain: backend/src/Domain
  - Infrastructure: backend/src/Infrastructure
  - Unit tests: backend/UnitTests
- Clients: clients/
  - AdminWeb: ASP.NET Core MVC
  - CustomerWeb: ASP.NET Core MVC (khách đặt món)
  - KdsWeb: ASP.NET Core MVC (Kitchen Display System)
  - WaiterApp: .NET MAUI (phục vụ, di động)
  - Contracts chung: clients/TableOrdering.Contracts
- CI/CD: .github/workflows
- README chi tiết: backend/README.md, clients/README.md

## Yêu cầu

- .NET 10 SDK
- SQL Server (hoặc chỉnh provider)
- Node/npm (nếu mở rộng asset front-end)
- Git + EF Tools: dotnet tool install --global dotnet-ef

## Thiết lập nhanh

1. Cập nhật ConnectionStrings trong backend/src/Api/appsettings.Development.json.
2. Tạo migration:
```sh
dotnet ef migrations add InitialCreate -s backend/src/Api -p backend/src/Infrastructure -o Persistence/Migrations
dotnet ef database update -s backend/src/Api -p backend/src/Infrastructure
```
3. Chạy API:
```sh
dotnet run --project backend/src/Api
```
4. Chạy một client (ví dụ AdminWeb):
```sh
dotnet run --project clients/AdminWeb/AdminWeb.csproj
```

## Test

```sh
dotnet test backend/UnitTests/UnitTests.csproj
```

## Nâng cấp .NET 10 (nếu từ 9)

- Sửa TargetFramework trong *.csproj: net10.0
- dotnet restore
- Kiểm tra breaking changes ASP.NET Core/EF Core (nếu có)
- Regenerate EF migrations nếu cần mapping mới

## Kiến trúc lớp

Api → Application → Domain  
Api → Infrastructure → Domain  
Application không truy cập EF trực tiếp; tất cả persistence qua Infrastructure.

## Bảo mật

- Đăng ký JWT trong Api Extensions (JwtExtensions.cs)
- Lưu secret bằng biến môi trường khi deploy (JWT signing key, connection string)

## Realtime (KDS)

- Hub: backend/src/Api/Hubs/KdsHub.cs
- Notifier: Application abstractions IKitchenTicketNotifier + triển khai trong Infrastructure/SignalR

## Clients

- Mỗi app có appsettings.* để cấu hình BackendBaseUrl
- WaiterApp dùng MAUI (đa nền tảng)

## CI

- Workflow: .github/workflows (build + test)
- Thêm bước publish container tùy nhu cầu

## License

Chưa thiết lập (thêm LICENSE nếu cần).