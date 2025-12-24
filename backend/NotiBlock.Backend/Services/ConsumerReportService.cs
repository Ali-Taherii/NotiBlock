using NotiBlock.Backend.Interfaces;
using NotiBlock.Backend.Models;
using NotiBlock.Backend.DTOs;
using NotiBlock.Backend.Data;

namespace NotiBlock.Backend.Services
{
    public class ConsumerReportService : IConsumerReportService
    {
        private readonly AppDbContext _context;
        public ConsumerReportService(AppDbContext context) => _context = context;

        public async Task<ConsumerReport> SubmitReportAsync(ConsumerReportDTO dto, Guid consumerId)
        {
            var report = new ConsumerReport
            { 
                ProductId = dto.ProductId,
                Description = dto.IssueDescription,
                ConsumerId = consumerId,
                CreatedAt = DateTime.UtcNow
            };
            _context.ConsumerReports.Add(report);
            await _context.SaveChangesAsync();
            return report;
        }

    }
}
