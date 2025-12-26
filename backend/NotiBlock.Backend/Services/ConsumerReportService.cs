using NotiBlock.Backend.Interfaces;
using NotiBlock.Backend.Models;
using NotiBlock.Backend.DTOs;
using NotiBlock.Backend.Data;

namespace NotiBlock.Backend.Services
{
    public class ConsumerReportService(AppDbContext context) : IConsumerReportService
    {
        public async Task<ConsumerReport> SubmitReportAsync(ConsumerReportDTO dto, Guid consumerId)
        {
            var report = new ConsumerReport
            { 
                ProductId = dto.ProductId,
                Description = dto.IssueDescription,
                ConsumerId = consumerId,
                CreatedAt = DateTime.UtcNow
            };
            context.ConsumerReports.Add(report);
            await context.SaveChangesAsync();
            return report;
        }

    }
}
