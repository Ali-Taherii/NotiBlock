using Microsoft.AspNetCore.Mvc;
using NotiBlock.Backend.DTOs;
using NotiBlock.Backend.Interfaces;

namespace NotiBlock.Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RecallController(IRecallService service) : ControllerBase
    {
        private readonly IRecallService _service = service;

        [HttpPost]
        public async Task<IActionResult> Create ([FromBody] RecallCreateDto dto)
        {
            {
                try
                {
                    var recall = await _service.CreateRecallAsync(dto);
                    return Ok(new { success = true, message = "Recall created successfully", data = recall });
                }
                catch (ArgumentNullException ex)
                {
                    return BadRequest(new { success = false, message = ex.Message });
                }
                catch (InvalidOperationException ex)
                {
                    return BadRequest(new { success = false, message = ex.Message });
                }
                catch (Exception ex)
                {
                    return StatusCode(500, new { success = false, message = "An error occurred while creating the recall", error = ex.Message });
                }
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var recall = await _service.GetRecallByIdAsync(id);
                return Ok(new { success = true, message = "Recall retrieved successfully", data = recall });
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { success = false, message = ex.Message });
            }
        }

        [HttpGet("product/{productId}")]
        public async Task<IActionResult> GetByProductId(string productId)
        {
            try
            {
                var recall = await _service.GetRecallByProductIdAsync(productId);
                return Ok(new { success = true, message = "Recall retrieved successfully", data = recall });
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { success = false, message = ex.Message });
            }
        }

        [HttpGet("issuedAt/{issuedAt:datetime}")]
        public async Task<IActionResult> GetByIssueDate(DateTime issuedAt)
        {
            var recalls = await _service.GetRecallsByIssueDate(issuedAt);
            return Ok(new { success = true, message = "Recalls retrieved successfully", data = recalls });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var result = await _service.DeleteRecallByIdAsync(id);
                if (result is not null)
                    return Ok(new { success = true, message = "Recall deleted successfully", data = result });
                
                return NotFound(new { success = false, message = $"Recall with ID '{id}' not found." });
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                // Log the exception here
                return StatusCode(500, new { success = false, message = "An error occurred while deleting the recall", error = ex.Message });
            }
        }
        
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] RecallCreateDto dto)
        {
            try
            {
                var updatedRecall = await _service.UpdateRecallAsync(id, dto);
                return Ok(new { success = true, message = "Recall updated successfully", data = updatedRecall });
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { success = false, message = ex.Message });
            }
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var recalls = await _service.GetAllRecallsAsync();
                return Ok(new { success = true, message = "All recalls retrieved successfully", data = recalls });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "An error occurred while retrieving recalls", error = ex.Message });
            }
        }
    }
}