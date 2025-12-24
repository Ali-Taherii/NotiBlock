using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NotiBlock.Backend.DTOs;
using NotiBlock.Backend.Interfaces;
using System.Security.Claims;

namespace NotiBlock.Backend.Controllers
{
    [Authorize(Roles = "reseller")]
    [ApiController]
    [Route("api/reseller-tickets")]
    public class ResellerTicketController(IResellerTicketService service) : ControllerBase
    {
        private readonly IResellerTicketService _service = service;

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ResellerTicketDTO dto)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var ticket = await _service.CreateTicketAsync(dto, userId);
            return Ok(ticket);
        }
    }
}
