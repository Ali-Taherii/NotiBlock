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
        public async Task<IActionResult> Create([FromBody] RecallCreateDto dto)
        {
            var recall = await _service.CreateRecallAsync(dto);
            return Ok(recall);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
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
        }

        [HttpGet("product/{productId}")]
        public async Task<IActionResult> GetByProductId(string productId)
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
        }

        [HttpGet("issuedAt/{issuedAt:datetime}")]
        public async Task<IActionResult> GetByIssueDate(DateTime issuedAt)
        {
            var recalls = await _service.GetRecallsByIssueDate(issuedAt);
            return Ok(recalls);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _service.DeleteRecallByIdAsync(id);
            return result is not null ? Ok(result) : NotFound($"Recall with ID '{id}' not found.");
        }
        

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] RecallCreateDto dto)
        {
            try
            {
                var updatedRecall = await _service.UpdateRecallAsync(id, dto);
                return Ok(updatedRecall);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAll()
        {
            var recalls = await _service.GetAllRecallsAsync();
            return Ok(recalls);
        }

    }
}