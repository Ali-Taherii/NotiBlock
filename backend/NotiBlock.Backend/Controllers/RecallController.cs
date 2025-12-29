using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NotiBlock.Backend.DTOs;
using NotiBlock.Backend.Interfaces;
using System.Security.Claims;

namespace NotiBlock.Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RecallController(IRecallService service) : ControllerBase
    {
        private readonly IRecallService _service = service;

        [HttpPost]
        [Authorize(Roles = "manufacturer")]
        public async Task<IActionResult> CreateRecall([FromBody] RecallCreateDTO dto)
        {
            try
            {
                var manufacturerIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(manufacturerIdStr))
                {
                    return BadRequest("Manufacturer identifier is missing.");
                }
                var manufacturerId = Guid.Parse(manufacturerIdStr);
                var recall = await _service.CreateRecallAsync(dto, manufacturerId);

                // Issue recall to blockchain
                recall = await _service.IssueRecallToBlockchainAsync(recall.Id);

                return Ok(recall);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAllRecalls()
        {
            try
            {
                var recalls = await _service.GetAllRecallsAsync();
                return Ok(new { success = true, data = recalls });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetRecallById(int id)
        {
            try
            {
                var recall = await _service.GetRecallByIdAsync(id);
                return Ok(recall);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("product/{productId}")]
        public async Task<IActionResult> GetRecallByProductId(string productId)
        {
            try
            {
                var recall = await _service.GetRecallByProductIdAsync(productId);
                return Ok(recall);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "manufacturer")]
        public async Task<IActionResult> DeleteRecall(int id)
        {
            try
            {
                var manufacturerId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var recall = await _service.DeleteRecallByIdAsync(id);
                return Ok(recall);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "manufacturer")]
        public async Task<IActionResult> UpdateRecall(int id, [FromBody] RecallCreateDTO dto)
        {
            try
            {
                var manufacturerId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var recall = await _service.UpdateRecallAsync(id, dto);
                return Ok(recall);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("date/{issuedAt}")]
        public async Task<IActionResult> GetRecallsByIssueDate(DateTime issuedAt)
        {
            try
            {
                var recalls = await _service.GetRecallsByIssueDate(issuedAt);
                return Ok(recalls);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}