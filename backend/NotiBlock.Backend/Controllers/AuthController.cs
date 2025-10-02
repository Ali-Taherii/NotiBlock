using Microsoft.AspNetCore.Mvc;
using NotiBlock.Backend.Interfaces;
using NotiBlock.Backend.DTOs;

namespace NotiBlock.Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController(IAuthService authService) : ControllerBase
    {
        private readonly IAuthService _authService = authService;

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
            catch (Exception)
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
                var token = await _authService.LoginAsync(loginDto);

                if (string.IsNullOrEmpty(token))
                    return Unauthorized(new { message = "Invalid email or password" });

                // Set the JWT token as an HTTP-only cookie
                Response.Cookies.Append("jwt_token", token, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true, // Use only on HTTPS
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTime.UtcNow.AddDays(1) // Cookie expiration (adjust as needed)
                });

                // Return the token in the response body as well
                return Ok(new { message = "Login successful", token });
            }
            catch (Exception ex)
            {
                // Log the exception
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}
