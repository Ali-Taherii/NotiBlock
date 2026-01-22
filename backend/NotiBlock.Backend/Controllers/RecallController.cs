using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NotiBlock.Backend.DTOs;
using NotiBlock.Backend.Interfaces;
using System.Security.Claims;

namespace NotiBlock.Backend.Controllers
{
    [ApiController]
    [Route("api/recalls")]
    public class RecallController(IRecallService service, ILogger<RecallController> logger) : ControllerBase
    {
        private readonly IRecallService _service = service;
        private readonly ILogger<RecallController> _logger = logger;

        [HttpPost]
        [Authorize(Roles = "manufacturer")]
        public async Task<IActionResult> CreateRecall([FromBody] RecallCreateDTO dto)
        {
            try
            {
                var manufacturerId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var recall = await _service.CreateRecallAsync(dto, manufacturerId);
                _logger.LogInformation("Recall created successfully by manufacturer {ManufacturerId}", manufacturerId);

                return CreatedAtAction(nameof(GetRecallById), new { id = recall.Id }, 
                    ApiResponse<object>.SuccessResponse(recall, "Recall created successfully"));
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Manufacturer not found for recall creation");
                return NotFound(ApiResponse<object>.ErrorResponse(ex.Message));
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized recall creation");
                return StatusCode(403, ApiResponse<object>.ErrorResponse(ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid recall creation request");
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating recall");
                return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred while creating the recall"));
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetRecallById(Guid id)
        {
            try
            {
                var recall = await _service.GetRecallByIdAsync(id);

                if (recall == null)
                {
                    _logger.LogWarning("Recall not found: {RecallId}", id);
                    return NotFound(ApiResponse<object>.ErrorResponse("Recall not found"));
                }

                _logger.LogInformation("Recall {RecallId} retrieved successfully", id);
                return Ok(ApiResponse<object>.SuccessResponse(recall));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving recall {RecallId}", id);
                return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred while retrieving the recall"));
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAllRecalls([FromQuery] bool includeDeleted = false)
        {
            try
            {
                var recalls = await _service.GetAllRecallsAsync(includeDeleted);
                _logger.LogInformation("Retrieved all recalls successfully (includeDeleted: {IncludeDeleted})", includeDeleted);
                return Ok(ApiResponse<object>.SuccessResponse(recalls));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all recalls");
                return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred while retrieving recalls"));
            }
        }

        [HttpGet("manufacturer/{manufacturerId}")]
        public async Task<IActionResult> GetRecallsByManufacturer(Guid manufacturerId)
        {
            try
            {
                var recalls = await _service.GetRecallsByManufacturerAsync(manufacturerId);
                _logger.LogInformation("Retrieved recalls for manufacturer {ManufacturerId} successfully", manufacturerId);
                return Ok(ApiResponse<object>.SuccessResponse(recalls));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving recalls for manufacturer {ManufacturerId}", manufacturerId);
                return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred while retrieving recalls"));
            }
        }

        [HttpGet("manufacturer")]
        [Authorize(Roles = "manufacturer")]
        public async Task<IActionResult> GetMyRecalls()
        {
            try
            {
                var manufacturerId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var recalls = await _service.GetRecallsByManufacturerAsync(manufacturerId);
                _logger.LogInformation("Manufacturer {ManufacturerId} retrieved their recalls", manufacturerId);
                return Ok(ApiResponse<object>.SuccessResponse(recalls));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving manufacturer's recalls");
                return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred while retrieving your recalls"));
            }
        }

        // ===== BLOCKCHAIN INTEGRATION ENDPOINTS =====

        [HttpPost("{id}/issue-blockchain")]
        [Authorize(Roles = "manufacturer")]
        public async Task<IActionResult> IssueRecallToBlockchain(Guid id)
        {
            try
            {
                var manufacturerId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var blockchainData = await _service.IssueRecallToBlockchainAsync(id, manufacturerId);
                
                _logger.LogInformation("Recall {RecallId} issued to blockchain successfully. TxHash: {TxHash}", 
                    id, blockchainData.TransactionHash);
                
                return Ok(ApiResponse<RecallBlockchainDTO>.SuccessResponse(blockchainData, 
                    "Recall issued to blockchain successfully"));
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized attempt to issue recall to blockchain");
                return StatusCode(403, ApiResponse<object>.ErrorResponse(ex.Message));
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Recall not found: {RecallId}", id);
                return NotFound(ApiResponse<object>.ErrorResponse(ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation issuing recall to blockchain");
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error issuing recall to blockchain");
                return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred while issuing recall to blockchain"));
            }
        }

        [HttpPost("{id}/update-status-blockchain")]
        [Authorize(Roles = "manufacturer")]
        public async Task<IActionResult> UpdateRecallStatusOnBlockchain(Guid id, [FromBody] UpdateRecallStatusDTO dto)
        {
            try
            {
                var manufacturerId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var blockchainData = await _service.UpdateRecallStatusOnBlockchainAsync(id, dto.NewStatus, manufacturerId);
                
                _logger.LogInformation("Recall {RecallId} status updated on blockchain. NewStatus: {NewStatus}, TxHash: {TxHash}", 
                    id, dto.NewStatus, blockchainData.TransactionHash);
                
                return Ok(ApiResponse<RecallBlockchainDTO>.SuccessResponse(blockchainData, 
                    "Recall status updated on blockchain successfully"));
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized attempt to update recall status on blockchain");
                return StatusCode(403, ApiResponse<object>.ErrorResponse(ex.Message));
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Recall not found: {RecallId}", id);
                return NotFound(ApiResponse<object>.ErrorResponse(ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation updating recall status on blockchain");
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating recall status on blockchain");
                return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred while updating recall status"));
            }
        }

        [HttpGet("{id}/blockchain-data")]
        public async Task<IActionResult> GetRecallBlockchainData(Guid id)
        {
            try
            {
                var blockchainData = await _service.GetRecallBlockchainDataAsync(id);
                
                if (blockchainData == null)
                {
                    _logger.LogInformation("Recall {RecallId} has not been issued to blockchain yet", id);
                    return NotFound(ApiResponse<object>.ErrorResponse("Recall has not been issued to blockchain yet"));
                }

                _logger.LogInformation("Retrieved blockchain data for recall {RecallId}", id);
                return Ok(ApiResponse<RecallBlockchainDTO>.SuccessResponse(blockchainData));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving blockchain data for recall {RecallId}", id);
                return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred while retrieving blockchain data"));
            }
        }

        [HttpGet("{id}/verify-blockchain")]
        public async Task<IActionResult> VerifyRecallOnBlockchain(Guid id)
        {
            try
            {
                var isVerified = await _service.VerifyRecallOnBlockchainAsync(id);
                
                var message = isVerified ? "Recall verified on blockchain" : "Recall not verified on blockchain";
                var response = new { verified = isVerified, message = message };
                
                _logger.LogInformation("Recall {RecallId} blockchain verification: {IsVerified}", id, isVerified);
                return Ok(ApiResponse<object>.SuccessResponse(response));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying recall on blockchain");
                return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred while verifying recall"));
            }
        }

        // ===== EXISTING ENDPOINTS (Keep unchanged) =====

        [HttpGet("product/{productId}")]
        public async Task<IActionResult> GetRecallsByProduct(string productId)
        {
            try
            {
                var recalls = await _service.GetRecallsByProductAsync(productId);
                _logger.LogInformation("Retrieved recalls for product {ProductId}", productId);
                return Ok(ApiResponse<object>.SuccessResponse(recalls));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid product ID");
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving recalls for product {ProductId}", productId);
                return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred while retrieving recalls"));
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "manufacturer")]
        public async Task<IActionResult> UpdateRecall(Guid id, [FromBody] RecallUpdateDTO dto)
        {
            try
            {
                var manufacturerId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var updated = await _service.UpdateRecallAsync(id, dto, manufacturerId);

                if (updated == null)
                {
                    _logger.LogWarning("Recall not found for update: {RecallId}", id);
                    return NotFound(ApiResponse<object>.ErrorResponse("Recall not found"));
                }

                _logger.LogInformation("Recall {RecallId} updated successfully", id);
                return Ok(ApiResponse<object>.SuccessResponse(updated, "Recall updated successfully"));
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized update attempt on recall {RecallId}", id);
                return StatusCode(403, ApiResponse<object>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating recall {RecallId}", id);
                return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred while updating the recall"));
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "manufacturer")]
        public async Task<IActionResult> DeleteRecall(Guid id)
        {
            try
            {
                var manufacturerId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var deleted = await _service.SoftDeleteRecallAsync(id, manufacturerId);

                if (!deleted)
                {
                    _logger.LogWarning("Recall not found for deletion: {RecallId}", id);
                    return NotFound(ApiResponse<object>.ErrorResponse("Recall not found"));
                }

                _logger.LogInformation("Recall {RecallId} deleted successfully", id);
                return Ok(ApiResponse.SuccessResponse("Recall deleted successfully"));
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized delete attempt on recall {RecallId}", id);
                return StatusCode(403, ApiResponse<object>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting recall {RecallId}", id);
                return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred while deleting the recall"));
            }
        }

        [HttpPost("{id}/resolve")]
        [Authorize(Roles = "manufacturer")]
        public async Task<IActionResult> ResolveRecall(Guid id)
        {
            try
            {
                var manufacturerId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var resolved = await _service.ResolveRecallAsync(id, manufacturerId);

                if (!resolved)
                {
                    _logger.LogWarning("Recall not found for resolution: {RecallId}", id);
                    return NotFound(ApiResponse<object>.ErrorResponse("Recall not found"));
                }

                _logger.LogInformation("Recall {RecallId} resolved successfully", id);
                return Ok(ApiResponse.SuccessResponse("Recall resolved successfully"));
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized resolve attempt on recall {RecallId}", id);
                return StatusCode(403, ApiResponse<object>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resolving recall {RecallId}", id);
                return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred while resolving the recall"));
            }
        }
    }
}