using Google.Apis.Auth;
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
  private readonly IConfiguration _config;

  public AuthController(ChatContext context, IConfiguration config)
  {
    _context = context;
    _config = config;
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

    var tokenSecret = _config["Authentication:TokenSecret"];
    if (string.IsNullOrEmpty(tokenSecret))
    {
      throw new InvalidOperationException("Token secret is not configured properly.");
    }
    var token = GenerateJwtToken(user, tokenSecret);
    return Ok(new { Token = token });
  }

  [HttpPost("google-callback")]
  public async Task<IActionResult> GoogleCallback([FromBody] GoogleSignInDto googleDto)
  {
    var token = googleDto.Token;

    // Verify Google token using Google's API
    var payload = await VerifyGoogleToken(token);
    if (payload == null)
    {
      return Unauthorized(new { message = "Invalid Google token." });
    }

    // Extract user email from Google payload
    var email = payload.Email;

    // Find or register the user as explained in the previous response
    var user = _context.Users.SingleOrDefault(u => u.Username == email);
    if (user == null)
    {
      // Create a new user if not found
      user = new User { Username = email, PasswordHash = BCrypt.Net.BCrypt.HashPassword("google") }; // not optimal, but it requires a hashed password
      _context.Users.Add(user);
      _context.SaveChanges();
    }

    // Generate JWT token and return to the client
    var tokenSecret = _config["Authentication:TokenSecret"];
    var jwtToken = GenerateJwtToken(user, tokenSecret);

    return Ok(new { Token = jwtToken });
  }

  private async Task<GoogleJsonWebSignature.Payload> VerifyGoogleToken(string token)
  {
    try
    {
      var settings = new GoogleJsonWebSignature.ValidationSettings
      {
        Audience = new List<string> { _config["Authentication:Google:ClientId"] }
      };
      var payload = await GoogleJsonWebSignature.ValidateAsync(token, settings);
      return payload;
    }
    catch (Exception)
    {
      return null;
    }
  }

  private static string GenerateJwtToken(User user, string secret)
  {
    var tokenHandler = new JwtSecurityTokenHandler();
    var key = Encoding.ASCII.GetBytes(secret);

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