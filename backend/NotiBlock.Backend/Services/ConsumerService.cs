using NotiBlock.Backend.Data;
using NotiBlock.Backend.DTOs;
using NotiBlock.Backend.Interfaces;
using NotiBlock.Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace NotiBlock.Backend.Services
{
    public class ConsumerService(AppDbContext context) : IConsumerService
    {
        private readonly AppDbContext _context = context;

        //public async Task<Consumer> CreateConsumerAsync(ConsumerCreateDTO dto)
        //{
        //    var existing = await _context.Consumers
        //        .FirstOrDefaultAsync(c => c.Email == dto.Email);

        //    if (existing != null)
        //    {
        //        return existing; // Already registered
        //    }

        //    var consumer = new Consumer
        //    {
        //        Email = dto.Email,
        //        Name = dto.Name,
        //        PhoneNumber = dto.PhoneNumber
        //    };

        //    _context.Consumers.Add(consumer);
        //    await _context.SaveChangesAsync();

        //    return consumer;
        //}


        //public async Task<IEnumerable<Consumer>> GetAllConsumersAsync()
        //{
        //    return await _context.Consumers
        //        //.Include(c => c.Responses)
        //        .ToListAsync();
        //}

        //public async Task<Consumer> GetConsumerByEmailAsync(string email)
        //{
        //    var consumer = await _context.Consumers
        //        //.Include(c => c.Responses)
        //        .FirstOrDefaultAsync(c => c.Email == email);

        //    return consumer ?? throw new InvalidOperationException($"Consumer with email '{email}' not found.");
        //}
    }
}