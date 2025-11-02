using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NotiBlock.Backend.DTOs;
using NotiBlock.Backend.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace NotiBlock.Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController(IAuthService service) : ControllerBase
    {
        private readonly IAuthService _service = service;

        // Consumer Endpoints
        [HttpPost("consumer/register")]
        public async Task<IActionResult> RegisterConsumer([FromBody] AuthRegisterDTO dto)
        {
            var token = await _service.RegisterConsumerAsync(dto);

            Response.Cookies.Append("jwt_token", token, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = DateTime.UtcNow.AddHours(1)
            });

            return Ok(new { token });
        }

        [HttpPost("consumer/login")]
        public async Task<IActionResult> LoginConsumer([FromBody] AuthLoginDTO dto)
        {
            var token = await _service.LoginConsumerAsync(dto);

            Response.Cookies.Append("jwt_token", token, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = DateTime.UtcNow.AddHours(1)
            });

            return Ok(new { token });
        }


        // Reseller Endpoints
        [HttpPost("reseller/register")]
        public async Task<IActionResult> RegisterReseller([FromBody] AuthRegisterDTO dto)
        {
            var token = await _service.RegisterResellerAsync(dto);

            Response.Cookies.Append("jwt_token", token, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = DateTime.UtcNow.AddHours(1)
            });

            return Ok(new { token });
        }

        [HttpPost("reseller/login")]
        public async Task<IActionResult> LoginReseller([FromBody] AuthLoginDTO dto)
        {
            var token = await _service.LoginResellerAsync(dto);

            Response.Cookies.Append("jwt_token", token, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = DateTime.UtcNow.AddHours(1)
            });

            return Ok(new { token });
        }


        // Manufacturer Endpoints
        [HttpPost("manufacturer/register")]
        public async Task<IActionResult> RegisterManufacturer([FromBody] AuthRegisterDTO dto)
        {
            var token = await _service.RegisterManufacturerAsync(dto);

            Response.Cookies.Append("jwt_token", token, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = DateTime.UtcNow.AddHours(1)
            });

            return Ok(new { token });
        }

        [HttpPost("manufacturer/login")]
        public async Task<IActionResult> LoginManufacturer([FromBody] AuthLoginDTO dto)
        {
            var token = await _service.LoginManufacturerAsync(dto);

            Response.Cookies.Append("jwt_token", token, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = DateTime.UtcNow.AddHours(1)
            });

            return Ok(new { token });
        }


        // Regulator Endpoints
        [HttpPost("regulator/register")]
        public async Task<IActionResult> RegisterRegulator([FromBody] AuthRegisterDTO dto)
        {
            var token = await _service.RegisterRegulatorAsync(dto);

            Response.Cookies.Append("jwt_token", token, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = DateTime.UtcNow.AddHours(1)
            });

            return Ok(new { token });
        }

        [HttpPost("regulator/login")]
        public async Task<IActionResult> LoginRegulator([FromBody] AuthLoginDTO dto)
        {
            var token = await _service.LoginRegulatorAsync(dto);

            Response.Cookies.Append("jwt_token", token, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = DateTime.UtcNow.AddHours(1)
            });

            return Ok(new { token });
        }


        // General
        [Authorize]
        [HttpGet("me")]
        public IActionResult Me()
        {
            string? GetFirstClaim(params string[] types) =>
                types.Select(t => User.FindFirst(t)?.Value).FirstOrDefault(v => v is not null);

            var email = GetFirstClaim(ClaimTypes.Email, "email", JwtRegisteredClaimNames.Email);

            var role = GetFirstClaim(ClaimTypes.Role, "role");

            var userId = GetFirstClaim(ClaimTypes.NameIdentifier, "nameid", JwtRegisteredClaimNames.Sub);

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
