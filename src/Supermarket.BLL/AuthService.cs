using System.Threading.Tasks;
using Supermarket.DAL;
using Supermarket.Models.Entities;

namespace Supermarket.BLL
{
    public class AuthService
    {
        private readonly UserRepository _userRepo;

        public AuthService(string connectionString)
        {
            _userRepo = new UserRepository(connectionString);
        }

        public async Task<User> LoginAsync(string username, string password)
        {
            var user = await _userRepo.GetUserByUsernameAsync(username);
            if (user != null && user.PasswordHash == password) // In production, use password hashing!
            {
                return user;
            }
            return null;
        }
    }
}
