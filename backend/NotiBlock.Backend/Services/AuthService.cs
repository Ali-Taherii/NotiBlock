using NotiBlock.Backend.Models;
using NotiBlock.Backend.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NotiBlock.Backend.DTOs;
using NotiBlock.Backend.Interfaces;
using Nethereum.Signer;

namespace NotiBlock.Backend.Services
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _context;
        private readonly IPasswordHasher<AppUser> _passwordHasher;

        public AuthService(AppDbContext context, IPasswordHasher<AppUser> passwordHasher)
        {
            _context = context;
            _passwordHasher = passwordHasher;
        }

        public async Task<AppUser> RegisterAsync(AuthDTO.AuthRegisterDto dto)
        {
            var existingUser = await _context.AppUsers
                .FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (existingUser != null)
                throw new Exception("User with this email already exists.");

            var user = new AppUser
                {
                    Email = dto.Email,
                    Role = dto.Role
                };

            user.PasswordHash = _passwordHasher.HashPassword(user, dto.Password);

            _context.AppUsers.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }


        public async Task<AppUser?> LoginAsync(AuthDTO.AuthLoginDto dto)
        {
            var user = await _context.AppUsers
                .FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (user == null)
                return null;
            var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, dto.Password);
            if (result == PasswordVerificationResult.Failed)
                return null;
            return user;
                
        } 
    }
}
