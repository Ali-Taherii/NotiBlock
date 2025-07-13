using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NotiBlock.Backend.Data;
using NotiBlock.Backend.DTOs;
using NotiBlock.Backend.Interfaces;
using NotiBlock.Backend.Models;
using Org.BouncyCastle.Bcpg.OpenPgp;

namespace NotiBlock.Backend.Services
{
    public class RecallService(AppDbContext context) : IRecallService
    {
        private readonly AppDbContext _context = context;

        public async Task<Recall> CreateRecallAsync(RecallCreateDto dto)
        {
            var recall = new Recall { ProductId = dto.ProductId, Reason = dto.Reason, ActionRequired=dto.ActionRequired };
            _context.Recalls.Add(recall);
            await _context.SaveChangesAsync();
            return recall;
        }

        public async Task<IEnumerable<Recall>> GetAllRecallsAsync()
        {
            return await _context.Recalls.ToListAsync();
        }

        public async Task<Recall> GetRecallByIdAsync(int id)
        {
            var recall = await _context.Recalls.FindAsync(id);

            return recall ?? throw new InvalidOperationException($"Recall with ID '{id}' not found.");
        }

        public async Task<IEnumerable<Recall>> GetRecallsByIssueDate(DateTime issuedAt)
        {
            return await _context.Recalls
                .Where(r => r.IssuedAt.Date == issuedAt.Date)
                .ToListAsync();
        }

        public async Task<Recall> GetRecallByProductIdAsync(string productId)
        {
            var recall = await _context.Recalls
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
            catch(InvalidOperationException ex)
            {
                throw new InvalidOperationException($"Recall with ID '{id}' not found.", ex);
            }
        }

        public async Task<Recall> UpdateRecallAsync(int id, RecallCreateDto dto)
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