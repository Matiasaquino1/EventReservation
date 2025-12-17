using EventReservations.Dto;
using EventReservations.Models;


namespace EventReservations.Repositories
{
    public interface IUserRepository
    {
        Task<IEnumerable<User>> GetAllAsync();
        Task<User?> GetByIdAsync(int id);
        Task UpdateAsync(User user);
        Task DeleteAsync(User user);
        Task<int> CountAdminsAsync();

        Task<User?> AddAsync(User user);
        Task<bool> EmailExistsAsync(string email);
        Task<User?> GetByEmailAsync(string email);

        Task SaveChangesAsync();
    }
}
