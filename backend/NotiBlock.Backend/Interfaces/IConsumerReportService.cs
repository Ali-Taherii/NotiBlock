using NotiBlock.Backend.DTOs;
using NotiBlock.Backend.Models;

namespace NotiBlock.Backend.Interfaces
{
    public interface IConsumerReportService
    {
        Task<ConsumerReport> SubmitReportAsync(ConsumerReportDTO dto, Guid consumerId);
    }
}
