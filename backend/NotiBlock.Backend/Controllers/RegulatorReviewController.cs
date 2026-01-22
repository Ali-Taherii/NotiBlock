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
    public class RegulatorReviewController(IRegulatorReviewService service, ILogger<RegulatorReviewController> logger) : ControllerBase
    {
        private readonly IRegulatorReviewService _service = service;
        private readonly ILogger<RegulatorReviewController> _logger = logger;

        // ===== REGULATOR ENDPOINTS =====

        [HttpPost]
        [Authorize(Roles = "regulator")]
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
        [Authorize(Roles = "regulator")]
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
        [Authorize(Roles = "regulator")]
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
        [Authorize(Roles = "regulator")]
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
        [Authorize(Roles = "regulator")]
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
        [Authorize(Roles = "regulator,manufacturer")]
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
        [Authorize(Roles = "regulator")]
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
        [Authorize(Roles = "regulator")]
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
        [Authorize(Roles = "regulator")]
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

        // ===== MANUFACTURER ENDPOINTS =====

        /// <summary>
        /// Get approved tickets related to manufacturer's products
        /// </summary>
        [HttpGet("manufacturer/approved-tickets")]
        [Authorize(Roles = "manufacturer")]
        public async Task<IActionResult> GetApprovedTicketsForManufacturer([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            try
            {
                var manufacturerId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var result = await _service.GetApprovedTicketsForManufacturerAsync(manufacturerId, page, pageSize);
                
                _logger.LogInformation("Manufacturer {ManufacturerId} retrieved {Count} approved tickets (Page {Page})",
                    manufacturerId, result.Items.Count, page);
                
                return Ok(ApiResponse<PagedResultsDTO<ResellerTicket>>.SuccessResponse(result, 
                    $"Retrieved {result.Items.Count} approved tickets"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving approved tickets for manufacturer");
                return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred while retrieving approved tickets"));
            }
        }

        ///// <summary>
        ///// Get all tickets (any status) related to manufacturer's products
        ///// </summary>
        //[HttpGet("manufacturer/my-approved-tickets")]
        //[Authorize(Roles = "manufacturer")]
        //public async Task<IActionResult> GetApprovedTicketsForManufacturer([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        //{
        //    try
        //    {
        //        var manufacturerId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        //        var result = await _service.GetApprovedTicketsForManufacturerAsync(manufacturerId, page, pageSize);
                
        //        _logger.LogInformation("Manufacturer {ManufacturerId} retrieved {Count} tickets (Page {Page})",
        //            manufacturerId, result.Items.Count, page);
                
        //        return Ok(ApiResponse<PagedResultsDTO<ResellerTicket>>.SuccessResponse(result, 
        //            $"Retrieved {result.Items.Count} tickets"));
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error retrieving tickets for manufacturer");
        //        return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred while retrieving tickets"));
        //    }
        //}

        ///// <summary>
        ///// Get ticket details with reviews (for manufacturer)
        ///// </summary>
        //[HttpGet("manufacturer/tickets/{ticketId}")]
        //[Authorize(Roles = "manufacturer")]
        //public async Task<IActionResult> GetTicketDetailsForManufacturer(Guid ticketId)
        //{
        //    try
        //    {
        //        var manufacturerId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        //        var ticket = await _service.GetTicketDetailsForManufacturerAsync(ticketId, manufacturerId);
                
        //        _logger.LogInformation("Manufacturer {ManufacturerId} retrieved ticket {TicketId} details",
        //            manufacturerId, ticketId);
                
        //        return Ok(ApiResponse<object>.SuccessResponse(ticket, "Ticket details retrieved successfully"));
        //    }
        //    catch (UnauthorizedAccessException ex)
        //    {
        //        _logger.LogWarning(ex, "Manufacturer attempted to access unrelated ticket {TicketId}", ticketId);
        //        return StatusCode(403, ApiResponse<object>.ErrorResponse(ex.Message));
        //    }
        //    catch (KeyNotFoundException ex)
        //    {
        //        _logger.LogWarning(ex, "Ticket not found: {TicketId}", ticketId);
        //        return NotFound(ApiResponse<object>.ErrorResponse(ex.Message));
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error retrieving ticket {TicketId} for manufacturer", ticketId);
        //        return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred while retrieving ticket details"));
        //    }
        //}

        ///// <summary>
        ///// Get manufacturer-specific statistics about tickets
        ///// </summary>
        //[HttpGet("manufacturer/stats")]
        //[Authorize(Roles = "manufacturer")]
        //public async Task<IActionResult> GetManufacturerTicketStats()
        //{
        //    try
        //    {
        //        var manufacturerId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        //        var stats = await _service.GetManufacturerTicketStatsAsync(manufacturerId);
                
        //        _logger.LogInformation("Manufacturer {ManufacturerId} retrieved ticket statistics", manufacturerId);
                
        //        return Ok(ApiResponse<object>.SuccessResponse(stats, "Statistics retrieved successfully"));
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error retrieving manufacturer ticket statistics");
        //        return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred while retrieving statistics"));
        //    }
        //}
    }
}