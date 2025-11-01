using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NotiBlock.Backend.Data;
using NotiBlock.Backend.DTOs;
using NotiBlock.Backend.Helpers;
using NotiBlock.Backend.Interfaces;
using NotiBlock.Backend.Models;

namespace NotiBlock.Backend.Services
{
    public class AuthService(AppDbContext context, IConfiguration config) : IAuthService
    {
        private readonly AppDbContext _context = context;
        private readonly IConfiguration _config = config;
        private readonly PasswordHasher<object> _hasher = new();

        // Consumer Auth
        public async Task<string> RegisterConsumerAsync(AuthRegisterDTO dto)
        {
            if (await _context.Consumers.AnyAsync(c => c.Email == dto.Email))
                throw new Exception("Invalid credentials");

            var consumer = new Consumer
            {
                Id = Guid.NewGuid(),
                Name = dto.Name,
                Email = dto.Email,
                PhoneNumber = dto.PhoneNumber,
                WalletAddress = dto.WalletAddress,
                PasswordHash = _hasher.HashPassword(new object(), dto.Password)
            };

            _context.Consumers.Add(consumer);
            await _context.SaveChangesAsync();

            return JwtTokenGenerator.Generate(consumer.Id, consumer.Email, "consumer", _config);
        }

        public async Task<string> LoginConsumerAsync(AuthLoginDTO dto)
        {
            var user = await _context.Consumers.FirstOrDefaultAsync(c => c.Email == dto.Email);
            if (user == null || string.IsNullOrEmpty(user.Email)) throw new Exception("Invalid credentials");

            var result = _hasher.VerifyHashedPassword(new object(), user.PasswordHash, dto.Password);
            if (result == PasswordVerificationResult.Failed) throw new Exception("Invalid credentials");

            return JwtTokenGenerator.Generate(user.Id, user.Email!, "consumer", _config);
        }


        // Reseller Auth
        public async Task<string> RegisterResellerAsync(AuthRegisterDTO dto)
        {
            if (await _context.Resellers.AnyAsync(r => r.Email == dto.Email))
                throw new Exception("Invalid credentials");
            var reseller = new Reseller
            {
                Id = Guid.NewGuid(),
                CompanyName = dto.Name,
                Email = dto.Email,
                //PhoneNumber = dto.PhoneNumber,
                WalletAddress = dto.WalletAddress,
                PasswordHash = _hasher.HashPassword(new object(), dto.Password)
            };
            _context.Resellers.Add(reseller);
            await _context.SaveChangesAsync();
            return JwtTokenGenerator.Generate(reseller.Id, reseller.Email, "reseller", _config);
        }

        public async Task<string> LoginResellerAsync(AuthLoginDTO dto)
        {
            var user = await _context.Resellers.FirstOrDefaultAsync(r => r.Email == dto.Email);
            if (user == null || string.IsNullOrEmpty(user.Email)) throw new Exception("Invalid credentials");
            var result = _hasher.VerifyHashedPassword(new object(), user.PasswordHash, dto.Password);
            if (result == PasswordVerificationResult.Failed) throw new Exception("Invalid credentials");
            return JwtTokenGenerator.Generate(user.Id, user.Email!, "reseller", _config);
        }


        // Manufacturer Auth
        public async Task<string> RegisterManufacturerAsync(AuthRegisterDTO dto)
        {
            if (await _context.Manufacturers.AnyAsync(m => m.Email == dto.Email))
                throw new Exception("Invalid credentials");
            var manufacturer = new Manufacturer
            {
                Id = Guid.NewGuid(),
                CompanyName = dto.Name,
                Email = dto.Email,
                //PhoneNumber = dto.PhoneNumber,
                WalletAddress = dto.WalletAddress,
                PasswordHash = _hasher.HashPassword(new object(), dto.Password)
            };
            _context.Manufacturers.Add(manufacturer);
            await _context.SaveChangesAsync();
            return JwtTokenGenerator.Generate(manufacturer.Id, manufacturer.Email, "manufacturer", _config);
        }

        public async Task<string> LoginManufacturerAsync(AuthLoginDTO dto)
        {
            var user = await _context.Manufacturers.FirstOrDefaultAsync(m => m.Email == dto.Email);
            if (user == null || string.IsNullOrEmpty(user.Email)) throw new Exception("Invalid credentials");
            var result = _hasher.VerifyHashedPassword(new object(), user.PasswordHash, dto.Password);
            if (result == PasswordVerificationResult.Failed) throw new Exception("Invalid credentials");
            return JwtTokenGenerator.Generate(user.Id, user.Email!, "manufacturer", _config);


        }


        // Regulator Auth
        public async Task<string> RegisterRegulatorAsync(AuthRegisterDTO dto)
        {
            if (await _context.Regulators.AnyAsync(r => r.Email == dto.Email))
                throw new Exception("Invalid credentials");
            var regulator = new Regulator
            {
                Id = Guid.NewGuid(),
                AgencyName = dto.Name,
                Email = dto.Email,
                //PhoneNumber = dto.PhoneNumber,
                WalletAddress = dto.WalletAddress,
                PasswordHash = _hasher.HashPassword(new object(), dto.Password)
            };
            _context.Regulators.Add(regulator);
            await _context.SaveChangesAsync();
            return JwtTokenGenerator.Generate(regulator.Id, regulator.Email, "regulator", _config);
        }

        public async Task<string> LoginRegulatorAsync(AuthLoginDTO dto)
        {
            var user = await _context.Regulators.FirstOrDefaultAsync(r => r.Email == dto.Email);
            if (user == null || string.IsNullOrEmpty(user.Email)) throw new Exception("Invalid credentials");
            var result = _hasher.VerifyHashedPassword(new object(), user.PasswordHash, dto.Password);
            if (result == PasswordVerificationResult.Failed) throw new Exception("Invalid credentials");
            return JwtTokenGenerator.Generate(user.Id, user.Email!, "regulator", _config);
        }
    }

}
