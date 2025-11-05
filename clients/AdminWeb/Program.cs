using AdminWeb.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddHttpContextAccessor();

// AuthN/Z - Default to Cookie for MVC; keep JwtBearer registered (reads token from cookie "admin_token")
builder.Services
 .AddAuthentication(options =>
 {
 options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
 options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
 options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
 })
 .AddCookie(opt =>
 {
 opt.LoginPath = "/login";
 opt.AccessDeniedPath = "/forbidden";
 opt.Cookie.Name = "aw_auth";
 opt.SlidingExpiration = true;
 opt.ExpireTimeSpan = TimeSpan.FromHours(8);
 })
 .AddJwtBearer(o =>
 {
 o.RequireHttpsMetadata = false; // dev only; set true in production over HTTPS
 o.TokenValidationParameters = new TokenValidationParameters
 {
 ValidateIssuer = true,
 ValidateAudience = true,
 ValidateLifetime = true,
 ValidateIssuerSigningKey = true,
 ValidIssuer = builder.Configuration["Jwt:Issuer"],
 ValidAudience = builder.Configuration["Jwt:Audience"],
 IssuerSigningKey = new SymmetricSecurityKey(
 Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)),
 ClockSkew = TimeSpan.FromMinutes(1)
 };
 o.Events = new JwtBearerEvents
 {
 OnMessageReceived = ctx =>
 {
 var t = ctx.Request.Cookies["admin_token"];
 if (!string.IsNullOrEmpty(t)) ctx.Token = t;
 return Task.CompletedTask;
 },
 OnChallenge = ctx =>
 {
 // Avoid interfering with static files
 if (!ctx.Handled && ctx.HttpContext.Request.Path.HasValue &&
 !ctx.HttpContext.Request.Path.Value!.StartsWith("/lib") &&
 !ctx.HttpContext.Request.Path.Value!.StartsWith("/css") &&
 !ctx.HttpContext.Request.Path.Value!.StartsWith("/js") &&
 !ctx.HttpContext.Request.Path.Value!.StartsWith("/images"))
 {
 ctx.HandleResponse();
 var ret = Uri.EscapeDataString(ctx.Request.Path + ctx.Request.QueryString);
 ctx.Response.Redirect($"/login?returnUrl={ret}");
 }
 return Task.CompletedTask;
 }
 };
 });

builder.Services.AddAuthorization(opt =>
{
 opt.AddPolicy("RequireSignedIn", p => p.RequireAuthenticatedUser());
});

// HttpClient gọi Backend API (typed)
builder.Services.AddHttpClient<IBackendApiClient, BackendApiClient>((sp, http) =>
{
 var cfg = sp.GetRequiredService<IConfiguration>();
 http.BaseAddress = new Uri(cfg["Backend:BaseUrl"]!);
 var token = cfg["Backend:StaticBearer"];
 if (!string.IsNullOrWhiteSpace(token))
 http.DefaultRequestHeaders.Authorization =
 new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
});

// SignalR client sẽ dùng JS, không cần DI
var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
 app.UseExceptionHandler("/Home/Error");
 app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// Auth middlewares
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
 name: "default",
 pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

