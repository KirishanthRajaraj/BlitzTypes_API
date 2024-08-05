using BlitzTypes_API.Data;
using BlitzTypes_API.Models;
using BlitzTypes_API.Models.Authentication;

namespace BlitzTypes_API.Repositories
{
    public class UserRepository
    {
        public readonly BlitzTypesContext _context;
        public UserRepository(BlitzTypesContext context)
        {
            _context = context;
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

        public User? SetUserById(string id)
        {
            var query = _context.Users.FirstOrDefault(x => x.Id == id);
            return query;
        }
    }
}
