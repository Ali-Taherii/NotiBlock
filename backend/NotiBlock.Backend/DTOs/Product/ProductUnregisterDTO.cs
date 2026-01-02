using System.ComponentModel.DataAnnotations;

namespace NotiBlock.Backend.DTOs.Product
{
    public class ProductUnregisterDTO
    {
        [Required(ErrorMessage = "Serial number is required")]
        [StringLength(100, MinimumLength = 5, ErrorMessage = "Serial number must be between 5 and 100 characters")]
        public required string SerialNumber { get; set; }

        [Required(ErrorMessage = "Unregister type is required")]
        public required UnregisterType Type { get; set; }
    }

    public enum UnregisterType
    {
        RemoveReseller,   // Manufacturer removes reseller assignment (0)
        RemoveConsumer    // Manufacturer or Reseller removes consumer assignment (1)
    }
}