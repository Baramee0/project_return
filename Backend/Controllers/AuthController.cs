using Backend.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
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

        [HttpPost("register")]
        public async Task<ActionResult<AuthResponse>> Register(Register register)
        {
            // Check if the email already exists
            if (await _context.Users.AnyAsync(u => u.Email == register.Email))
            {
                return BadRequest("Email already in use.");
            }

            //Hash password
            string hashedPassword = _passwordService.HashPassword(register.Password);

            var user = new User
            {
                FirstName = register.FirstName,
                LastName = register.LastName,
                Email = register.Email,
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
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt
            };

            string token = _jwtService.GenerateToken(user);

            return Ok(new AuthResponse
            {
                User = user,
                Token = token // Replace with actual JWT token generation logic",
            });
        }
    }
}