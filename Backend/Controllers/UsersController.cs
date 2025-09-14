using Backend.Data;
using Backend.Models;
using Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/users")]
    public class UsersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly PasswordService _passwordService;

        public UsersController(ApplicationDbContext context, PasswordService passwordService)
        {
            _context = context;
            _passwordService = passwordService;
        }

        // GET: api/users
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserResponse>>> GetUsers()
        {
            var users = await _context.Users
                .Select(u => new UserResponse
                {
                    Id = u.Id,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    Email = u.Email,
                    CreatedAt = u.CreatedAt
                })
                .ToListAsync();

            return users;
        }

        // GET: api/users/5
        [HttpGet("{id}")]
        public async Task<ActionResult<UserResponse>> GetUser(int id)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            var userResponse = new UserResponse
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                CreatedAt = user.CreatedAt
            };

            return userResponse;
        }

        // POST: api/users
        [HttpPost]
        public async Task<ActionResult<UserResponse>> CreateUser(Register register)
        {
            var email = register.Email.Trim().ToLowerInvariant();

            if (await _context.Users.AnyAsync(u => u.Email == email))
            {
                return BadRequest("Email already in use.");
            }

            if (register.Password.Length < 6)
            {
                return BadRequest("Password must be at least 6 characters long.");
            }

            var hashed = _passwordService.HashPassword(register.Password);

            var user = new User
            {
                FirstName = register.FirstName,
                LastName = register.LastName,
                Email = email,
                Password = hashed
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
            };

            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, userResponse);
        }

        // PUT: api/users/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, UserResponse update)
        {
            if (id != update.Id)
            {
                return BadRequest();
            }

            var existing = await _context.Users.FindAsync(id);
            if (existing == null)
            {
                return NotFound();
            }

            existing.FirstName = update.FirstName;
            existing.LastName = update.LastName;
            existing.Email = update.Email.Trim().ToLowerInvariant();
            existing.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/users/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
