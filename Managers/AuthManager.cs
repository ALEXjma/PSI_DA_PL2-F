using System.Linq;
using iTasks.Data;
using iTasks.Models;

namespace iTasks.Auth
{
    public class AuthManager
    {
        private readonly iTasksDbContext _context;

        public AuthManager()
        {
            _context = new iTasksDbContext();
        }

        public Utilizador Authenticate(string username, string password)
        {
            // Simple authentication (password should be hashed in production)
            return _context.Utilizadores.FirstOrDefault(u => u.Username == username && u.Password == password);
        }
    }
}