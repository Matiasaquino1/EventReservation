using Microsoft.AspNetCore.Mvc;
using EventReservations.Dto;
using EventReservations.Services; // IUserService, IJwtService
using Microsoft.AspNetCore.Authorization;
using AutoMapper;
using Microsoft.Extensions.Logging; // Para ILogger
using System.ComponentModel.DataAnnotations; // Para validación (si no está en DTOs)

[ApiController]
[Route("api/[controller]")]
/// <summary>
/// Controlador para gestionar autenticación de usuarios, incluyendo registro y login.
/// Utiliza servicios para manejar usuarios y generación de tokens JWT.
/// </summary>
public class AuthController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IJwtService _jwtService;
    private readonly IMapper _mapper;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IUserService userService, IJwtService jwtService, IMapper mapper, ILogger<AuthController> logger)
    {
        _userService = userService;
        _jwtService = jwtService;
        _mapper = mapper;
        _logger = logger;
    }

    /// <summary>
    /// Registra un nuevo usuario en el sistema.
    /// </summary>
    /// <param name="request">Datos del usuario a registrar (email, password, role).</param>
    /// <returns>Usuario registrado exitosamente.</returns>
    /// <response code="200">Usuario registrado exitosamente.</response>
    /// <response code="400">Datos inválidos o usuario ya existe.</response>
    /// <response code="500">Error interno del servidor.</response>
    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(LoginRequestDto), 200)]
    [ProducesResponseType(typeof(object), 400)]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
    {
        var registeredUser = await _userService.RegisterAsync(request);
        if (registeredUser == null)
        {
            _logger.LogWarning("Intento de registro fallido: usuario ya existe o datos inválidos para {Email}", request.Email);
            return BadRequest(new { error = "Usuario ya existe o datos inválidos." });
        }

        var userDto = _mapper.Map<LoginRequestDto>(registeredUser);
        _logger.LogInformation("Usuario registrado exitosamente: {UserId}", registeredUser.UserId);
        return Ok(userDto);
    }

    /// <summary>
    /// Inicia sesión y genera un token JWT.
    /// </summary>
    /// <param name="loginDto">Credenciales de login (email, password).</param>
    /// <returns>Token JWT y datos del usuario si las credenciales son válidas.</returns>
    /// <response code="200">Login exitoso con token.</response>
    /// <response code="401">Credenciales inválidas.</response>
    /// <response code="500">Error interno del servidor.</response>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(LoginResponseDto), 200)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto loginDto)
    {
        var user = await _userService.LoginAsync(loginDto);
        if (user == null)
        {
            _logger.LogWarning("Intento de login fallido para {Email}", loginDto.Email);
            return Unauthorized(new { error = "Credenciales inválidas." });
        }

        var jwt = _jwtService.GenerateToken(user);
        var response = _mapper.Map<LoginResponseDto>(user);
        response.Token = jwt;

        _logger.LogInformation("Login exitoso para usuario {UserId}", user.UserId);
        return Ok(response);
    }
}




