using Microsoft.AspNetCore.Mvc;
using NotiBlock.Backend.DTOs;
using NotiBlock.Backend.Interfaces;

namespace NotiBlock.Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RecallController : ControllerBase
    {
        private readonly IRecallService _service;

        public RecallController(IRecallService service)
        {
            _service = service;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] RecallCreateDto dto)
        {
            var recall = await _service.CreateRecallAsync(dto);
            return Ok(recall);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var recalls = await _service.GetAllRecallsAsync();
            return Ok(recalls);
        }
    }
}