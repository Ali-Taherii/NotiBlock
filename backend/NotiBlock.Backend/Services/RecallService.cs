using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NotiBlock.Backend.Data;
using NotiBlock.Backend.DTOs;
using NotiBlock.Backend.Interfaces;
using NotiBlock.Backend.Models;
using Org.BouncyCastle.Bcpg.OpenPgp;
using System.Threading.Tasks;

namespace NotiBlock.Backend.Services
{
    public class RecallService(AppDbContext context) : IRecallService
    {
        private readonly AppDbContext _context = context;

        public async Task<Recall> CreateRecallAsync(RecallCreateDTO dto, int manufacturerId)
        {
            var recall = new Recall
            {
                ProductId = dto.ProductId,
                Reason = dto.Reason,
                ActionRequired = dto.ActionRequired,
                ManufacturerId = manufacturerId
            };

            _context.Recalls.Add(recall);
            await _context.SaveChangesAsync();
            return recall;
        }

        public async Task<Recall> IssueRecallToBlockchainAsync(int recallId)
        {
            var recall = await GetRecallByIdAsync(recallId);

            // Simulate blockchain integration
            // In a real implementation, this would interact with a blockchain service
            string transactionHash = await SimulateBlockchainRecordAsync(recall);

            recall.TransactionHash = transactionHash;
            _context.Recalls.Update(recall);
            await _context.SaveChangesAsync();

            return recall;
        }

        private static async Task<string> SimulateBlockchainRecordAsync(Recall recall)
        {
            // Simulate blockchain interaction
            // This would be replaced with actual blockchain service calls
            await Task.Delay(100); // Simulate network latency

            // Generate a fake transaction hash
            return $"0x{Guid.NewGuid():N}";
        }

        public async Task<IEnumerable<Recall>> GetAllRecallsAsync()
        {
            return await _context.Recalls
                //.Include(r => r.Manufacturer)
                .ToListAsync();
        }

        public async Task<Recall> GetRecallByIdAsync(int id)
        {
            var recall = await _context.Recalls
                //.Include(r => r.Manufacturer)
                .FirstOrDefaultAsync(r => r.Id == id);

            return recall ?? throw new InvalidOperationException($"Recall with ID '{id}' not found.");
        }

        public async Task<IEnumerable<Recall>> GetRecallsByIssueDate(DateTime issuedAt)
        {
            return await _context.Recalls
                //.Include(r => r.Manufacturer)
                .Where(r => r.IssuedAt.Date == issuedAt.Date)
                .ToListAsync();
        }

        public async Task<Recall> GetRecallByProductIdAsync(string productId)
        {
            var recall = await _context.Recalls
                //.Include(r => r.Manufacturer)
                .FirstOrDefaultAsync(r => r.ProductId == productId);

            return recall ?? throw new InvalidOperationException($"Recall for product ID '{productId}' not found.");
        }

        public async Task<Recall> DeleteRecallByIdAsync(int id)
        {
            try
            {
                var recall = await GetRecallByIdAsync(id);
                _context.Recalls.Remove(recall);
                await _context.SaveChangesAsync();
                return recall;

            }
            catch (InvalidOperationException ex)
            {
                throw new InvalidOperationException($"Recall with ID '{id}' not found.", ex);
            }
        }

        public async Task<Recall> UpdateRecallAsync(int id, RecallCreateDTO dto)
        {
            var recall = await GetRecallByIdAsync(id);
            recall.ProductId = dto.ProductId;
            recall.Reason = dto.Reason;
            recall.ActionRequired = dto.ActionRequired;
            _context.Recalls.Update(recall);
            await _context.SaveChangesAsync();
            return recall;
        }
    }
}