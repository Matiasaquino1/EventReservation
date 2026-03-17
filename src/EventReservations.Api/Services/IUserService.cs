using EventReservations.Data;
using EventReservations.Models;
using EventReservations.Repositories;
using Microsoft.EntityFrameworkCore;

namespace EventReservations.Services
{
    public interface IUserService
    {
        Task<IEnumerable<User>> GetAllAsync();
        Task<User?> GetByIdAsync(int id);
        Task UpdateAsync(User user);
        Task DeleteAsync(User user);
        Task<(IEnumerable<User> Users, int TotalCount)> GetUsersPagedAsync(int page, int limit);
    }

    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;

        private readonly ApplicationDbContext _context;

        public UserService(IUserRepository userRepository, ApplicationDbContext context)
        {
            _context = context;
            _userRepository = userRepository;
        }

        public async Task<IEnumerable<User>> GetAllAsync()
        {
            return await _userRepository.GetAllAsync();
        }

        public async Task<User?> GetByIdAsync(int id)
        {
            return await _userRepository.GetByIdAsync(id);
        }

        public async Task UpdateAsync(User user)
        {
            await _userRepository.UpdateAsync(user);
            await _userRepository.SaveChangesAsync();
        }

        public async Task DeleteAsync(User user)
        {
            await _userRepository.DeleteAsync(user);
            await _userRepository.SaveChangesAsync();
        }

        public async Task<(IEnumerable<User> Users, int TotalCount)> GetUsersPagedAsync(int page, int limit)
        {
            var query = _context.Users
                .Include(u => u.Reservations)
                    .ThenInclude(r => r.Event)
                .AsNoTracking() 
                .AsQueryable();

            //  Obtener el total de registros antes de paginar
            var totalCount = await query.CountAsync();

            // paginación (Skip y Take)
            var users = await query
                .OrderBy(u => u.UserId) 
                .Skip((page - 1) * limit)
                .Take(limit)
                .ToListAsync();

            return (users, totalCount);
        }
    }



}
