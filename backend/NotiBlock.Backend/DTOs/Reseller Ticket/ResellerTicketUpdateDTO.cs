using System.ComponentModel.DataAnnotations;
using NotiBlock.Backend.Models;

namespace NotiBlock.Backend.DTOs
{
    public class ResellerTicketUpdateDTO
    {
        [Required(ErrorMessage = "Category is required")]
        public TicketCategory Category { get; set; }

        [Required(ErrorMessage = "Description is required")]
        [StringLength(1000, MinimumLength = 10, ErrorMessage = "Description must be between 10 and 1000 characters")]
        public string Description { get; set; } = string.Empty;

        [Range(0, 3, ErrorMessage = "Priority must be between 0 and 3")]
        public int Priority { get; set; } = 0;
    }
}