using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NBitcoin.Secp256k1;
using NotiBlock.Backend.Data;
using NotiBlock.Backend.DTOs;
using NotiBlock.Backend.Interfaces;
using System.Security.Claims;

namespace NotiBlock.Backend.Controllers
{
    [ApiController]
    [Route("api/products")]
    public class ProductController(IProductService service) : ControllerBase
    {
        private readonly IProductService _service = service;

        [HttpPost("create")]
        [Authorize(Roles ="manufacturer")]
        public async Task<IActionResult> Create([FromBody] ProductCreateDTO dto)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await _service.CreateProductAsync(dto, userId);
            return Ok(result);
        }

        [HttpPost("register")]
        [Authorize(Roles = "consumer, reseller")]
        public async Task<IActionResult> Register([FromBody] ProductRegisterDTO dto)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await _service.RegisterProductAsync(dto, userId);
            return Ok(result);
        }

        [HttpGet("{serialNumber}")]
        public async Task<IActionResult> GetBySerialNumber(string serialNumber)
        {
            var result = await _service.GetProductBySerialNumberAsync(serialNumber);
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpPut("{serialNumber}")]
        public async Task<IActionResult> Update(string serialNumber, [FromBody] ProductRegisterDTO dto)
        {
            var product = await _service.GetProductBySerialNumberAsync(serialNumber);
            if (product == null) return NotFound();

            var updatedProduct = await _service.UpdateProductAsync(serialNumber, dto);
            return Ok(updatedProduct);
        }

        [HttpDelete("{serialNumber}")]
        public async Task<IActionResult> Delete(string serialNumber)
        {
            var product = await _service.GetProductBySerialNumberAsync(serialNumber);
            if (product == null) return NotFound();

            var deleted = await _service.DeleteProductAsync(serialNumber);
            if (!deleted) return NotFound();

            return NoContent();
        }
    }
}
