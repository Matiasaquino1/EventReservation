using Microsoft.AspNetCore.Mvc;
using EventReservations.Dto;
using EventReservations.Services; // IUserService
using Microsoft.AspNetCore.Authorization;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IJwtService _jwtService;

    public AuthController(IUserService userService, IJwtService jwtService)
    {
        _userService = userService;
        _jwtService = jwtService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
    {
        var registeredUser = await _userService.RegisterAsync(request);
        return Ok(new { registeredUser.UserId, registeredUser.Email, registeredUser.Name, registeredUser.Role });
    }

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


