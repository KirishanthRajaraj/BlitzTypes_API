using BlitzTypes_API.Data;
using BlitzTypes_API.Models;
using BlitzTypes_API.Models.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BlitzTypes_API.Repositories
{
    public class UserRepository
    {
        public readonly BlitzTypesContext _context;
        private readonly UserManager<User> _userManager;

        public UserRepository(BlitzTypesContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public List<User> GetAllUsers()
        {
            var query = _context.Users.ToList();
            return query;
        }

        public User? GetUserById(string id)
        {
            var query = _context.Users.FirstOrDefault(x => x.Id == id);
            return query;
        }

        public async Task<User?> GetUserByRefreshTokenHashAsync(Guid refreshTokenHash)
        {
            var query = await _userManager.Users.FirstOrDefaultAsync(u => u.refreshToken == refreshTokenHash);
                return query;
        }

        public User? SetUserById(string id)
        {
            var query = _context.Users.FirstOrDefault(x => x.Id == id);
            return query;
        }

        public async Task<bool> removeRefreshTokenFromUser(User? user)
        {
            if(user == null) return false;
            user.refreshToken = null;
            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                return true;
            }
            return false;
        }
    }
}
