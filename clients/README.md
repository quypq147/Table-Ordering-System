# Clients – TableOrdering (.NET 10)

Tập hợp ứng dụng khách: AdminWeb, CustomerWeb, KdsWeb (MVC), WaiterApp (MAUI), cùng dự án Contracts dùng chia sẻ kiểu.

## Dự án

- AdminWeb: quản trị (danh mục, món, bàn, đơn, người dùng)
- CustomerWeb: giao diện khách đặt món / giỏ hàng
- KdsWeb: hiển thị trạng thái đơn ở bếp (SignalR)
- WaiterApp: ứng dụng phục vụ (MAUI)
- TableOrdering.Contracts: kiểu chia sẻ (DTO / enums chung)

## Cấu trúc chính

- Solution: TableOrdering.Clients.sln
- Mỗi web: Program.cs, Controllers/, Services/ (BackendApiClient), Views/, wwwroot/
- WaiterApp: MAUI (Pages/, Components/, Services/)

## Yêu cầu

- .NET 10 SDK
- Backend API chạy (cấu hình BaseUrl trong appsettings.*)
- (Tuỳ chọn) Node nếu build asset tĩnh mở rộng

## Chạy từng ứng dụng

```sh
dotnet restore
dotnet build TableOrdering.Clients.sln

dotnet run --project AdminWeb/AdminWeb.csproj
dotnet run --project CustomerWeb/CustomerWeb.csproj
dotnet run --project KdsWeb/KdsWeb.csproj
dotnet run --project WaiterApp/WaiterApp.csproj
```

Port lấy từ Properties/launchSettings.json của mỗi dự án.

## Cấu hình

- appsettings.json / appsettings.Development.json: BackendBaseUrl, tùy chọn Auth
- Đặt ASPNETCORE_ENVIRONMENT=Development khi local

## Tích hợp Backend

- Services/BackendApiClient.* gọi HTTP đến API (JWT nếu cần)
- KdsWeb dùng SignalR để subscribe ticket/order status

## Nâng cấp .NET 10

- Sửa TargetFramework net10.0
- Cập nhật package (Microsoft.AspNetCore.*, SignalR, MAUI)
- Kiểm tra razor & tooling tương thích

## Phát triển

- Tách logic gọi API vào Services (giảm duplicate)
- DTO nội bộ: Dtos/ hoặc Models/
- Dùng partial Views / Layout để tái sử dụng header/footer

## Gợi ý mở rộng

- Thêm Tailwind/Bootstrap cập nhật phiên bản
- Thêm Auth flow đồng bộ (login → token → cookie)
- Thêm e2e test (Playwright/Selenium)

## Contracts

- TableOrdering.Contracts chứa enum/trừu tượng chung (TableStatus …) dùng Reference thay vì copy mã.