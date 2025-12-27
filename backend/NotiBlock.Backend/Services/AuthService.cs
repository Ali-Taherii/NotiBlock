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

            var user = await _context.Regulators.FirstOrDefaultAsync(r => r.Email.Equals(dto.Email, StringComparison.InvariantCultureIgnoreCase));
            
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

        // Change Password
        public async Task ChangePasswordAsync(Guid userId, string role, ChangePasswordDTO dto)
        {
            if (string.IsNullOrWhiteSpace(dto.CurrentPassword))
            {
                _logger.LogWarning("Change password attempted with empty current password");
                throw new ArgumentException("Current password is required");
            }

            if (string.IsNullOrWhiteSpace(dto.NewPassword))
            {
                _logger.LogWarning("Change password attempted with empty new password");
                throw new ArgumentException("New password is required");
            }

            if (dto.NewPassword != dto.ConfirmPassword)
            {
                _logger.LogWarning("Change password attempted with mismatched passwords");
                throw new ArgumentException("New password and confirmation do not match");
            }

            if (dto.NewPassword.Length < 6)
            {
                _logger.LogWarning("Change password attempted with weak password");
                throw new ArgumentException("New password must be at least 6 characters long");
            }

            object? user = role switch
            {
                "consumer" => await _context.Consumers.FindAsync(userId),
                "reseller" => await _context.Resellers.FindAsync(userId),
                "manufacturer" => await _context.Manufacturers.FindAsync(userId),
                "regulator" => await _context.Regulators.FindAsync(userId),
                _ => null
            };

            if (user == null)
            {
                _logger.LogWarning("Change password failed: user not found for {UserId} with role {Role}", userId, role);
                throw new KeyNotFoundException("User not found");
            }

            // Verify current password
            var currentPasswordHash = role switch
            {
                "consumer" => ((Consumer)user).PasswordHash,
                "reseller" => ((Reseller)user).PasswordHash,
                "manufacturer" => ((Manufacturer)user).PasswordHash,
                "regulator" => ((Regulator)user).PasswordHash,
                _ => string.Empty
            };

            var verificationResult = _hasher.VerifyHashedPassword(new object(), currentPasswordHash, dto.CurrentPassword);
            
            if (verificationResult == PasswordVerificationResult.Failed)
            {
                _logger.LogWarning("Change password failed: incorrect current password for user {UserId}", userId);
                throw new UnauthorizedAccessException("Current password is incorrect");
            }

            // Hash new password
            var newPasswordHash = _hasher.HashPassword(new object(), dto.NewPassword);

            // Update password based on role
            switch (role)
            {
                case "consumer":
                    ((Consumer)user).PasswordHash = newPasswordHash;
                    _context.Consumers.Update((Consumer)user);
                    break;
                case "reseller":
                    ((Reseller)user).PasswordHash = newPasswordHash;
                    _context.Resellers.Update((Reseller)user);
                    break;
                case "manufacturer":
                    ((Manufacturer)user).PasswordHash = newPasswordHash;
                    _context.Manufacturers.Update((Manufacturer)user);
                    break;
                case "regulator":
                    ((Regulator)user).PasswordHash = newPasswordHash;
                    _context.Regulators.Update((Regulator)user);
                    break;
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Password changed successfully for user {UserId} with role {Role}", userId, role);
        }

        // Update Profile
        public async Task<object> UpdateProfileAsync(Guid userId, string role, UpdateProfileDTO dto)
        {
            object? user = role switch
            {
                "consumer" => await _context.Consumers.FindAsync(userId),
                "reseller" => await _context.Resellers.FindAsync(userId),
                "manufacturer" => await _context.Manufacturers.FindAsync(userId),
                "regulator" => await _context.Regulators.FindAsync(userId),
                _ => null
            };

            if (user == null)
            {
                _logger.LogWarning("Update profile failed: user not found for {UserId} with role {Role}", userId, role);
                throw new KeyNotFoundException("User not found");
            }

            switch (role)
            {
                case "consumer":
                    var consumer = (Consumer)user;
                    if (!string.IsNullOrWhiteSpace(dto.Name))
                        consumer.Name = dto.Name.Trim();
                    if (!string.IsNullOrWhiteSpace(dto.PhoneNumber))
                        consumer.PhoneNumber = dto.PhoneNumber.Trim();
                    if (dto.WalletAddress != null)
                        consumer.WalletAddress = dto.WalletAddress.Trim();
                    _context.Consumers.Update(consumer);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Consumer profile updated for {UserId}", userId);
                    return new
                    {
                        consumer.Id,
                        consumer.Name,
                        consumer.Email,
                        consumer.PhoneNumber,
                        consumer.WalletAddress,
                        consumer.CreatedAt
                    };

                case "reseller":
                    var reseller = (Reseller)user;
                    if (!string.IsNullOrWhiteSpace(dto.Name))
                        reseller.CompanyName = dto.Name.Trim();
                    if (dto.WalletAddress != null)
                        reseller.WalletAddress = dto.WalletAddress.Trim();
                    _context.Resellers.Update(reseller);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Reseller profile updated for {UserId}", userId);
                    return new
                    {
                        reseller.Id,
                        reseller.CompanyName,
                        reseller.Email,
                        reseller.WalletAddress,
                        reseller.CreatedAt
                    };

                case "manufacturer":
                    var manufacturer = (Manufacturer)user;
                    if (!string.IsNullOrWhiteSpace(dto.Name))
                        manufacturer.CompanyName = dto.Name.Trim();
                    if (dto.WalletAddress != null)
                        manufacturer.WalletAddress = dto.WalletAddress.Trim();
                    _context.Manufacturers.Update(manufacturer);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Manufacturer profile updated for {UserId}", userId);
                    return new
                    {
                        manufacturer.Id,
                        manufacturer.CompanyName,
                        manufacturer.Email,
                        manufacturer.WalletAddress,
                        manufacturer.CreatedAt
                    };

                case "regulator":
                    var regulator = (Regulator)user;
                    if (!string.IsNullOrWhiteSpace(dto.Name))
                        regulator.AgencyName = dto.Name.Trim();
                    if (dto.WalletAddress != null)
                        regulator.WalletAddress = dto.WalletAddress.Trim();
                    _context.Regulators.Update(regulator);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Regulator profile updated for {UserId}", userId);
                    return new
                    {
                        regulator.Id,
                        regulator.AgencyName,
                        regulator.Email,
                        regulator.WalletAddress,
                        regulator.CreatedAt
                    };

                default:
                    throw new ArgumentException("Invalid user role");
            }
        }

        // Check Email Availability
        public async Task<bool> IsEmailAvailableAsync(string email, string userType)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                _logger.LogWarning("Email availability check attempted with empty email");
                throw new ArgumentException("Email is required");
            }

            var normalizedEmail = email.Trim().ToLowerInvariant();

            bool exists = userType.ToLowerInvariant() switch
            {
                "consumer" => await _context.Consumers.AnyAsync(c => c.Email == normalizedEmail),
                "reseller" => await _context.Resellers.AnyAsync(r => r.Email == normalizedEmail),
                "manufacturer" => await _context.Manufacturers.AnyAsync(m => m.Email == normalizedEmail),
                "regulator" => await _context.Regulators.AnyAsync(r => r.Email == normalizedEmail),
                _ => throw new ArgumentException("Invalid user type")
            };

            _logger.LogInformation("Email availability checked for {Email} in {UserType}: {Available}", 
                normalizedEmail, userType, !exists);

            return !exists; // Return true if email is available (not exists)
        }

        // Get User Statistics (for dashboard)
        public async Task<object> GetUserStatsAsync(Guid userId, string role)
        {
            switch (role.ToLowerInvariant())
            {
                case "consumer":
                    var consumerProductsCount = await _context.Products.CountAsync(p => p.OwnerId == userId);
                    var consumerReportsCount = await _context.ConsumerReports.CountAsync(r => r.ConsumerId == userId);
                    var consumerRecallsCount = await _context.Products
                        .Where(p => p.OwnerId == userId)
                        .Select(p => p.Model)
                        .Distinct()
                        .CountAsync(); // Simplified - you'd want to join with actual recalls

                    _logger.LogInformation("Stats retrieved for consumer {UserId}", userId);

                    return new
                    {
                        productsOwned = consumerProductsCount,
                        reportsSubmitted = consumerReportsCount,
                        activeRecalls = consumerRecallsCount, // Should check actual recalls
                        role = "consumer"
                    };

                case "reseller":
                    var resellerProductsCount = await _context.Products.CountAsync(p => p.ResellerId == userId);
                    var resellerTicketsCount = await _context.ResellerTickets.CountAsync(t => t.ResellerId == userId);
                    var pendingReportsCount = await _context.ConsumerReports
                        .CountAsync(r => r.Product.ResellerId == userId && r.Status == "pending");

                    _logger.LogInformation("Stats retrieved for reseller {UserId}", userId);

                    return new
                    {
                        productsSold = resellerProductsCount,
                        ticketsCreated = resellerTicketsCount,
                        pendingReports = pendingReportsCount,
                        role = "reseller"
                    };

                case "manufacturer":
                    var manufacturerProductsCount = await _context.Products.CountAsync(p => p.ManufacturerId == userId);
                    var manufacturerRecallsCount = await _context.Recalls.CountAsync(r => r.ManufacturerId == userId);

                    _logger.LogInformation("Stats retrieved for manufacturer {UserId}", userId);

                    return new
                    {
                        productsCreated = manufacturerProductsCount,
                        recallsIssued = manufacturerRecallsCount,
                        role = "manufacturer"
                    };

                case "regulator":
                    var pendingTicketsCount = await _context.ResellerTickets
                        .CountAsync(t => t.Status == "submitted_to_regulator");
                    var reviewedTicketsCount = await _context.ResellerTickets
                        .CountAsync(t => t.Status == "approved_for_manufacturer" || t.Status == "rejected");

                    _logger.LogInformation("Stats retrieved for regulator {UserId}", userId);

                    return new
                    {
                        pendingReviews = pendingTicketsCount,
                        reviewedTickets = reviewedTicketsCount,
                        role = "regulator"
                    };

                default:
                    throw new ArgumentException("Invalid user role");
            }
        }

        // Delete Account (Soft Delete)
        public async Task DeleteAccountAsync(Guid userId, string role, string password)
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                _logger.LogWarning("Account deletion attempted without password confirmation");
                throw new ArgumentException("Password is required to delete account");
            }

            object? user = role switch
            {
                "consumer" => await _context.Consumers.FindAsync(userId),
                "reseller" => await _context.Resellers.FindAsync(userId),
                "manufacturer" => await _context.Manufacturers.FindAsync(userId),
                "regulator" => await _context.Regulators.FindAsync(userId),
                _ => null
            };

            if (user == null)
            {
                _logger.LogWarning("Account deletion failed: user not found for {UserId} with role {Role}", userId, role);
                throw new KeyNotFoundException("User not found");
            }

            // Verify password
            var passwordHash = role switch
            {
                "consumer" => ((Consumer)user).PasswordHash,
                "reseller" => ((Reseller)user).PasswordHash,
                "manufacturer" => ((Manufacturer)user).PasswordHash,
                "regulator" => ((Regulator)user).PasswordHash,
                _ => string.Empty
            };

            var verificationResult = _hasher.VerifyHashedPassword(new object(), passwordHash, password);
            
            if (verificationResult == PasswordVerificationResult.Failed)
            {
                _logger.LogWarning("Account deletion failed: incorrect password for user {UserId}", userId);
                throw new UnauthorizedAccessException("Incorrect password");
            }

            // Soft delete by anonymizing data (better than hard delete for audit trail)
            var deletedEmail = $"deleted_{userId}@notiblock.deleted";
            var deletedAt = DateTime.UtcNow;

            switch (role)
            {
                case "consumer":
                    var consumer = (Consumer)user;
                    consumer.Email = deletedEmail;
                    consumer.Name = "Deleted User";
                    consumer.PasswordHash = string.Empty;
                    consumer.PhoneNumber = null;
                    consumer.WalletAddress = string.Empty;
                    _context.Consumers.Update(consumer);
                    break;

                case "reseller":
                    var reseller = (Reseller)user;
                    reseller.Email = deletedEmail;
                    reseller.CompanyName = "Deleted Company";
                    reseller.PasswordHash = string.Empty;
                    reseller.WalletAddress = string.Empty;
                    _context.Resellers.Update(reseller);
                    break;

                case "manufacturer":
                    var manufacturer = (Manufacturer)user;
                    // Check if manufacturer has products in circulation
                    var hasProducts = await _context.Products.AnyAsync(p => p.ManufacturerId == userId);
                    if (hasProducts)
                    {
                        _logger.LogWarning("Manufacturer {UserId} attempted to delete account with products in circulation", userId);
                        throw new InvalidOperationException("Cannot delete account while products are in circulation. Contact support.");
                    }
                    manufacturer.Email = deletedEmail;
                    manufacturer.CompanyName = "Deleted Company";
                    manufacturer.PasswordHash = string.Empty;
                    manufacturer.WalletAddress = string.Empty;
                    _context.Manufacturers.Update(manufacturer);
                    break;

                case "regulator":
                    var regulator = (Regulator)user;
                    regulator.Email = deletedEmail;
                    regulator.AgencyName = "Deleted Agency";
                    regulator.PasswordHash = string.Empty;
                    regulator.WalletAddress = string.Empty;
                    _context.Regulators.Update(regulator);
                    break;
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Account soft deleted for user {UserId} with role {Role}", userId, role);
        }
    }
}
