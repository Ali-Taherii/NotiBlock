using Microsoft.EntityFrameworkCore;
using NotiBlock.Backend.Data;
using NotiBlock.Backend.DTOs;
using NotiBlock.Backend.Interfaces;
using NotiBlock.Backend.Models;

namespace NotiBlock.Backend.Services {
public class RecallService : IRecallService
{
    private readonly AppDbContext _context;

    public RecallService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Recall> CreateRecallAsync(RecallCreateDto dto)
    {
        var recall = new Recall { ProductId = dto.ProductId, Reason = dto.Reason };
        _context.Recalls.Add(recall);
        await _context.SaveChangesAsync();
        return recall;
    }

    public async Task<IEnumerable<Recall>> GetAllRecallsAsync()
    {
        return await _context.Recalls.ToListAsync();
    }
}
}