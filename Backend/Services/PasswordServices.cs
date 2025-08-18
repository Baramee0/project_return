namespace Backend.Services
{
    public class PasswordService // Fixed class name
    {
        public string HashPassword(string password) // Removed static
        {
            return BCrypt.Net.BCrypt.HashPassword(password, 12);
        }

        public bool VerifyPassword(string password, string hashedPassword) // Removed static
        {
            return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
        }
    }
}