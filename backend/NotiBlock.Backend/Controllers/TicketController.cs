using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NotiBlock.Backend.DTOs;
using NotiBlock.Backend.Interfaces;
using NotiBlock.Backend.Models;
using System.Security.Claims;

namespace NotiBlock.Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TicketController(ITicketService service) : ControllerBase
    {
        private readonly ITicketService _service = service;

        [HttpPost]
        [Authorize(Roles = "consumer,reseller")]
        public async Task<IActionResult> CreateTicket([FromBody] TicketCreateDTO dto)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var ticket = await _service.CreateTicketAsync(dto, userId);
            return Ok(ticket);
        }

        [HttpGet]
        [Authorize(Roles = "Regulator")]
        public async Task<IActionResult> GetAll()
        {
            var tickets = await _service.GetAllTicketsAsync();
            return Ok(tickets);
        }

        [HttpPatch("{id}/approve")]
        [Authorize(Roles = "regulator")]
        public async Task<IActionResult> ApproveTicket(int id)
        {
            var regulatorId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await _service.ApproveTicketAsync(id, regulatorId);
            if (result == null) return NotFound("Ticket not found or already processed.");
            return Ok(result);
        }

        [HttpGet("user/{id}")]
        [Authorize]
        public async Task<IActionResult> GetTicketsByConsumerId(int id)
        {
            var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            
            // Check if the current user is requesting their own data or has admin/regulator role
            if (id != currentUserId && !User.IsInRole("regulator"))
            {
                return Forbid();
            }
            
            var tickets = await _service.GetTicketsByUserId(id);
            return Ok(tickets);
        }

        [HttpPut("edit/{ticketId}")]
        [Authorize(Roles="consumer,reseller")]
        public async Task<IActionResult> UpdateTicket(int ticketId, [FromBody] TicketCreateDTO dto)
        {
            var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var updatedTicket = await _service.UpdateTicketAsync(ticketId, dto, currentUserId);
            if (updatedTicket == null) return NotFound("Ticket not found or you are not authorized to update it.");
            return Ok(updatedTicket);

        }

        [HttpDelete("delete/{ticketId}")]
        [Authorize(Roles="consumer,reseller")]
        public async Task<IActionResult> DeleteTicket(int ticketId)
        {
            var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await _service.DeleteTicketAsync(ticketId, currentUserId);
            if (result == null) return NotFound("Ticket not found or you are not authorized to delete it.");
            return Ok(new { message = "Ticket deleted successfully." });
        }
    }

}
