using System.ComponentModel.DataAnnotations;

namespace NotiBlock.Backend.DTOs
{
    public class BulkRequestDTO<T>
    {
        [Required(ErrorMessage = "Items are required")]
        [MinLength(1, ErrorMessage = "At least one item is required")]
        public required List<T> Items { get; set; }
    }

    public class BulkOperationItemResultDTO<T>
    {
        public int Index { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
    }

    public class BulkOperationResultDTO<T>
    {
        public int Total { get; set; }
        public int Succeeded { get; set; }
        public int Failed { get; set; }
        public List<BulkOperationItemResultDTO<T>> Results { get; set; } = [];
    }
}