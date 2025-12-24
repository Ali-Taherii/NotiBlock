using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NotiBlock.Backend.DTOs;
using NotiBlock.Backend.Interfaces;
using System.Security.Claims;

namespace NotiBlock.Backend.Controllers
{
    [Authorize(Roles = "Consumer")]
    [ApiController]
    [Route("api/[controller]")]
    public class ConsumerReportController(IConsumerReportService service) : ControllerBase
    {
        private readonly IConsumerReportService _service = service;

        [HttpPost]
        public async Task<IActionResult> Submit([FromBody] ConsumerReportDTO dto)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var report = await _service.SubmitReportAsync(dto, userId);
            return Ok(report);
        }
    }
}
