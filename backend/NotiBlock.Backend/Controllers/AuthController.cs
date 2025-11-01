using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NotiBlock.Backend.DTOs;
using NotiBlock.Backend.Interfaces;

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
            return Ok(new { token });
        }

        [HttpPost("consumer/login")]
        public async Task<IActionResult> LoginConsumer([FromBody] AuthLoginDTO dto)
        {
            var token = await _service.LoginConsumerAsync(dto);
            return Ok(new { token });
        }


        // Reseller Endpoints
        [HttpPost("reseller/register")]
        public async Task<IActionResult> RegisterReseller([FromBody] AuthRegisterDTO dto)
        {
            var token = await _service.RegisterResellerAsync(dto);
            return Ok(new { token });
        }

        [HttpPost("reseller/login")]
        public async Task<IActionResult> LoginReseller([FromBody] AuthLoginDTO dto)
        {
            var token = await _service.LoginResellerAsync(dto);
            return Ok(new { token });
        }


        // Manufacturer Endpoints
        [HttpPost("manufacturer/register")]
        public async Task<IActionResult> RegisterManufacturer([FromBody] AuthRegisterDTO dto)
        {
            var token = await _service.RegisterManufacturerAsync(dto);
            return Ok(new { token });
        }

        [HttpPost("manufacturer/login")]
        public async Task<IActionResult> LoginManufacturer([FromBody] AuthLoginDTO dto)
        {
            var token = await _service.LoginManufacturerAsync(dto);
            return Ok(new { token });
        }


        // Regulator Endpoints
        [HttpPost("regulator/register")]
        public async Task<IActionResult> RegisterRegulator([FromBody] AuthRegisterDTO dto)
        {
            var token = await _service.RegisterRegulatorAsync(dto);
            return Ok(new { token });
        }

        [HttpPost("regulator/login")]
        public async Task<IActionResult> LoginRegulator([FromBody] AuthLoginDTO dto)
        {
            var token = await _service.LoginRegulatorAsync(dto);
            return Ok(new { token });
        }
    }
}
