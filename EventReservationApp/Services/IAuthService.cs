using BCrypt.Net;
using EventReservations.Dto;
using EventReservations.Models;  
using EventReservations.Repositories;
using Microsoft.EntityFrameworkCore;
using Stripe.Forwarding;


namespace EventReservations.Services  
{
    public interface IAuthService
    {
        Task<User> RegisterAsync(RegisterRequestDto dto);
        Task<User?> LoginAsync(LoginRequestDto loginDto);
    }

    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;

        public AuthService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<User> RegisterAsync(RegisterRequestDto dto)
        {
            if (await _userRepository.EmailExistsAsync(dto.Email))
                return null;

            var user = new User
            {
                Name = dto.Name,
                Email = dto.Email,
                Role = "User",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Created = DateTime.UtcNow
            };

            await _userRepository.AddAsync(user);
            await _userRepository.SaveChangesAsync();

            return user;
        }

        public async Task<User?> LoginAsync(LoginRequestDto loginDto)
        {
            var user = await _userRepository.GetByEmailAsync(loginDto.Email);

            if (user == null)
                return null;

            bool passwordValid = BCrypt.Net.BCrypt.Verify(
                loginDto.Password,
                user.PasswordHash
            );

            return passwordValid ? user : null;
        }
    }

}

