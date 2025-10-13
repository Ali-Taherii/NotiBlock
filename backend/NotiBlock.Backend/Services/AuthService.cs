using NotiBlock.Backend.Models;
using NotiBlock.Backend.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NotiBlock.Backend.DTOs;
using NotiBlock.Backend.Interfaces;
using Nethereum.Signer;
using NotiBlock.Backend.Helpers;

namespace NotiBlock.Backend.Services
{
    public class AuthService(AppDbContext context, IPasswordHasher<AppUser> passwordHasher, IConfiguration config) : IAuthService
    {
        private readonly AppDbContext _context = context;
        private readonly IPasswordHasher<AppUser> _passwordHasher = passwordHasher;
        private readonly IConfiguration _config = config;

        public async Task<string> RegisterAsync(AuthDTO.AuthRegisterDto dto)
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

            return JwtTokenGenerator.GenerateToken(user, _config);
        }

        public async Task<string> LoginAsync(AuthDTO.AuthLoginDto dto)
        {
            var user = await _context.AppUsers
                .FirstOrDefaultAsync(u => u.Email == dto.Email) ?? throw new Exception("Invalid credentials");

            var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, dto.Password);
            if (result == PasswordVerificationResult.Failed)
                throw new Exception("Invalid credentials");

            // Generate Token
            return JwtTokenGenerator.GenerateToken(user, _config);
        }
    }
}