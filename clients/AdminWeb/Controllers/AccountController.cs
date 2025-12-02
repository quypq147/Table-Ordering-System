using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using AdminWeb.Services;

namespace AdminWeb.Controllers;

public class AccountController(IBackendApiClient api) : Controller
{
 [HttpGet("/login")]
 public IActionResult Login(string? returnUrl = "/") => View(model: returnUrl);

 public sealed record LoginVm(string UserNameOrEmail, string Password, string? ReturnUrl, bool RememberMe = false);

 [HttpPost("/login")]
 public async Task<IActionResult> LoginPost([FromForm] LoginVm vm)
 {
 try
 {
 var (token, display, roles) = await api.LoginAsync(vm.UserNameOrEmail, vm.Password);

 //1) Cookie xác th?c c?a ASP.NET (?? [Authorize])
 var claims = new List<Claim>
 {
 new Claim(ClaimTypes.Name, display ?? vm.UserNameOrEmail),
 };
 foreach (var r in roles) claims.Add(new Claim(ClaimTypes.Role, r));

 var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
 var principal = new ClaimsPrincipal(identity);

 await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal,
 new AuthenticationProperties
 {
 IsPersistent = vm.RememberMe,
 ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
 });

 //2) Cookie l?u JWT ?? g?i Backend API (gi? nh? b?n ?ang làm)
 Response.Cookies.Append("admin_token", token, new CookieOptions
 {
 HttpOnly = true, SameSite = SameSiteMode.Lax, Secure = false // true ? prod/https
 });

 var ret = string.IsNullOrWhiteSpace(vm.ReturnUrl) ? "/" : vm.ReturnUrl!;
 return Redirect(ret);
 }
 catch (Exception ex)
 {
 ModelState.AddModelError(string.Empty, ex.Message);
 return View("Login", vm.ReturnUrl);
 }
 }

 [HttpPost("/logout")]
 public async Task<IActionResult> Logout()
 {
 await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
 Response.Cookies.Delete("admin_token");
 Response.Cookies.Delete("admin_name");
 return Redirect("/login");
 }
}
