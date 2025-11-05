using Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly SignInManager<AppUser> _signIn;
    private readonly UserManager<AppUser> _userMgr;
    private readonly RoleManager<AppRole> _roleMgr;
    private readonly IConfiguration _cfg;

    public AuthController(SignInManager<AppUser> signIn, UserManager<AppUser> userMgr,
                          RoleManager<AppRole> roleMgr, IConfiguration cfg)
    {
        _signIn = signIn; _userMgr = userMgr; _roleMgr = roleMgr; _cfg = cfg;
    }

    public sealed record LoginDto(string UserNameOrEmail, string Password);

    [HttpPost("login")]
    [ProducesResponseType(typeof(object), 200)]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var user = await _userMgr.FindByNameAsync(dto.UserNameOrEmail)
                   ?? await _userMgr.FindByEmailAsync(dto.UserNameOrEmail);
        if (user is null || !user.IsActive) return Unauthorized();

        if (!await _userMgr.CheckPasswordAsync(user, dto.Password))
            return Unauthorized();

        var roles = await _userMgr.GetRolesAsync(user);

        var jwt = _cfg.GetSection("Jwt");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(ClaimTypes.Name, user.UserName ?? string.Empty),
            new(ClaimTypes.Email, user.Email ?? string.Empty)
        };
        claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

        var token = new JwtSecurityToken(
            issuer: jwt["Issuer"], audience: jwt["Audience"],
            claims: claims, expires: DateTime.UtcNow.AddHours(8), signingCredentials: creds);

        return Ok(new
        {
            token = new JwtSecurityTokenHandler().WriteToken(token),
            user = new { id = user.Id, user.UserName, user.Email, roles }
        });
    }
}