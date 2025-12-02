var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

// HttpClient gọi Backend API (typed)
builder.Services.AddHttpClient<BackendApiClient>((sp, http) =>
{
    var cfg = sp.GetRequiredService<IConfiguration>();
    http.BaseAddress = new Uri(cfg["Backend:BaseUrl"]!);
    // Nếu backend dùng JWT cố định cho DEV, set header ở đây:
    var token = cfg["Backend:StaticBearer"];
    if (!string.IsNullOrWhiteSpace(token))
        http.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
});

// Https redirection options (avoid warning in Dev when https port is unknown)
var httpsPortValue = builder.Configuration["ASPNETCORE_HTTPS_PORT"];
if (int.TryParse(httpsPortValue, out var httpsPort))
{
    builder.Services.AddHttpsRedirection(o => o.HttpsPort = httpsPort);
}

// SignalR client sẽ dùng JS, không cần DI
var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// Chỉ bật redirect khi chạy Production hoặc khi đã biết rõ https port
if (!app.Environment.IsDevelopment() || int.TryParse(app.Configuration["ASPNETCORE_HTTPS_PORT"], out _))
{
    app.UseHttpsRedirection();
}

app.UseStaticFiles();
app.UseRouting();

// Nếu có cookie auth cho Admin sau này
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Board}/{action=Index}/{id?}");

app.Run();
