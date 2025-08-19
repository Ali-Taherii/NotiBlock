using Microsoft.AspNetCore.Mvc;
using NotiBlock.Backend.Interfaces;
using NotiBlock.Backend.DTOs;
using System.Threading.Tasks;

namespace NotiBlock.Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] AuthDTO.AuthRegisterDto registerDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var user = await _authService.RegisterAsync(registerDto);
                return Ok(new { message = "Registration successful", userId = user.Id });
            }
            catch (Exception ex)
            {
                // Log the exception
                return StatusCode(500, new { message = "An error occurred during registration" });
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] AuthDTO.AuthLoginDto loginDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var user = await _authService.LoginAsync(loginDto);

                if (user == null)
                    return Unauthorized(new { message = "Invalid email or password" });

                // Here you would generate and return a JWT token
                // For now, just returning a success message
                return Ok(new { message = "Login successful", userId = user.Id });
            }
            catch (Exception ex)
            {
                // Log the exception
                return StatusCode(500, new { message = "An error occurred during login" });
            }
        }
    }
}
