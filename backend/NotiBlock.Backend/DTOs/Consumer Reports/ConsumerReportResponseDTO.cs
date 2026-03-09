using Nethereum.Contracts.Standards.ENS.ENSRegistry.ContractDefinition;
using NotiBlock.Backend.Models;

namespace NotiBlock.Backend.DTOs
{

    public class ConsumerReportResponseDTO
    {
        public Guid Id { get; set; }
        public Guid ConsumerId { get; set; }
        public string ConsumerName { get; set; } = string.Empty;
        public string ConsumerEmail { get; set; } = string.Empty;

        public string SerialNumber { get; set; } = string.Empty;

        // product details for reseller context
        public ProductBasicInfoDTO? Product { get; set; }

        public string Description { get; set; } = string.Empty; public string? PhotoPath { get; set; }

        // Computed property for PhotoUrl
        public string? PhotoUrl
        {
            get
            {
                if (string.IsNullOrEmpty(PhotoPath))
                    return null;

                // Convert relative path to absolute URL
                // Path format: uploads/reports/filename.jpg
                // URL format: /uploads/reports/filename.jpg
                return $"/{PhotoPath.Replace("\\", "/")}";
            }
        }

        public ReportStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? ResolvedAt { get; set; }

        public Guid? ResellerTicketId { get; set; }
        public Guid? ResolvedBy { get; set; }
        public string? ResolutionNotes { get; set; }
    }

    public class ProductBasicInfoDTO
    {
        public Guid Id { get; set; }
        public string SerialNumber { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public string ManufacturerName { get; set; } = string.Empty;
    }
}