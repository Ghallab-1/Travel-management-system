using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TravelManagement.Infrastructure.Data;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;

namespace TravelManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IConfiguration _config;

    public AuthController(AppDbContext db, IConfiguration config)
    {
        _db = db;
        _config = config;
    }

    public class LoginRequest { public string Email { get; set; } = string.Empty; public string Password { get; set; } = string.Empty; }
    public class LoginResponse { public string Token { get; set; } = string.Empty; public int ExpiresIn { get; set; } public int UserId { get; set; } public string FullName { get; set; } = string.Empty; public string RoleName { get; set; } = string.Empty; public int DepartmentId { get; set; } }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request?.Email) || string.IsNullOrWhiteSpace(request?.Password)) return BadRequest("Email and password required");

        var user = await _db.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.Email == request.Email);
        if (user == null) return Unauthorized();

        // Verify password using BCrypt
        if (string.IsNullOrWhiteSpace(user.PasswordHash) || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            return Unauthorized();
        }

        var jwtKey = _config["Jwt:Key"] ?? "super_secret_dev_key_please_change";
        var jwtIssuer = _config["Jwt:Issuer"] ?? "TravelManagementAPI";
        var keyBytes = System.Text.Encoding.UTF8.GetBytes(jwtKey);

        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, user.FullName)
        };
        if (user.Role != null)
        {
            claims.Add(new Claim(ClaimTypes.Role, user.Role.RoleName));
        }

        var tokenHandler = new JwtSecurityTokenHandler();
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddHours(8),
            Issuer = jwtIssuer,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(keyBytes), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        var tokenString = tokenHandler.WriteToken(token);

        return Ok(new LoginResponse { Token = tokenString, ExpiresIn = 60 * 60 * 8, UserId = user.Id, FullName = user.FullName, RoleName = user.Role?.RoleName ?? string.Empty, DepartmentId = user.DepartmentId });
    }
}