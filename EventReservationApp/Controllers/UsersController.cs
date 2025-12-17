using AutoMapper;
using EventReservations.Dto;
using EventReservations.Models;
using EventReservations.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EventReservations.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/users")]
    [Authorize(Roles = "Admin")]
    public class UsersController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;

        public UsersController(IUserRepository userRepository, IMapper mapper)
        {
            _userRepository = userRepository;
            _mapper = mapper;
        }

        // GET: api/v1/users
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetAllUsers()
        {
            var users = await _userRepository.GetAllAsync();

            var result = _mapper.Map<IEnumerable<UserDto>>(users);
            return Ok(result);
        }

        // GET: api/v1/users/{id}
        [HttpGet("{id:int}")]
        public async Task<ActionResult<UserDto>> GetUserById(int id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
                return NotFound(new { error = "Usuario no encontrado." });

            return Ok(_mapper.Map<UserDto>(user));
        }

        // PUT: api/v1/users/{id}
        [HttpPut("{id:int}")]
        public async Task<ActionResult> UpdateUser(int id, [FromBody] UserDto dto)
        {
            if (id != dto.UserId)
                return BadRequest(new { error = "ID inválido." });

            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
                return NotFound(new { error = "Usuario no encontrado." });

            // Actualización controlada
            user.Name = dto.Name;
            user.Email = dto.Email;
            user.Role = dto.Role;

            await _userRepository.UpdateAsync(user);

            return NoContent();
        }

        // DELETE: api/v1/users/{id}
        [HttpDelete("{id:int}")]
        public async Task<ActionResult> DeleteUser(int id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
                return NotFound(new { error = "Usuario no encontrado." });

            // Protección: no borrar último admin
            if (user.Role == "Admin")
            {
                var adminCount = await _userRepository.CountAdminsAsync();
                if (adminCount <= 1)
                    return BadRequest(new { error = "No se puede eliminar el último administrador." });
            }

            await _userRepository.DeleteAsync(user);

            return NoContent();
        }
    }
}

