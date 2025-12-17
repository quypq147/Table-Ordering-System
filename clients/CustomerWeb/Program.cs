using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using CustomerWeb.Services;

var builder = WebApplication.CreateBuilder(args);

// Đọc URL backend (API) từ cấu hình
var backendBase = builder.Configuration["Backend:BaseUrl"] ?? "http://localhost:5075";

// MVC
builder.Services.AddControllersWithViews();

// ✅ ĐĂNG KÝ HttpClient + typed client for IBackendApiClient
builder.Services.AddHttpClient<IBackendApiClient, BackendApiClient>(c =>
{
    c.BaseAddress = new Uri(backendBase);
});

// ✅ ĐĂNG KÝ named client "backend" for PublicProxyController
builder.Services.AddHttpClient("backend", c =>
{
    c.BaseAddress = new Uri(backendBase);
});

// ✅ Đăng ký IHttpContextAccessor để BackendApiClient có thể truy cập cookies/session
builder.Services.AddHttpContextAccessor();

// ✅ Session requires a distributed cache
builder.Services.AddDistributedMemoryCache();

// ✅ Kích hoạt Session
builder.Services.AddSession(options =>
{
    options.Cookie.Name = ".CustomerWeb.Session";
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// (tuỳ chọn) bật HSTS/HTTPS cho Production, tắt ở Dev để khỏi cảnh báo
if (builder.Environment.IsProduction())
{
    builder.Services.AddHttpsRedirection(options =>
    {
        // nếu bạn có https port cụ thể thì điền vào đây
        // options.HttpsPort = 7049;
    });
}

var app = builder.Build();

// ⚠️ Chỉ redirect HTTPS ở Production (Dev nhiều profile không khai báo https)
if (app.Environment.IsProduction())
{
    app.UseHttpsRedirection();
    app.UseHsts();
}

// Static files + routing
app.UseStaticFiles();
app.UseRouting();

// ✅ Sử dụng Session (phải SAU UseRouting và TRƯỚC MapControllerRoute)
app.UseSession();
app.MapReverseProxy();

// MVC routes
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

