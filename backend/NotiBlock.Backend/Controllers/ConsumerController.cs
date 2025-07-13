using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NotiBlock.Backend.DTOs;
using NotiBlock.Backend.Interfaces;

namespace NotiBlock.Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ConsumerController(IConsumerService service) : ControllerBase
    {
        private readonly IConsumerService _service = service;

        [HttpPost]
        public async Task<IActionResult> Register([FromBody] ConsumerCreateDTO dto)
        {
            var consumer = await _service.CreateConsumerAsync(dto);
            return Ok(consumer);
        }

        [HttpGet("{email}")]
        public async Task<IActionResult> GetByEmail(string email)
        {
            var consumer = await _service.GetConsumerByEmailAsync(email);
            if (consumer == null) return NotFound();
            return Ok(consumer);
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAll()
        {
            var consumers = await _service.GetAllConsumersAsync();
            return Ok(consumers);
        }
    }
}