using Microsoft.AspNetCore.Mvc;
using EventReservations.Dto;
using EventReservations.Services; // IUserService
using Microsoft.AspNetCore.Authorization;
using AutoMapper;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IJwtService _jwtService;
    private readonly IMapper _mapper;

    public AuthController(IUserService userService, IJwtService jwtService, IMapper mapper)
    {
        _userService = userService;
        _jwtService = jwtService;
        _mapper = mapper;
    }

    /// <summary>
    /// Registra un nuevo usuario en el sistema.
    /// </summary>
    /// <param name="request">Datos del usuario a registrar (email, password, role).</param>
    /// <returns>Usuario registrado o error si ya existe.</returns>
    /// <response code="200">Usuario registrado exitosamente.</response>
    /// <response code="400">Datos inválidos.</response>
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
    {
        var registeredUser = await _userService.RegisterAsync(request);
        return Ok(new { registeredUser.UserId, registeredUser.Email, registeredUser.Name, registeredUser.Role });
    }


    /// <summary>
    /// Inicia sesión y genera un token JWT.
    /// </summary>
    /// <param name="loginDto">Credenciales de login (email, password).</param>
    /// <returns>Token JWT si las credenciales son válidas.</returns>
    /// <response code="200">Login exitoso con token.</response>
    /// <response code="401">Credenciales inválidas.</response>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto loginDto)
    {
        var user = await _userService.LoginAsync(loginDto);
        if (user == null) return Unauthorized();

        var jwt = _jwtService.GenerateToken(user);

        var response = new LoginResponseDto
        {
            Token = jwt,
            Email = user.Email,
            Role = user.Role
        };

        return Ok(response);
    }
}


