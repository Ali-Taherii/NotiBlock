using Microsoft.AspNetCore.Mvc;
using NotiBlock.Backend.DTOs;
using NotiBlock.Backend.Interfaces;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

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
                var token = await _authService.RegisterAsync(registerDto);

                // Set the JWT token as an HTTP-only cookie
                Response.Cookies.Append("jwt_token", token, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true, // Use only on HTTPS
                    SameSite = SameSiteMode.None,
                    Expires = DateTime.UtcNow.AddHours(5)
                });

                return Ok(new { message = "Registration successful" });
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
                    SameSite = SameSiteMode.None,
                    Expires = DateTime.UtcNow.AddHours(5)
                });

                // Return the token in the response body as well
                return Ok(new { message = "Login successful" });
            }
            catch (Exception ex)
            {
                // Log the exception
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [Authorize]
        [HttpGet("me")]
        public IActionResult Me()
        {
            // Extract standard claims
            var email = User.FindFirst(ClaimTypes.Email)?.Value
                        ?? User.FindFirst("email")?.Value;

            var role = User.FindFirst(ClaimTypes.Role)?.Value
                       ?? User.FindFirst("role")?.Value;

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                         ?? User.FindFirst("sub")?.Value;

            return Ok(new { userId, email, role });
        }


        [HttpPost("logout")]
        public IActionResult Logout()
        {
            // Clear the JWT token cookie by setting an expired cookie with the same name
            Response.Cookies.Append("jwt_token", "", new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = DateTime.UtcNow.AddDays(-1) // Set expiration in the past
            });

            return Ok(new { message = "Logout successful" });
        }
    }
}
