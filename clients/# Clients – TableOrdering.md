# Clients – TableOrdering

Bộ ứng dụng web MVC (.NET) cho hệ thống gọi món:
- AdminWeb: quản trị danh mục, món, bàn, người dùng, đơn hàng.
- CustomerWeb: giao diện khách đặt món, giỏ hàng, trình bày menu công khai.
- KdsWeb: màn hình bếp hiển thị/trạng thái đơn.

Yêu cầu:
- .NET SDK 9.x
- Backend API đang chạy (xem tại [backend](../backend))

## Cấu trúc

- Solution: [clients/TableOrdering.Clients.sln](TableOrdering.Clients.sln)

- AdminWeb: [clients/AdminWeb](AdminWeb)
  - Entry: [AdminWeb/Program.cs](AdminWeb/Program.cs)
  - Cấu hình: [AdminWeb/appsettings.json](AdminWeb/appsettings.json), [AdminWeb/appsettings.Development.json](AdminWeb/appsettings.Development.json)
  - Khởi chạy: [AdminWeb/Properties/launchSettings.json](AdminWeb/Properties/launchSettings.json)
  - Controllers: [AdminWeb/Controllers](AdminWeb/Controllers)
    - [AccountController.cs](AdminWeb/Controllers/AccountController.cs), [CategoriesController.cs](AdminWeb/Controllers/CategoriesController.cs), [DiagnosticsController.cs](AdminWeb/Controllers/DiagnosticsController.cs),
      [HomeController.cs](AdminWeb/Controllers/HomeController.cs), [MenuController.cs](AdminWeb/Controllers/MenuController.cs),
      [OrdersController.cs](AdminWeb/Controllers/OrdersController.cs), [TablesController.cs](AdminWeb/Controllers/TablesController.cs),
      [UsersController.cs](AdminWeb/Controllers/UsersController.cs)
  - Services: [AdminWeb/Services](AdminWeb/Services)
    - [IBackendApiClient.cs](AdminWeb/Services/IBackendApiClient.cs), [BackendApiClient.cs](AdminWeb/Services/BackendApiClient.cs)
  - Views: [AdminWeb/Views](AdminWeb/Views)
    - Layout & shared: [_ViewImports.cshtml](AdminWeb/Views/_ViewImports.cshtml), [_ViewStart.cshtml](AdminWeb/Views/_ViewStart.cshtml), [Shared](AdminWeb/Views/Shared)
    - Khu vực: [Account](AdminWeb/Views/Account), [Categories](AdminWeb/Views/Categories), [Home](AdminWeb/Views/Home), [Menu](AdminWeb/Views/Menu),
      [Orders](AdminWeb/Views/Orders), [Tables](AdminWeb/Views/Tables), [Users](AdminWeb/Views/Users)
  - Tài sản tĩnh: [AdminWeb/wwwroot](AdminWeb/wwwroot)

- CustomerWeb: [clients/CustomerWeb](CustomerWeb)
  - Entry: [CustomerWeb/Program.cs](CustomerWeb/Program.cs)
  - Cấu hình: [CustomerWeb/appsettings.json](CustomerWeb/appsettings.json), [CustomerWeb/appsettings.Development.json](CustomerWeb/appsettings.Development.json)
  - Khởi chạy: [CustomerWeb/Properties/launchSettings.json](CustomerWeb/Properties/launchSettings.json)
  - Controllers: [CustomerWeb/Controllers](CustomerWeb/Controllers)
    - [CartController.cs](CustomerWeb/Controllers/CartController.cs), [ClientController.cs](CustomerWeb/Controllers/ClientController.cs),
      [HomeController.cs](CustomerWeb/Controllers/HomeController.cs), [PublicProxyController.cs](CustomerWeb/Controllers/PublicProxyController.cs)
  - Services: [CustomerWeb/Services](CustomerWeb/Services)
    - [IBackendApiClient.cs](CustomerWeb/Services/IBackendApiClient.cs), [BackendApiClient.cs](CustomerWeb/Services/BackendApiClient.cs)
  - Views: [CustomerWeb/Views](CustomerWeb/Views)
    - Layout & shared: [_ViewImports.cshtml](CustomerWeb/Views/_ViewImports.cshtml), [_ViewStart.cshtml](CustomerWeb/Views/_ViewStart.cshtml), [Shared](CustomerWeb/Views/Shared)
    - Khu vực: [Client](CustomerWeb/Views/Client), [Home](CustomerWeb/Views/Home)
  - Tài sản tĩnh: [CustomerWeb/wwwroot](CustomerWeb/wwwroot)
  - DTO: [CustomerWeb/Dtos.cs](CustomerWeb/Dtos.cs)

