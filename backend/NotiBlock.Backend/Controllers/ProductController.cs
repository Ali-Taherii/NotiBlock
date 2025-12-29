using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NotiBlock.Backend.DTOs;
using NotiBlock.Backend.Models;
using NotiBlock.Backend.Interfaces;
using System.Security.Claims;

namespace NotiBlock.Backend.Controllers
{
    [ApiController]
    [Route("api/products")]
    public class ProductController(IProductService service, ILogger<ProductController> logger) : ControllerBase
    {
        private readonly IProductService _service = service;
        private readonly ILogger<ProductController> _logger = logger;

        [HttpPost("create")]
        [Authorize(Roles = "manufacturer")]
        public async Task<IActionResult> Create([FromBody] ProductCreateDTO dto)
        {
            try
            {
                var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var result = await _service.CreateProductAsync(dto, userId);
                _logger.LogInformation("Product created successfully by user {UserId}", userId);
                return Ok(ApiResponse<object>.SuccessResponse(result, "Product created successfully"));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid product creation request");
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating product");
                return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred while creating the product"));
            }
        }

        [HttpPost("register")]
        [Authorize(Roles = "consumer,reseller")]
        public async Task<IActionResult> Register([FromBody] ProductRegisterDTO dto)
        {
            try
            {
                var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var role = User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;
                var result = await _service.RegisterProductAsync(dto, userId, role);
                _logger.LogInformation("Product registered successfully by user {UserId} with role {Role}", userId, role);
                return Ok(ApiResponse<object>.SuccessResponse(result, "Product registered successfully"));
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Product not found for registration");
                return NotFound(ApiResponse<object>.ErrorResponse(ex.Message));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid product registration request");
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering product");
                return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred while registering the product"));
            }
        }

        [HttpPost("unregister")]
        [Authorize(Roles = "manufacturer,reseller")]
        public async Task<IActionResult> Unregister([FromBody] ProductUnregisterDTO dto)
        {
            try
            {
                var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var role = User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;
                var result = await _service.UnregisterProductAsync(dto, userId, role);
                
                string message = dto.Type == UnregisterType.RemoveReseller 
                    ? "Reseller removed from product successfully" 
                    : "Consumer removed from product successfully";
                
                _logger.LogInformation("Product unregistered successfully by user {UserId} with role {Role}", userId, role);
                return Ok(ApiResponse<object>.SuccessResponse(result, message));
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized unregister attempt");
                return StatusCode(403, ApiResponse<object>.ErrorResponse(ex.Message));
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Product not found for unregistration");
                return NotFound(ApiResponse<object>.ErrorResponse(ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid unregister operation");
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid unregister request");
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error unregistering product");
                return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred while unregistering the product"));
            }
        }

        [HttpGet("{serialNumber}")]
        public async Task<IActionResult> GetBySerialNumber(string serialNumber)
        {
            try
            {
                var result = await _service.GetProductBySerialNumberAsync(serialNumber);
                _logger.LogInformation("Product retrieved successfully with serial number {SerialNumber}", serialNumber);
                return Ok(ApiResponse<object>.SuccessResponse(result));
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Product not found with serial number {SerialNumber}", serialNumber);
                return NotFound(ApiResponse<object>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving product with serial number {SerialNumber}", serialNumber);
                return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred while retrieving the product"));
            }
        }

        [HttpPut("{serialNumber}")]
        [Authorize(Roles = "manufacturer,reseller")]
        public async Task<IActionResult> Update(string serialNumber, [FromBody] ProductUpdateDTO dto)
        {
            try
            {
                var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var role = User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;

                var updatedProduct = await _service.UpdateProductAsync(serialNumber, dto, userId, role);
                _logger.LogInformation("Product {SerialNumber} updated successfully by user {UserId}", serialNumber, userId);
                return Ok(ApiResponse<object>.SuccessResponse(updatedProduct, "Product updated successfully"));
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized update attempt on product {SerialNumber}", serialNumber);
                return StatusCode(403, ApiResponse<object>.ErrorResponse(ex.Message));
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Product not found for update: {SerialNumber}", serialNumber);
                return NotFound(ApiResponse<object>.ErrorResponse(ex.Message));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid product update request for {SerialNumber}", serialNumber);
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating product {SerialNumber}", serialNumber);
                return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred while updating the product"));
            }
        }

        [HttpDelete("{serialNumber}")]
        [Authorize(Roles = "manufacturer")]
        public async Task<IActionResult> Delete(string serialNumber)
        {
            try
            {
                var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var result = await _service.DeleteProductAsync(serialNumber, userId);

                _logger.LogInformation("Product {SerialNumber} deleted successfully by manufacturer {UserId}", serialNumber, userId);
                return Ok(ApiResponse.SuccessResponse("Product deleted successfully"));
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized delete attempt on product {SerialNumber}", serialNumber);
                return StatusCode(403, ApiResponse.ErrorResponse(ex.Message));
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Product not found for deletion: {SerialNumber}", serialNumber);
                return NotFound(ApiResponse.ErrorResponse(ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid delete operation on product {SerialNumber}", serialNumber);
                return BadRequest(ApiResponse.ErrorResponse(ex.Message));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid delete request for {SerialNumber}", serialNumber);
                return BadRequest(ApiResponse.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting product {SerialNumber}", serialNumber);
                return StatusCode(500, ApiResponse.ErrorResponse("An error occurred while deleting the product"));
            }
        }

        // List endpoints
        [HttpGet("manufacturer/my-products")]
        [Authorize(Roles = "manufacturer")]
        public async Task<IActionResult> GetMyProducts([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            try
            {
                var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var result = await _service.GetManufacturerProductsAsync(userId, page, pageSize);

                _logger.LogInformation("Manufacturer {UserId} retrieved their products (Page {Page})", userId, page);
                return Ok(ApiResponse<PagedResultsDTO<Product>>.SuccessResponse(result, $"Retrieved {result.Items.Count} products"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving manufacturer products");
                return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred while retrieving products"));
            }
        }

        [HttpGet("reseller/my-products")]
        [Authorize(Roles = "reseller")]
        public async Task<IActionResult> GetMyResellerProducts([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            try
            {
                var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var result = await _service.GetResellerProductsAsync(userId, page, pageSize);

                _logger.LogInformation("Reseller {UserId} retrieved their products (Page {Page})", userId, page);
                return Ok(ApiResponse<PagedResultsDTO<Product>>.SuccessResponse(result, $"Retrieved {result.Items.Count} products"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving reseller products");
                return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred while retrieving products"));
            }
        }

        [HttpGet("consumer/my-products")]
        [Authorize(Roles = "consumer")]
        public async Task<IActionResult> GetMyConsumerProducts([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            try
            {
                var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var result = await _service.GetConsumerProductsAsync(userId, page, pageSize);

                _logger.LogInformation("Consumer {UserId} retrieved their products (Page {Page})", userId, page);
                return Ok(ApiResponse<PagedResultsDTO<Product>>.SuccessResponse(result, $"Retrieved {result.Items.Count} products"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving consumer products");
                return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred while retrieving products"));
            }
        }
    }
}
