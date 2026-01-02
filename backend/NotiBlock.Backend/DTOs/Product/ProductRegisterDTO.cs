using System.ComponentModel.DataAnnotations;

namespace NotiBlock.Backend.DTOs.Product
{
    public class ProductRegisterDTO
    {
        [Required(ErrorMessage = "Serial number is required")]
        [StringLength(100, MinimumLength = 5, ErrorMessage = "Serial number must be between 5 and 100 characters")]
        public required string SerialNumber { get; set; }
        // No need for ManufacturerId, ResellerId, or OwnerId here; they come from JWT claims
    }
}