- KdsWeb: [clients/KdsWeb](KdsWeb)
  - Entry: [KdsWeb/Program.cs](KdsWeb/Program.cs)
  - Cấu hình: [KdsWeb/appsettings.json](KdsWeb/appsettings.json), [KdsWeb/appsettings.Development.json](KdsWeb/appsettings.Development.json)
  - Khởi chạy: [KdsWeb/Properties/launchSettings.json](KdsWeb/Properties/launchSettings.json)
  - Controllers: [KdsWeb/Controllers](KdsWeb/Controllers)
    - [BoardController.cs](KdsWeb/Controllers/BoardController.cs)
  - Backend client: [KdsWeb/BackendApiClient.cs](KdsWeb/BackendApiClient.cs)
  - Views: [KdsWeb/Views](KdsWeb/Views)
  - Tài sản tĩnh: [KdsWeb/wwwroot](KdsWeb/wwwroot)
  - DTO: [KdsWeb/Dtos/Dtos.cs](KdsWeb/Dtos/Dtos.cs)

## Cấu hình

Mỗi ứng dụng dùng appsettings*.json riêng:
- Backend API base URL, thông tin auth (nếu cần) khai báo trong appsettings.* của từng app:
  - AdminWeb: [AdminWeb/appsettings.json](AdminWeb/appsettings.json)
  - CustomerWeb: [CustomerWeb/appsettings.json](CustomerWeb/appsettings.json)
  - KdsWeb: [KdsWeb/appsettings.json](KdsWeb/appsettings.json)
- Môi trường: biến ASPNETCORE_ENVIRONMENT = Development khi chạy cục bộ.

Lưu ý:
- Các biến thể dev/local đã được ignore ở repo gốc qua [.gitignore](../.gitignore).
- Các client gọi Backend qua các lớp BackendApiClient tương ứng.

## Build & chạy

- Mở solution: [clients/TableOrdering.Clients.sln](TableOrdering.Clients.sln) và F5 dự án mong muốn, hoặc CLI:

```sh
# Build toàn bộ clients
dotnet build clients/TableOrdering.Clients.sln

# Chạy từng ứng dụng
dotnet run --project clients/AdminWeb/AdminWeb.csproj
dotnet run --project clients/CustomerWeb/CustomerWeb.csproj
dotnet run --project clients/KdsWeb/KdsWeb.csproj
```

Ứng dụng sẽ dùng port theo [launchSettings.json] của mỗi dự án:
- AdminWeb: [AdminWeb/Properties/launchSettings.json](AdminWeb/Properties/launchSettings.json)
- CustomerWeb: [CustomerWeb/Properties/launchSettings.json](CustomerWeb/Properties/launchSettings.json)
- KdsWeb: [KdsWeb/Properties/launchSettings.json](KdsWeb/Properties/launchSettings.json)

## Tích hợp Backend

- API backend: xem hướng dẫn tại [backend/# Backend – TableOrdering.md](../backend/%23%20Backend%20%E2%80%93%20TableOrdering.md)
- Đảm bảo Backend chạy và URL cấu hình trong appsettings của Clients trỏ đúng đến API.

## Ghi chú phát triển

- Kiến trúc MVC ASP.NET Core: Controllers → Services (HTTP client) → Views.
- Dùng DTO nội bộ ở mỗi app để map dữ liệu trả về từ Backend.
- Thử nhanh endpoints bằng cách chạy song song Backend và Client tương ứng.