using System.ComponentModel.DataAnnotations;

namespace NotiBlock.Backend.DTOs
{
    public class ProductUpdateDTO
    {
        [Required(ErrorMessage = "Model is required")]
        [StringLength(200, MinimumLength = 2, ErrorMessage = "Model must be between 2 and 200 characters")]
        public required string Model { get; set; }

        public Guid? ResellerId { get; set; }
        
        public Guid? OwnerId { get; set; }
    }
}
