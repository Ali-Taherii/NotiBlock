using System.ComponentModel.DataAnnotations;

namespace NotiBlock.Backend.DTOs.Product
{
    // For manufacturer creating a product
    public class ProductCreateDTO
    {
        [Required(ErrorMessage = "Serial number is required")]
        [StringLength(100, MinimumLength = 5, ErrorMessage = "Serial number must be between 5 and 100 characters")]
        [RegularExpression(@"^[A-Z0-9\-]+$", ErrorMessage = "Serial number can only contain uppercase letters, numbers, and hyphens")]
        public required string SerialNumber { get; set; }

        [Required(ErrorMessage = "Model is required")]
        [StringLength(200, MinimumLength = 2, ErrorMessage = "Model must be between 2 and 200 characters")]
        public required string Model { get; set; }
        // ManufacturerId comes from JWT claims
    }
}
