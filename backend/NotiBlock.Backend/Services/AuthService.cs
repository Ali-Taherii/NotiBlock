using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NotiBlock.Backend.Data;
using NotiBlock.Backend.DTOs;
using NotiBlock.Backend.Helpers;
using NotiBlock.Backend.Interfaces;
using NotiBlock.Backend.Models;

namespace NotiBlock.Backend.Services
{
    public class AuthService(AppDbContext context, IConfiguration config, ILogger<AuthService> logger) : IAuthService
    {
        private readonly AppDbContext _context = context;
        private readonly IConfiguration _config = config;
        private readonly ILogger<AuthService> _logger = logger;
        private readonly PasswordHasher<object> _hasher = new();

        // Consumer Auth
        public async Task<string> RegisterConsumerAsync(AuthRegisterDTO dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Email))
            {
                _logger.LogWarning("Consumer registration attempted with empty email");
                throw new ArgumentException("Email is required");
            }

            if (string.IsNullOrWhiteSpace(dto.Password))
            {
                _logger.LogWarning("Consumer registration attempted with empty password");
                throw new ArgumentException("Password is required");
            }

            if (await _context.Consumers.AnyAsync(c => c.Email == dto.Email))
            {
                _logger.LogWarning("Consumer registration attempted with duplicate email: {Email}", dto.Email);
                throw new InvalidOperationException("An error has occured");
            }

            var consumer = new Consumer
            {
                Id = Guid.NewGuid(),
                Name = dto.Name?.Trim(),
                Email = dto.Email.Trim().ToLowerInvariant(),
                PhoneNumber = dto.PhoneNumber?.Trim(),
                WalletAddress = dto.WalletAddress?.Trim() ?? string.Empty,
                PasswordHash = _hasher.HashPassword(new object(), dto.Password),
                CreatedAt = DateTime.UtcNow
            };

            _context.Consumers.Add(consumer);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Consumer registered successfully: {Email} with ID {ConsumerId}", consumer.Email, consumer.Id);

