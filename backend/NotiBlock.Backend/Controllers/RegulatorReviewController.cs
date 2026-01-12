using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NotiBlock.Backend.DTOs;
using NotiBlock.Backend.Interfaces;
using NotiBlock.Backend.Models;
using System.Security.Claims;

namespace NotiBlock.Backend.Controllers
{
    [ApiController]
    [Route("api/regulator-reviews")]
    [Authorize(Roles = "regulator")]
    public class RegulatorReviewController(IRegulatorReviewService service, ILogger<RegulatorReviewController> logger) : ControllerBase
    {
        private readonly IRegulatorReviewService _service = service;
        private readonly ILogger<RegulatorReviewController> _logger = logger;

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] RegulatorReviewCreateDTO dto)
        {
            try
            {
                var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var review = await _service.CreateReviewAsync(dto, userId);
                _logger.LogInformation("Review created successfully by regulator {UserId}", userId);
                return Ok(ApiResponse<object>.SuccessResponse(review, "Review submitted successfully"));
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Ticket not found for review");
                return NotFound(ApiResponse<object>.ErrorResponse(ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid review submission");
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating review");
                return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred while creating the review"));
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                var review = await _service.GetReviewByIdAsync(id);
                
                // Authorization: Regulators can only view their own reviews
                var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                if (review.RegulatorId != userId)
                {
                    _logger.LogWarning("Regulator {UserId} attempted to view review {ReviewId} created by another regulator",
                        userId, id);
                    return Forbid();
                }

                _logger.LogInformation("Review {ReviewId} retrieved successfully", id);
                return Ok(ApiResponse<object>.SuccessResponse(review));
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Review not found: {ReviewId}", id);
                return NotFound(ApiResponse<object>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving review {ReviewId}", id);
                return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred while retrieving the review"));
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] RegulatorReviewUpdateDTO dto)
        {
            try
            {
                var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var review = await _service.UpdateReviewAsync(id, dto, userId);
                _logger.LogInformation("Review {ReviewId} updated successfully by regulator {UserId}", id, userId);
                return Ok(ApiResponse<object>.SuccessResponse(review, "Review updated successfully"));
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized update attempt on review {ReviewId}", id);
                return StatusCode(403, ApiResponse<object>.ErrorResponse(ex.Message));
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Review not found for update: {ReviewId}", id);
                return NotFound(ApiResponse<object>.ErrorResponse(ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid update operation on review {ReviewId}", id);
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating review {ReviewId}", id);
                return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred while updating the review"));
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                await _service.DeleteReviewAsync(id, userId);
                _logger.LogInformation("Review {ReviewId} deleted successfully by regulator {UserId}", id, userId);
                return Ok(ApiResponse.SuccessResponse("Review deleted successfully"));
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized delete attempt on review {ReviewId}", id);
                return StatusCode(403, ApiResponse.ErrorResponse(ex.Message));
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Review not found for deletion: {ReviewId}", id);
                return NotFound(ApiResponse.ErrorResponse(ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid delete operation on review {ReviewId}", id);
                return BadRequest(ApiResponse.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting review {ReviewId}", id);
                return StatusCode(500, ApiResponse.ErrorResponse("An error occurred while deleting the review"));
            }
        }

        [HttpGet("my-reviews")]
        public async Task<IActionResult> GetMyReviews([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            try
            {
                var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var result = await _service.GetReviewsByRegulatorAsync(userId, page, pageSize);
                _logger.LogInformation("Regulator {UserId} retrieved their reviews (Page {Page})", userId, page);
                return Ok(ApiResponse<PagedResultsDTO<RegulatorReview>>.SuccessResponse(result, $"Retrieved {result.Items.Count} reviews"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving regulator reviews");
                return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred while retrieving reviews"));
            }
        }

        [HttpGet("ticket/{ticketId}")]
        public async Task<IActionResult> GetReviewsByTicket(Guid ticketId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            try
            {
                var result = await _service.GetReviewsByTicketAsync(ticketId, page, pageSize);
                _logger.LogInformation("Reviews for ticket {TicketId} retrieved (Page {Page})", ticketId, page);
                return Ok(ApiResponse<PagedResultsDTO<RegulatorReview>>.SuccessResponse(result, $"Retrieved {result.Items.Count} reviews"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving ticket reviews");
                return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred while retrieving reviews"));
            }
        }

        [HttpGet("pending-tickets")]
        public async Task<IActionResult> GetPendingTickets([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            try
            {
                var result = await _service.GetPendingTicketsAsync(page, pageSize);
                _logger.LogInformation("Pending tickets retrieved (Page {Page})", page);
                return Ok(ApiResponse<PagedResultsDTO<ResellerTicket>>.SuccessResponse(result, $"Retrieved {result.Items.Count} pending tickets"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving pending tickets");
                return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred while retrieving pending tickets"));
            }
        }

        [HttpPost("ticket/{ticketId}/escalate")]
        public async Task<IActionResult> EscalateToManufacturer(Guid ticketId)
        {
            try
            {
                var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var ticket = await _service.EscalateToManufacturerAsync(ticketId, userId);
                _logger.LogInformation("Ticket {TicketId} escalated to manufacturer by regulator {UserId}", ticketId, userId);
                return Ok(ApiResponse<object>.SuccessResponse(ticket, "Ticket escalated to manufacturer successfully"));
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized escalation attempt on ticket {TicketId}", ticketId);
                return StatusCode(403, ApiResponse<object>.ErrorResponse(ex.Message));
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Ticket not found for escalation: {TicketId}", ticketId);
                return NotFound(ApiResponse<object>.ErrorResponse(ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid escalation operation on ticket {TicketId}", ticketId);
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error escalating ticket {TicketId}", ticketId);
                return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred while escalating the ticket"));
            }
        }

        [HttpGet("stats")]
        public async Task<IActionResult> GetStatistics()
        {
            try
            {
                var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var stats = await _service.GetRegulatorStatsAsync(userId);
                _logger.LogInformation("Statistics retrieved for regulator {UserId}", userId);
                return Ok(ApiResponse<object>.SuccessResponse(stats, "Statistics retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving regulator statistics");
                return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred while retrieving statistics"));
            }
        }
    }
}