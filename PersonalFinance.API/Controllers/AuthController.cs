using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using PersonalFinance.API.Data;
using PersonalFinance.API.Models;

namespace PersonalFinance.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            try
            {
                var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
                if (existingUser != null)
                {
                    return BadRequest("Пользователь с таким email уже существует");
                }

                var user = new User
                {
                    // НЕ УСТАНАВЛИВАЕМ Id - он сгенерируется автоматически!
                    Email = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    PasswordHash = HashPassword(model.Password),
                    CreatedAt = DateTime.UtcNow
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync(); // Здесь Id автоматически присвоится

                await CreateDefaultCategories(user.Id); // Теперь user.Id содержит сгенерированный ID

                var token = GenerateJwtToken(user);

                return Ok(new
                {
                    Message = "Пользователь успешно зарегистрирован",
                    Token = token,
                    User = new { user.Id, user.Email, user.FirstName, user.LastName }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Ошибка при регистрации: {ex.Message}");
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
                if (user != null && VerifyPassword(model.Password, user.PasswordHash))
                {
                    var token = GenerateJwtToken(user);

                    return Ok(new
                    {
                        Token = token,
                        User = new { user.Id, user.Email, user.FirstName, user.LastName }
                    });
                }

                return Unauthorized("Неверный email или пароль");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Ошибка при входе: {ex.Message}");
            }
        }

        private string GenerateJwtToken(User user)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Secret"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: jwtSettings["ValidIssuer"],
                audience: jwtSettings["ValidAudience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(Convert.ToDouble(jwtSettings["ExpiryInMinutes"])),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private string HashPassword(string password)
        {
            using var sha256 = System.Security.Cryptography.SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }

        private bool VerifyPassword(string password, string storedHash)
        {
            return HashPassword(password) == storedHash;
        }

        private async Task CreateDefaultCategories(int userId)
        {
            var defaultCategories = new[]
            {
                new { Name = "Зарплата", IsIncome = true },
                new { Name = "Продукты", IsIncome = false },
                new { Name = "Транспорт", IsIncome = false },
                new { Name = "Жилье", IsIncome = false },
                new { Name = "Развлечения", IsIncome = false },
                new { Name = "Здоровье", IsIncome = false },
                new { Name = "Инвестиции", IsIncome = true }
            };

            foreach (var category in defaultCategories)
            {
                _context.Categories.Add(new Category
                {
                    Name = category.Name,
                    IsIncome = category.IsIncome,
                    UserId = userId,
                    Description = $"Категория по умолчанию: {category.Name}"
                });
            }

            await _context.SaveChangesAsync();
        }
    }

    public class RegisterModel
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class LoginModel
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}