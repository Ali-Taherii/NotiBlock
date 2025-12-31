using NotiBlock.Backend.DTOs;
using NotiBlock.Backend.Models;

namespace NotiBlock.Backend.Interfaces
{
    public interface IConsumerReportService
    {
        // CRUD operations
        Task<ConsumerReport> SubmitReportAsync(ConsumerReportCreateDTO dto, Guid consumerId);
        Task<ConsumerReport> GetReportByIdAsync(Guid id);
        Task<ConsumerReport> UpdateReportAsync(Guid id, ConsumerReportUpdateDTO dto, Guid consumerId);
        Task<bool> DeleteReportAsync(Guid id, Guid consumerId);

        // List operations
        Task<PagedResultsDTO<ConsumerReport>> GetConsumerReportsAsync(Guid consumerId, int page, int pageSize);
        Task<PagedResultsDTO<ConsumerReport>> GetReportsByProductAsync(string serialNumber, int page, int pageSize);
        Task<PagedResultsDTO<ConsumerReport>> GetAllReportsAsync(int page, int pageSize);
        Task<PagedResultsDTO<ConsumerReport>> GetReportsByStatusAsync(ReportStatus status, int page, int pageSize);

        // Reseller actions
        Task<ConsumerReport> ProcessReportActionAsync(Guid reportId, ConsumerReportActionDTO dto, Guid resellerId);

        // Statistics
        Task<object> GetReportStatisticsAsync(Guid? consumerId = null);
    }
}