            return JwtTokenGenerator.Generate(consumer.Id, consumer.Email, "consumer", _config);
        }

        public async Task<string> LoginConsumerAsync(AuthLoginDTO dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password))
            {
                _logger.LogWarning("Consumer login attempted with missing credentials");
                throw new ArgumentException("Email and password are required");
            }

            var user = await _context.Consumers.FirstOrDefaultAsync(c => c.Email == dto.Email.ToLowerInvariant());
            
            if (user == null)
            {
                _logger.LogWarning("Consumer login failed: user not found for email {Email}", dto.Email);
                throw new UnauthorizedAccessException("Invalid email or password");
            }

            var result = _hasher.VerifyHashedPassword(new object(), user.PasswordHash, dto.Password);
            
            if (result == PasswordVerificationResult.Failed)
            {
                _logger.LogWarning("Consumer login failed: incorrect password for email {Email}", dto.Email);
                throw new UnauthorizedAccessException("Invalid email or password");
            }

            _logger.LogInformation("Consumer logged in successfully: {Email}", user.Email);

            return JwtTokenGenerator.Generate(user.Id, user.Email!, "consumer", _config);
        }

        // Reseller Auth
        public async Task<string> RegisterResellerAsync(AuthRegisterDTO dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Email))
            {
                _logger.LogWarning("Reseller registration attempted with empty email");
                throw new ArgumentException("Email is required");
            }

            if (string.IsNullOrWhiteSpace(dto.Password))
            {
                _logger.LogWarning("Reseller registration attempted with empty password");
                throw new ArgumentException("Password is required");
            }

            if (string.IsNullOrWhiteSpace(dto.Name))
            {
                _logger.LogWarning("Reseller registration attempted with empty company name");
                throw new ArgumentException("Company name is required");
            }

            if (await _context.Resellers.AnyAsync(r => r.Email == dto.Email))
            {
                _logger.LogWarning("Reseller registration attempted with duplicate email: {Email}", dto.Email);
                throw new InvalidOperationException("An error has occured");
            }

            var reseller = new Reseller
            {
                Id = Guid.NewGuid(),
                CompanyName = dto.Name.Trim(),
                Email = dto.Email.Trim().ToLowerInvariant(),
                WalletAddress = dto.WalletAddress?.Trim() ?? string.Empty,
                PasswordHash = _hasher.HashPassword(new object(), dto.Password),
                CreatedAt = DateTime.UtcNow
            };

            _context.Resellers.Add(reseller);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Reseller registered successfully: {Email} ({CompanyName}) with ID {ResellerId}", 
                reseller.Email, reseller.CompanyName, reseller.Id);

            return JwtTokenGenerator.Generate(reseller.Id, reseller.Email, "reseller", _config);
        }

        public async Task<string> LoginResellerAsync(AuthLoginDTO dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password))
            {
                _logger.LogWarning("Reseller login attempted with missing credentials");
                throw new ArgumentException("Email and password are required");
            }

            var user = await _context.Resellers.FirstOrDefaultAsync(r => r.Email == dto.Email.ToLowerInvariant());
            
            if (user == null)
            {
                _logger.LogWarning("Reseller login failed: user not found for email {Email}", dto.Email);
                throw new UnauthorizedAccessException("Invalid email or password");
            }

            var result = _hasher.VerifyHashedPassword(new object(), user.PasswordHash, dto.Password);
            
            if (result == PasswordVerificationResult.Failed)
            {
                _logger.LogWarning("Reseller login failed: incorrect password for email {Email}", dto.Email);
                throw new UnauthorizedAccessException("Invalid email or password");
            }

            _logger.LogInformation("Reseller logged in successfully: {Email} ({CompanyName})", user.Email, user.CompanyName);

            return JwtTokenGenerator.Generate(user.Id, user.Email!, "reseller", _config);
        }

        // Manufacturer Auth
        public async Task<string> RegisterManufacturerAsync(AuthRegisterDTO dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Email))
            {
                _logger.LogWarning("Manufacturer registration attempted with empty email");
                throw new ArgumentException("Email is required");
            }

            if (string.IsNullOrWhiteSpace(dto.Password))
            {
                _logger.LogWarning("Manufacturer registration attempted with empty password");
                throw new ArgumentException("Password is required");
            }

            if (string.IsNullOrWhiteSpace(dto.Name))
            {
                _logger.LogWarning("Manufacturer registration attempted with empty company name");
                throw new ArgumentException("Company name is required");
            }

            if (await _context.Manufacturers.AnyAsync(m => m.Email == dto.Email))
            {
                _logger.LogWarning("Manufacturer registration attempted with duplicate email: {Email}", dto.Email);
                throw new InvalidOperationException("An error has occured");
            }

            var manufacturer = new Manufacturer
            {
                Id = Guid.NewGuid(),
                CompanyName = dto.Name.Trim(),
                Email = dto.Email.Trim().ToLowerInvariant(),
                WalletAddress = dto.WalletAddress?.Trim() ?? string.Empty,
                PasswordHash = _hasher.HashPassword(new object(), dto.Password),
                CreatedAt = DateTime.UtcNow
            };

            _context.Manufacturers.Add(manufacturer);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Manufacturer registered successfully: {Email} ({CompanyName}) with ID {ManufacturerId}", 
                manufacturer.Email, manufacturer.CompanyName, manufacturer.Id);

            return JwtTokenGenerator.Generate(manufacturer.Id, manufacturer.Email, "manufacturer", _config);
        }

        public async Task<string> LoginManufacturerAsync(AuthLoginDTO dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password))
            {
                _logger.LogWarning("Manufacturer login attempted with missing credentials");
                throw new ArgumentException("Email and password are required");
            }

            var user = await _context.Manufacturers.FirstOrDefaultAsync(m => m.Email == dto.Email.ToLowerInvariant());
            
            if (user == null)
            {
                _logger.LogWarning("Manufacturer login failed: user not found for email {Email}", dto.Email);
                throw new UnauthorizedAccessException("Invalid email or password");
            }

            var result = _hasher.VerifyHashedPassword(new object(), user.PasswordHash, dto.Password);
            
            if (result == PasswordVerificationResult.Failed)
            {
                _logger.LogWarning("Manufacturer login failed: incorrect password for email {Email}", dto.Email);
                throw new UnauthorizedAccessException("Invalid email or password");
            }

            _logger.LogInformation("Manufacturer logged in successfully: {Email} ({CompanyName})", user.Email, user.CompanyName);

            return JwtTokenGenerator.Generate(user.Id, user.Email!, "manufacturer", _config);
        }

        // Regulator Auth
        public async Task<string> RegisterRegulatorAsync(AuthRegisterDTO dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Email))
            {
                _logger.LogWarning("Regulator registration attempted with empty email");
                throw new ArgumentException("Email is required");
            }

            if (string.IsNullOrWhiteSpace(dto.Password))
            {
                _logger.LogWarning("Regulator registration attempted with empty password");
                throw new ArgumentException("Password is required");
            }

            if (string.IsNullOrWhiteSpace(dto.Name))
            {
                _logger.LogWarning("Regulator registration attempted with empty agency name");
                throw new ArgumentException("Agency name is required");
            }

            if (await _context.Regulators.AnyAsync(r => r.Email == dto.Email))
            {
                _logger.LogWarning("Regulator registration attempted with duplicate email: {Email}", dto.Email);
                throw new InvalidOperationException("An error has occured");
            }

            var regulator = new Regulator
            {
                Id = Guid.NewGuid(),
                AgencyName = dto.Name.Trim(),
                Email = dto.Email.Trim().ToLowerInvariant(),
                WalletAddress = dto.WalletAddress?.Trim() ?? string.Empty,
                PasswordHash = _hasher.HashPassword(new object(), dto.Password),
                CreatedAt = DateTime.UtcNow
            };

            _context.Regulators.Add(regulator);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Regulator registered successfully: {Email} ({AgencyName}) with ID {RegulatorId}", 
                regulator.Email, regulator.AgencyName, regulator.Id);

            return JwtTokenGenerator.Generate(regulator.Id, regulator.Email, "regulator", _config);
        }

        public async Task<string> LoginRegulatorAsync(AuthLoginDTO dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password))
            {
                _logger.LogWarning("Regulator login attempted with missing credentials");
                throw new ArgumentException("Email and password are required");
            }

            var user = await _context.Regulators.FirstOrDefaultAsync(r => r.Email == dto.Email.ToLowerInvariant());
            
            if (user == null)
            {
                _logger.LogWarning("Regulator login failed: user not found for email {Email}", dto.Email);
                throw new UnauthorizedAccessException("Invalid email or password");
            }

            var result = _hasher.VerifyHashedPassword(new object(), user.PasswordHash, dto.Password);
            
            if (result == PasswordVerificationResult.Failed)
            {
                _logger.LogWarning("Regulator login failed: incorrect password for email {Email}", dto.Email);
                throw new UnauthorizedAccessException("Invalid email or password");
            }

            _logger.LogInformation("Regulator logged in successfully: {Email} ({AgencyName})", user.Email, user.AgencyName);

            return JwtTokenGenerator.Generate(user.Id, user.Email!, "regulator", _config);
        }
    }
}
