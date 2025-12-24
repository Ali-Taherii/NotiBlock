using Microsoft.EntityFrameworkCore;
using NotiBlock.Backend.Data;
using NotiBlock.Backend.DTOs;
using NotiBlock.Backend.Interfaces;
using NotiBlock.Backend.Models;

namespace NotiBlock.Backend.Services
{
    public class ResellerTicketService(AppDbContext context) : IResellerTicketService
    {
        private readonly AppDbContext _context = context;

        public async Task<ResellerTicket> CreateTicketAsync(ResellerTicketDTO dto, Guid resellerId)
        {
            var exists = await _context.ResellerTickets.AnyAsync(t =>
                t.ResellerId == resellerId &&
                t.Category == dto.Category &&
                t.Status == "pending");

            if (exists) throw new Exception("An open ticket with this category already exists");

            var ticket = new ResellerTicket
            {
                ResellerId = resellerId,
                Category = dto.Category,
                Description = dto.Description
            };

            _context.ResellerTickets.Add(ticket);
            await _context.SaveChangesAsync();
            return ticket;
        }
    }
}
