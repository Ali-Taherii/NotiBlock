using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NotiBlock.Backend.DTOs;
using NotiBlock.Backend.Interfaces;
using NotiBlock.Backend.Models;
using System.Security.Claims;

namespace NotiBlock.Backend.Controllers
{
    [ApiController]
    [Route("api/consumer-reports")]
    public class ConsumerReportController(IConsumerReportService service, ILogger<ConsumerReportController> logger) : ControllerBase
    {
        private readonly IConsumerReportService _service = service;
        private readonly ILogger<ConsumerReportController> _logger = logger;

        [HttpPost]
        [Authorize(Roles = "consumer")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Submit([FromForm] ConsumerReportCreateDTO dto)
        {
            try
            {
                var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var report = await _service.SubmitReportAsync(dto, userId);
                _logger.LogInformation("Report submitted successfully by consumer {UserId}", userId);
                return Ok(ApiResponse<object>.SuccessResponse(report, "Report submitted successfully"));
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized report submission");
                return StatusCode(403, ApiResponse<object>.ErrorResponse(ex.Message));
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Product not found for report");
                return NotFound(ApiResponse<object>.ErrorResponse(ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid report submission");
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting report");
                return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred while submitting the report"));
            }
        }

        [HttpPost("bulk")]
        [Authorize(Roles = "consumer")]
        public async Task<IActionResult> SubmitBulk([FromBody] BulkRequestDTO<ConsumerReportCreateDTO> dto)
        {
            try
            {
                var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var result = await _service.SubmitReportsBulkAsync(dto.Items, userId);

                _logger.LogInformation("Bulk report submit completed by consumer {UserId}. Success: {SuccessCount}, Failed: {FailedCount}",
                    userId, result.Succeeded, result.Failed);

                return Ok(ApiResponse<BulkOperationResultDTO<object>>.SuccessResponse(new BulkOperationResultDTO<object>
                {
                    Total = result.Total,
                    Succeeded = result.Succeeded,
                    Failed = result.Failed,
                    Results = result.Results.Select(r => new BulkOperationItemResultDTO<object>
                    {
                        Index = r.Index,
                        Success = r.Success,
                        Message = r.Message,
                        Data = r.Data
                    }).ToList()
                }, "Bulk report submission completed"));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid bulk report submission request");
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting reports in bulk");
                return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred while submitting reports"));
            }
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "consumer,reseller,regulator")]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                var report = await _service.GetReportByIdAsync(id);

                // Authorization: Consumers can only view their own reports
                var role = User.FindFirstValue(ClaimTypes.Role);
                if (role == "consumer")
                {
                    var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                    if (report.ConsumerId != userId)
                    {
                        _logger.LogWarning("Consumer {UserId} attempted to view report {ReportId} owned by another consumer",
                            userId, id);
                        return Forbid();
                    }
                }
                // Resellers can only view reports for their products
                else if (role == "reseller")
                {
                    var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                    if (report.Product?.ResellerId != userId)
                    {
                        _logger.LogWarning("Reseller {UserId} attempted to view report {ReportId} for product not assigned to them",
                            userId, id);
                        return Forbid();
                    }
                }

                _logger.LogInformation("Report {ReportId} retrieved successfully", id);
                return Ok(ApiResponse<object>.SuccessResponse(report));
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Report not found: {ReportId}", id);
                return NotFound(ApiResponse<object>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving report {ReportId}", id);
                return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred while retrieving the report"));
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "consumer")]
        public async Task<IActionResult> Update(Guid id, [FromBody] ConsumerReportUpdateDTO dto)
        {
            try
            {
                var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var report = await _service.UpdateReportAsync(id, dto, userId);
                _logger.LogInformation("Report {ReportId} updated successfully by consumer {UserId}", id, userId);
                return Ok(ApiResponse<object>.SuccessResponse(report, "Report updated successfully"));
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized update attempt on report {ReportId}", id);
                return StatusCode(403, ApiResponse<object>.ErrorResponse(ex.Message));
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Report not found for update: {ReportId}", id);
                return NotFound(ApiResponse<object>.ErrorResponse(ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid update operation on report {ReportId}", id);
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating report {ReportId}", id);
                return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred while updating the report"));
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "consumer")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                await _service.DeleteReportAsync(id, userId);
                _logger.LogInformation("Report {ReportId} deleted successfully by consumer {UserId}", id, userId);
                return Ok(ApiResponse.SuccessResponse("Report deleted successfully"));
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized delete attempt on report {ReportId}", id);
                return StatusCode(403, ApiResponse.ErrorResponse(ex.Message));
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Report not found for deletion: {ReportId}", id);
                return NotFound(ApiResponse.ErrorResponse(ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid delete operation on report {ReportId}", id);
                return BadRequest(ApiResponse.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting report {ReportId}", id);
                return StatusCode(500, ApiResponse.ErrorResponse("An error occurred while deleting the report"));
            }
        }

        [HttpGet("my-reports")]
        [Authorize(Roles = "consumer")]
        public async Task<IActionResult> GetMyReports([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            try
            {
                var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var result = await _service.GetConsumerReportsAsync(userId, page, pageSize);
                _logger.LogInformation("Consumer {UserId} retrieved their reports (Page {Page})", userId, page);
                return Ok(ApiResponse<PagedResultsDTO<ConsumerReportResponseDTO>>.SuccessResponse(result, $"Retrieved {result.Items.Count} reports"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving consumer reports");
                return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred while retrieving reports"));
            }
        }

        [HttpGet("product/{serialNumber}")]
        [Authorize(Roles = "reseller,regulator")]
        public async Task<IActionResult> GetByProduct(string serialNumber, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            try
            {
                var result = await _service.GetReportsByProductAsync(serialNumber, page, pageSize);
                _logger.LogInformation("Reports for product {SerialNumber} retrieved (Page {Page})", serialNumber, page);
                return Ok(ApiResponse<PagedResultsDTO<ConsumerReport>>.SuccessResponse(result, $"Retrieved {result.Items.Count} reports"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving reports by product");
                return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred while retrieving reports"));
            }
        }

        [HttpGet("all")]
        [Authorize(Roles = "regulator")]
        public async Task<IActionResult> GetAllReports([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            try
            {
                var result = await _service.GetAllReportsAsync(page, pageSize);
                _logger.LogInformation("All reports retrieved (Page {Page})", page);
                return Ok(ApiResponse<PagedResultsDTO<ConsumerReport>>.SuccessResponse(result, $"Retrieved {result.Items.Count} reports"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all reports");
                return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred while retrieving reports"));
            }
        }

        [HttpGet("status/{status}")]
        [Authorize(Roles = "reseller,regulator")]
        public async Task<IActionResult> GetByStatus(ReportStatus status, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            try
            {
                var result = await _service.GetReportsByStatusAsync(status, page, pageSize);
                _logger.LogInformation("Reports with status {Status} retrieved (Page {Page})", status, page);
                return Ok(ApiResponse<PagedResultsDTO<ConsumerReport>>.SuccessResponse(result, $"Retrieved {result.Items.Count} reports"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving reports by status");
                return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred while retrieving reports"));
            }
        }

        [HttpPost("{id}/action")]
        [Authorize(Roles = "reseller")]
        public async Task<IActionResult> ProcessAction(Guid id, [FromBody] ConsumerReportActionDTO dto)
        {
            try
            {
                var resellerId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var report = await _service.ProcessReportActionAsync(id, dto, resellerId);
                _logger.LogInformation("Report {ReportId} action {Action} processed by reseller {ResellerId}",
                    id, dto.Action, resellerId);
                return Ok(ApiResponse<object>.SuccessResponse(report, $"Report {dto.Action.ToString().ToLower()}d successfully"));
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized action on report {ReportId}", id);
                return StatusCode(403, ApiResponse<object>.ErrorResponse(ex.Message));
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Report not found for action: {ReportId}", id);
                return NotFound(ApiResponse<object>.ErrorResponse(ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid action on report {ReportId}", id);
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid action request for report {ReportId}", id);
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing action on report {ReportId}", id);
                return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred while processing the action"));
            }
        }

        [HttpGet("statistics")]
        [Authorize(Roles = "consumer,reseller,regulator")]
        public async Task<IActionResult> GetStatistics()
        {
            try
            {
                var role = User.FindFirstValue(ClaimTypes.Role);
                Guid? consumerId = null;

                if (role == "consumer")
                {
                    consumerId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                }

                var stats = await _service.GetReportStatisticsAsync(consumerId);
                _logger.LogInformation("Report statistics retrieved");
                return Ok(ApiResponse<object>.SuccessResponse(stats));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving report statistics");
                return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred while retrieving statistics"));
            }
        }

        [HttpGet("reseller/related-reports")]
        [Authorize(Roles = "reseller")]
        public async Task<IActionResult> GetResellerRelatedReports(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            try
            {
                var resellerId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var result = await _service.GetResellerRelatedReportsAsync(resellerId, page, pageSize);

                _logger.LogInformation(
                    "Retrieved {Count} related consumer reports for reseller {ResellerId}",
                    result.Items.Count,
                    resellerId
                );

                return Ok(ApiResponse<PagedResultsDTO<ConsumerReportResponseDTO>>.SuccessResponse(
                    result,
                    $"Retrieved {result.Items.Count} consumer reports for your products"
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving reseller-related consumer reports");
                return StatusCode(500, ApiResponse<object>.ErrorResponse(
                    "An error occurred while retrieving consumer reports"
                ));
            }
        }
    }
}
