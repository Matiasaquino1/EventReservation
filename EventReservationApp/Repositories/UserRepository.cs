using EventReservations.Data;
using EventReservations.Models;
using Microsoft.EntityFrameworkCore;

namespace EventReservations.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _context;
        public UserRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<User?> GetByIdAsync(int id)
        {
            return await _context.Users.FindAsync(id);
        }
        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        }
        public async Task<User?> AddAsync(User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user;

        }
        public async Task<User?> UpdateAsync(User user)
        {
            _context.Users.Update(user);  
            await _context.SaveChangesAsync();  
            return user; 
        }

        public async Task DeleteAsync(int id)
        {
            var user = await _context.Users.FindAsync(id); 
            if (user != null)
            {
                _context.Users.Remove(user);  
                await _context.SaveChangesAsync();  
            }
        }

        public async Task<IEnumerable<User?>> GetAllAsync()
        {
            return await _context.Users.ToListAsync();
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _context.Users.AnyAsync(u => u.Email == email);
        }

    }
}
