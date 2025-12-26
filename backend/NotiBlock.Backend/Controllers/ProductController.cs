using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
        [Authorize(Roles = "manufacturer")]
        public async Task<IActionResult> Create([FromBody] ProductCreateDTO dto)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await _service.CreateProductAsync(dto, userId);
            return Ok(ApiResponse<object>.SuccessResponse(result, "Product created successfully"));
        }

        [HttpPost("register")]
        [Authorize(Roles = "consumer, reseller")]
        public async Task<IActionResult> Register([FromBody] ProductRegisterDTO dto)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var role = User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;
            var result = await _service.RegisterProductAsync(dto, userId, role);
            return Ok(ApiResponse<object>.SuccessResponse(result, "Product registered successfully"));
        }

        [HttpGet("{serialNumber}")]
        public async Task<IActionResult> GetBySerialNumber(string serialNumber)
        {
            var result = await _service.GetProductBySerialNumberAsync(serialNumber);
            if (result == null)
                return NotFound(ApiResponse<object>.ErrorResponse("Product not found"));

            return Ok(ApiResponse<object>.SuccessResponse(result));
        }

        [HttpPut("{serialNumber}")]
        [Authorize(Roles = "manufacturer, reseller")]
        public async Task<IActionResult> Update(string serialNumber, [FromBody] ProductUpdateDTO dto)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var role = User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;

            var updatedProduct = await _service.UpdateProductAsync(serialNumber, dto, userId, role);
            return Ok(ApiResponse<object>.SuccessResponse(updatedProduct, "Product updated successfully"));
        }

        [HttpDelete("{serialNumber}")]
        [Authorize(Roles = "manufacturer")]
        public async Task<IActionResult> Delete(string serialNumber)
        {
            var product = await _service.GetProductBySerialNumberAsync(serialNumber);
            if (product == null)
                return NotFound(ApiResponse.ErrorResponse("Product not found"));

            var deleted = await _service.DeleteProductAsync(serialNumber);
            if (!deleted)
                return NotFound(ApiResponse.ErrorResponse("Product could not be deleted"));

            return Ok(ApiResponse.SuccessResponse("Product deleted successfully"));
        }
    }
}
