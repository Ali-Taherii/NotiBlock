using Microsoft.EntityFrameworkCore;
using NotiBlock.Backend.Data;
using NotiBlock.Backend.DTOs;
using NotiBlock.Backend.Interfaces;
using NotiBlock.Backend.Models;
using Org.BouncyCastle.Crypto;

namespace NotiBlock.Backend.Services
{
    public class TicketService(AppDbContext context) : ITicketService
    {
        private readonly AppDbContext _context = context;

        public async Task<Ticket> CreateTicketAsync(TicketCreateDTO dto, int userId)
        {
            var ticket = new Ticket
            {
                ProductId = dto.ProductId,
                IssueDescription = dto.IssueDescription,
                CreatedById = userId,
                CreatedAt = DateTime.UtcNow,
                Status = "Pending"
            };

            _context.Tickets.Add(ticket);
            await _context.SaveChangesAsync();
            return ticket;
        }

        public async Task<Ticket?> UpdateTicketAsync(int ticketId, TicketCreateDTO dto, int userId)
        {
            try
            {
                var ticket = await _context.Tickets.FindAsync(ticketId);
                if (ticket == null)
                {
                    Console.WriteLine("Ticket not found");
                    return null;

                }
                if (ticket.CreatedById != userId || ticket.Status != "Pending")
                {
                    Console.WriteLine("Unauthorized update attempt or ticket already processed");
                    return null;
                }
                ticket.ProductId = dto.ProductId;
                ticket.IssueDescription = dto.IssueDescription;
                ticket.CreatedAt = DateTime.UtcNow;
                _context.Tickets.Update(ticket);
                await _context.SaveChangesAsync();
                return ticket;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }

        }

        public async Task<IEnumerable<Ticket>> GetAllTicketsAsync()
        {
            return await _context.Tickets
                //.Include(t => t.CreatedBy)
                //.Include(t => t.ApprovedBy)
                .ToListAsync();
        }

        public async Task<Ticket?> UpdateTicketStatus(int ticketId, string newStatus, int regulatorId)
        {
            var ticket = await _context.Tickets.FindAsync(ticketId);
            if (ticket == null)
            {
                Console.WriteLine("Ticket not found");
                return null;
            }
            if (ticket.Status != "pending")
            {
                Console.WriteLine("Ticket already processed");
                return null;
            }
            ticket.Status = newStatus;
            ticket.ApprovedById = regulatorId;
            ticket.ApprovedAt = DateTime.UtcNow;
            _context.Tickets.Update(ticket);
            await _context.SaveChangesAsync();
            return ticket;
        }

        public async Task<IEnumerable<Ticket>> GetTicketByStatus(string status)
        {
            return await _context.Tickets
                .Where(ticket => ticket.Status.ToLower() == status.ToLower())
                //.Include(t => t.CreatedBy)
                //.Include(t => t.ApprovedBy)
                .ToListAsync();
        }

        public async Task<IEnumerable<Ticket>> GetTicketsByUserId(int userId)
        {
            return await _context.Tickets
                .Where(ticket => ticket.CreatedById == userId)
                //.Include(t => t.CreatedBy)
                //.Include(t => t.ApprovedBy)
                .ToListAsync();
        }

        public async Task<Ticket?> DeleteTicketAsync(int ticketId, int userId)
        {
            var ticket = await _context.Tickets.FindAsync(ticketId);
            if (ticket == null)
            {
                Console.WriteLine("Ticket not found");
                return null;
            }
            if (ticket.CreatedById != userId || ticket.Status != "Pending")
            {
                Console.WriteLine("Unauthorized delete attempt or ticket already processed");
                return null;
            }
            _context.Tickets.Remove(ticket);
            await _context.SaveChangesAsync();
            return ticket;
        }
    }
}
