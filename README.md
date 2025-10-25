# TableOrdering

Monorepo quản lý gọi món gồm Backend (.NET) và Clients (web). Kiến trúc phân tầng: Domain, Application (CQRS), Infrastructure (EF Core), Api (ASP.NET Core).

- Runtime: .NET 9
- ORM: EF Core
- API: ASP.NET Core + Swagger
- CI: GitHub Actions

## Cấu trúc

- Solution chính: [TableOrdering.sln](TableOrdering.sln)
- Backend: [backend/src](backend/src)
  - API: [backend/src/Api](backend/src/Api)
  - Application: [backend/src/Application](backend/src/Application)
  - Domain: [backend/src/Domain](backend/src/Domain)
  - Infrastructure: [backend/src/Infrastructure](backend/src/Infrastructure)
  - Unit tests: [backend/UnitTests](backend/UnitTests), csproj: [backend/UnitTests/UnitTests.csproj](backend/UnitTests/UnitTests.csproj)
- Clients: [clients](clients)
  - Solution: [clients/TableOrdering.Clients.sln](clients/TableOrdering.Clients.sln)
  - AdminWeb: [clients/AdminWeb](clients/AdminWeb)
  - KdsWeb: [clients/KdsWeb](clients/KdsWeb)
- CI/CD: [./github/workflows](.github/workflows)
- Git ignore: [.gitignore](.gitignore)

## Bắt đầu

Yêu cầu:
- .NET SDK 9.x
- SQL Server (hoặc DB provider khác tùy cấu hình)

Cấu hình:
- Sửa connection string trong appsettings của API tại [backend/src/Api](backend/src/Api) (file appsettings.json trong thư mục này).

Build & chạy API:
```sh
dotnet restore
dotnet build TableOrdering.sln
dotnet run --project backend/src/Api
```

Truy cập:
- Swagger: theo URL/port hiển thị trong Output (ví dụ https://localhost:5001/swagger)

Chạy test:
```sh
dotnet test backend/UnitTests/UnitTests.csproj
```

## Cơ sở dữ liệu (EF Core)

Tạo migration và cập nhật DB (đường dẫn dùng thư mục dự án):
```sh
dotnet ef migrations add InitialCreate -s backend/src/Api -p backend/src/Infrastructure -o Persistence/Migrations
dotnet ef database update -s backend/src/Api -p backend/src/Infrastructure
```

Gợi ý:
- Nếu có Seeder/HostedService, bật trong API sau khi migrate.
- Biến môi trường/secret: dùng appsettings.json và biến môi trường; các biến thể dev/local đã được ignore trong [.gitignore](.gitignore).

## Phát triển

- Mô hình phân tầng:
  - Domain: entities, value objects, rules.
  - Application: CQRS (commands/queries, handlers, DTO/mapping).
  - Infrastructure: EF Core DbContext, configurations, repositories.
  - Api: composition root, DI, endpoints, Swagger.
- Mỗi dự án có thể có file .csproj tương ứng trong thư mục của nó; dùng đường dẫn thư mục với dotnet CLI để tránh phụ thuộc tên file.

## Clients

- Mở [clients/TableOrdering.Clients.sln](clients/TableOrdering.Clients.sln) trong Visual Studio.
- Build và chạy từng dự án (AdminWeb, KdsWeb) theo hướng dẫn riêng của mỗi project.

## CI/CD

- Workflow được định nghĩa tại [.github/workflows](.github/workflows) (build/test trên pull request và main).

## Ghi chú

- Kiểm tra các file cấu hình thực tế trong [backend/src/Api](backend/src/Api), [backend/src/Infrastructure](backend/src/Infrastructure) để điều chỉnh tên migration/thư mục nếu cần.
- Đã cấu hình ignore chuẩn cho .NET/VS/EF trong [.gitignore](.gitignore).

## License

Chưa thiết lập. Thêm LICENSE theo nhu cầu dự án.