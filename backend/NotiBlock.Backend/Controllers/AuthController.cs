using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NotiBlock.Backend.DTOs;
using NotiBlock.Backend.Interfaces;
using NotiBlock.Backend.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace NotiBlock.Backend.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController(IAuthService service, ILogger<AuthController> logger) : ControllerBase
    {
        private readonly IAuthService _service = service;
        private readonly ILogger<AuthController> _logger = logger;

        private OkObjectResult SetTokenAndRespond(string token, string message)
        {
            Response.Cookies.Append("jwt_token", token, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = DateTime.UtcNow.AddHours(1)
            });

            return Ok(ApiResponse<object>.SuccessResponse(new { token }, message));
        }

        // Consumer Endpoints
        [HttpPost("consumer/register")]
        public async Task<IActionResult> RegisterConsumer([FromBody] AuthRegisterDTO dto)
        {
            try
            {
                var token = await _service.RegisterConsumerAsync(dto);
                _logger.LogInformation("Consumer registered successfully via API");
                return SetTokenAndRespond(token, "Consumer registered successfully");
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid consumer registration request");
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Consumer registration failed");
                return Conflict(ApiResponse<object>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during consumer registration");
                return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred during registration"));
            }
        }

        [HttpPost("consumer/login")]
        public async Task<IActionResult> LoginConsumer([FromBody] AuthLoginDTO dto)
        {
            try
            {
                var token = await _service.LoginConsumerAsync(dto);
                _logger.LogInformation("Consumer logged in successfully via API");
                return SetTokenAndRespond(token, "Login successful");
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid consumer login request");
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Consumer login failed");
                return Unauthorized(ApiResponse<object>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during consumer login");
                return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred during login"));
            }
        }

        // Reseller Endpoints
        [HttpPost("reseller/register")]
        public async Task<IActionResult> RegisterReseller([FromBody] AuthRegisterDTO dto)
        {
            try
            {
                var token = await _service.RegisterResellerAsync(dto);
                _logger.LogInformation("Reseller registered successfully via API");
                return SetTokenAndRespond(token, "Reseller registered successfully");
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid reseller registration request");
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Reseller registration failed");
                return Conflict(ApiResponse<object>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during reseller registration");
                return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred during registration"));
            }
        }

        [HttpPost("reseller/login")]
        public async Task<IActionResult> LoginReseller([FromBody] AuthLoginDTO dto)
        {
            try
            {
                var token = await _service.LoginResellerAsync(dto);
                _logger.LogInformation("Reseller logged in successfully via API");
                return SetTokenAndRespond(token, "Login successful");
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid reseller login request");
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Reseller login failed");
                return Unauthorized(ApiResponse<object>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during reseller login");
                return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred during login"));
            }
        }

        // Manufacturer Endpoints
        [HttpPost("manufacturer/register")]
        public async Task<IActionResult> RegisterManufacturer([FromBody] AuthRegisterDTO dto)
        {
            try
            {
                var token = await _service.RegisterManufacturerAsync(dto);
                _logger.LogInformation("Manufacturer registered successfully via API");
                return SetTokenAndRespond(token, "Manufacturer registered successfully");
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid manufacturer registration request");
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Manufacturer registration failed");
                return Conflict(ApiResponse<object>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during manufacturer registration");
                return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred during registration"));
            }
        }

        [HttpPost("manufacturer/login")]
        public async Task<IActionResult> LoginManufacturer([FromBody] AuthLoginDTO dto)
        {
            try
            {
                var token = await _service.LoginManufacturerAsync(dto);
                _logger.LogInformation("Manufacturer logged in successfully via API");
                return SetTokenAndRespond(token, "Login successful");
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid manufacturer login request");
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Manufacturer login failed");
                return Unauthorized(ApiResponse<object>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during manufacturer login");
                return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred during login"));
            }
        }

        // Regulator Endpoints
        [HttpPost("regulator/register")]
        public async Task<IActionResult> RegisterRegulator([FromBody] AuthRegisterDTO dto)
        {
            try
            {
                var token = await _service.RegisterRegulatorAsync(dto);
                _logger.LogInformation("Regulator registered successfully via API");
                return SetTokenAndRespond(token, "Regulator registered successfully");
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid regulator registration request");
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Regulator registration failed");
                return Conflict(ApiResponse<object>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during regulator registration");
                return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred during registration"));
            }
        }

        [HttpPost("regulator/login")]
        public async Task<IActionResult> LoginRegulator([FromBody] AuthLoginDTO dto)
        {
            try
            {
                var token = await _service.LoginRegulatorAsync(dto);
                _logger.LogInformation("Regulator logged in successfully via API");
                return SetTokenAndRespond(token, "Login successful");
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid regulator login request");
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Regulator login failed");
                return Unauthorized(ApiResponse<object>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during regulator login");
                return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred during login"));
            }
        }

        // Get Current User
        [Authorize]
        [HttpGet("me")]
        public IActionResult Me()
        {
            try
            {
                string? GetFirstClaim(params string[] types) =>
                    types.Select(t => User.FindFirst(t)?.Value).FirstOrDefault(v => v is not null);

                var email = GetFirstClaim(ClaimTypes.Email, "email", JwtRegisteredClaimNames.Email);
                var role = GetFirstClaim(ClaimTypes.Role, "role");
                var userId = GetFirstClaim(ClaimTypes.NameIdentifier, "nameid", JwtRegisteredClaimNames.Sub);

                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("Me endpoint called with invalid token");
                    return Unauthorized(ApiResponse<object>.ErrorResponse("Invalid token"));
                }

                _logger.LogInformation("User {UserId} retrieved their profile", userId);

                return Ok(ApiResponse<object>.SuccessResponse(new { userId, email, role }, "User profile retrieved"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user profile");
                return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred while retrieving user profile"));
            }
        }

        // Logout
        [HttpPost("logout")]
        public IActionResult Logout()
        {
            try
            {
                Response.Cookies.Append("jwt_token", "", new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.None,
                    Expires = DateTime.UtcNow.AddDays(-1)
                });

                _logger.LogInformation("User logged out successfully");

                return Ok(ApiResponse.SuccessResponse("Logout successful"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during logout");
                return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred during logout"));
            }
        }
    }
}
