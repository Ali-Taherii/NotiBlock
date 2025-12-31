using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NotiBlock.Backend.DTOs;
using NotiBlock.Backend.Interfaces;
using NotiBlock.Backend.Models;
using System.Security.Claims;

namespace NotiBlock.Backend.Controllers
{
    [ApiController]
    [Route("api/reseller-tickets")]
    public class ResellerTicketController(IResellerTicketService service, ILogger<ResellerTicketController> logger) : ControllerBase
    {
        private readonly IResellerTicketService _service = service;
        private readonly ILogger<ResellerTicketController> _logger = logger;

        [HttpPost]
        [Authorize(Roles = "reseller")]
        public async Task<IActionResult> Create([FromBody] ResellerTicketDTO dto)
        {
            try
            {
                var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var ticket = await _service.CreateTicketAsync(dto, userId);
                _logger.LogInformation("Ticket created successfully by reseller {UserId}", userId);
                return Ok(ApiResponse<object>.SuccessResponse(ticket, "Ticket created successfully"));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid ticket creation request");
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating ticket");
                return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred while creating the ticket"));
            }
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "reseller,regulator")]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                var ticket = await _service.GetTicketByIdAsync(id);
                
                // Authorization: Resellers can only view their own tickets
                var role = User.FindFirstValue(ClaimTypes.Role);
                if (role == "reseller")
                {
                    var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                    if (ticket.ResellerId != userId)
                    {
                        _logger.LogWarning("Reseller {UserId} attempted to view ticket {TicketId} owned by another reseller",
                            userId, id);
                        return Forbid();
                    }
                }

                _logger.LogInformation("Ticket {TicketId} retrieved successfully", id);
                return Ok(ApiResponse<object>.SuccessResponse(ticket));
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Ticket not found: {TicketId}", id);
                return NotFound(ApiResponse<object>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving ticket {TicketId}", id);
                return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred while retrieving the ticket"));
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "reseller")]
        public async Task<IActionResult> Update(Guid id, [FromBody] ResellerTicketUpdateDTO dto)
        {
            try
            {
                var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var ticket = await _service.UpdateTicketAsync(id, dto, userId);
                _logger.LogInformation("Ticket {TicketId} updated successfully by reseller {UserId}", id, userId);
                return Ok(ApiResponse<object>.SuccessResponse(ticket, "Ticket updated successfully"));
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized update attempt on ticket {TicketId}", id);
                return StatusCode(403, ApiResponse<object>.ErrorResponse(ex.Message));
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Ticket not found for update: {TicketId}", id);
                return NotFound(ApiResponse<object>.ErrorResponse(ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid update operation on ticket {TicketId}", id);
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating ticket {TicketId}", id);
                return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred while updating the ticket"));
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "reseller")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                await _service.DeleteTicketAsync(id, userId);
                _logger.LogInformation("Ticket {TicketId} deleted successfully by reseller {UserId}", id, userId);
                return Ok(ApiResponse.SuccessResponse("Ticket deleted successfully"));
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized delete attempt on ticket {TicketId}", id);
                return StatusCode(403, ApiResponse.ErrorResponse(ex.Message));
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Ticket not found for deletion: {TicketId}", id);
                return NotFound(ApiResponse.ErrorResponse(ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid delete operation on ticket {TicketId}", id);
                return BadRequest(ApiResponse.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting ticket {TicketId}", id);
                return StatusCode(500, ApiResponse.ErrorResponse("An error occurred while deleting the ticket"));
            }
        }

        [HttpGet("my-tickets")]
        [Authorize(Roles = "reseller")]
        public async Task<IActionResult> GetMyTickets([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            try
            {
                var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var result = await _service.GetResellerTicketsAsync(userId, page, pageSize);
                _logger.LogInformation("Reseller {UserId} retrieved their tickets (Page {Page})", userId, page);
                return Ok(ApiResponse<PagedResultsDTO<ResellerTicket>>.SuccessResponse(result, $"Retrieved {result.Items.Count} tickets"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving reseller tickets");
                return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred while retrieving tickets"));
            }
        }

        [HttpGet("all")]
        [Authorize(Roles = "regulator")]
        public async Task<IActionResult> GetAllTickets([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            try
            {
                var result = await _service.GetAllTicketsAsync(page, pageSize);
                _logger.LogInformation("All tickets retrieved (Page {Page})", page);
                return Ok(ApiResponse<PagedResultsDTO<ResellerTicket>>.SuccessResponse(result, $"Retrieved {result.Items.Count} tickets"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all tickets");
                return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred while retrieving tickets"));
            }
        }

        [HttpGet("status/{status}")]
        [Authorize(Roles = "regulator")]
        public async Task<IActionResult> GetByStatus(TicketStatus status, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            try
            {
                var result = await _service.GetTicketsByStatusAsync(status, page, pageSize);
                _logger.LogInformation("Tickets with status {Status} retrieved (Page {Page})", status, page);
                return Ok(ApiResponse<PagedResultsDTO<ResellerTicket>>.SuccessResponse(result, $"Retrieved {result.Items.Count} tickets"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving tickets by status");
                return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred while retrieving tickets"));
            }
        }

        [HttpPost("{id}/action")]
        [Authorize(Roles = "regulator")]
        public async Task<IActionResult> ProcessAction(Guid id, [FromBody] TicketActionDTO dto)
        {
            try
            {
                var regulatorId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var ticket = await _service.ProcessTicketActionAsync(id, dto, regulatorId);
                _logger.LogInformation("Ticket {TicketId} action {Action} processed by regulator {RegulatorId}",
                    id, dto.Action, regulatorId);
                return Ok(ApiResponse<object>.SuccessResponse(ticket, $"Ticket {dto.Action.ToString().ToLower()}ed successfully"));
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Ticket not found for action: {TicketId}", id);
                return NotFound(ApiResponse<object>.ErrorResponse(ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid action on ticket {TicketId}", id);
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing action on ticket {TicketId}", id);
                return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred while processing the action"));
            }
        }

        [HttpGet("statistics")]
        [Authorize(Roles = "reseller,regulator")]
        public async Task<IActionResult> GetStatistics()
        {
            try
            {
                var role = User.FindFirstValue(ClaimTypes.Role);
                Guid? resellerId = null;

                if (role == "reseller")
                {
                    resellerId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                }

                var stats = await _service.GetTicketStatisticsAsync(resellerId);
                _logger.LogInformation("Ticket statistics retrieved");
                return Ok(ApiResponse<object>.SuccessResponse(stats));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving ticket statistics");
                return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred while retrieving statistics"));
            }
        }

        [HttpGet("readable")]
        [Authorize(Roles = "reseller,regulator")]
        public async Task<IActionResult> GetReadableTickets([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            try
            {
                var role = User.FindFirstValue(ClaimTypes.Role);
                Guid? resellerId = null;

                // Resellers can only see their own tickets
                if (role == "reseller")
                {
                    resellerId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                }

                var result = await _service.GetReadableTicketsAsync(resellerId, page, pageSize);
                _logger.LogInformation("Readable tickets retrieved (Page {Page})", page);
                return Ok(ApiResponse<PagedResultsDTO<ResellerTicketReadableView>>.SuccessResponse(
                    result, 
                    $"Retrieved {result.Items.Count} tickets"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving readable tickets");
                return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred while retrieving tickets"));
            }
        }
    }
}
