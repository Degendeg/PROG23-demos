using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using signalr_chat.Data;
using signalr_chat.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
  private readonly ChatContext _context;

  public AuthController(ChatContext context)
  {
    _context = context;
  }

  [HttpPost("signup")]
  public IActionResult Signup([FromBody] UserDto userDto)
  {
    if (userDto.Password != userDto.ConfirmPassword)
    {
      return BadRequest(new { message = "Passwords do not match" });
    }

    var hashedPassword = BCrypt.Net.BCrypt.HashPassword(userDto.Password);
    var user = new User { Username = userDto.Username, PasswordHash = hashedPassword };

    _context.Users.Add(user);
    _context.SaveChanges();

    return Ok(new { message = "User registered successfully" });
  }

  [HttpPost("login")]
  public IActionResult Login([FromBody] LoginDto loginDto)
  {
    var user = _context.Users.SingleOrDefault(u => u.Username == loginDto.Username);

    if (user == null || !BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash))
    {
      return Unauthorized(new { message = "Invalid credentials" });
    }

    var token = GenerateJwtToken(user);
    return Ok(new { Token = token });
  }

  private static string GenerateJwtToken(User user)
  {
    var tokenHandler = new JwtSecurityTokenHandler();
    var key = Encoding.ASCII.GetBytes("d5df9b30d5891d6e19c3eda79aef6fa0181cb5f0da195f2bbb54022c7d217b1b"); // private key

    var tokenDescriptor = new SecurityTokenDescriptor
    {
      Subject = new ClaimsIdentity(new Claim[]
        {
          new Claim(ClaimTypes.Name, user.Username)
        }),
      Expires = DateTime.UtcNow.AddHours(2),
      SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
    };

    var token = tokenHandler.CreateToken(tokenDescriptor);
    return tokenHandler.WriteToken(token);
  }
}

public class UserDto
{
  public required string Username { get; set; }
  public required string Password { get; set; }
  public required string ConfirmPassword { get; set; }
}

public class LoginDto
{
  public required string Username { get; set; }
  public required string Password { get; set; }
}