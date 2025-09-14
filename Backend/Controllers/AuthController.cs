using Backend.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Backend.Models;
using Backend.Services;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly PasswordService _passwordService;

        private readonly JwtService _jwtService;

        public AuthController(ApplicationDbContext context, PasswordService passwordService, JwtService jwtService)
        {
            _context = context;
            _passwordService = passwordService;
            _jwtService = jwtService;
        }

        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<ActionResult<AuthResponse>> Register(Register register)
        {
            var email = register.Email.Trim().ToLowerInvariant();
            // Check if the email already exists
            if (await _context.Users.AnyAsync(u => u.Email == email))
            {
                return BadRequest("Email already in use.");
            }

            if (register.Password.Length < 6)
            {
                return BadRequest("Password must be at least 6 characters long.");
            }

            if (!register.Password.Any(char.IsUpper))
            {
                return BadRequest("Password must contain at least one uppercase letter.");
            }

            if (!register.Password.Any(char.IsLower))
            {
                return BadRequest("Password must contain at least one lowercase letter.");
            }

            //Hash password
            string hashedPassword = _passwordService.HashPassword(register.Password);

            var user = new User
            {
                FirstName = register.FirstName,
                LastName = register.LastName,
                Email = email,
                Password = hashedPassword
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var userResponse = new UserResponse
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                CreatedAt = user.CreatedAt
                // UpdatedAt = user.UpdatedAt
            };

            string token = _jwtService.GenerateToken(user);
            string successMessage = "User registered successfully.";

            return Ok(new AuthResponse
            {
                Message = successMessage,
                User = userResponse,
                Token = token
            });
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<ActionResult<AuthResponse>> Login(Login login)
        {
            var email = login.Email.Trim().ToLowerInvariant();
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

            if (user == null || !_passwordService.VerifyPassword(login.Password, user.Password))
            {
                return Unauthorized("Invalid email or password.");
            }

            var userResponse = new UserResponse
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                CreatedAt = user.CreatedAt
                // UpdatedAt = user.UpdatedAt
            };

            string token = _jwtService.GenerateToken(user);
            string successMessage = "Login successful.";

            return Ok(new AuthResponse
            {
                Message = successMessage,
                User = userResponse,
                Token = token
            });
        }
    }
}
