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

        #region Registration/Login

        // ==================== GENERIC REGISTRATION ====================
        private async Task<string> RegisterUserAsync<T>(
            AuthRegisterDTO dto, 
            string role,
            Func<AuthRegisterDTO, T> userFactory,
            Func<string, Task<bool>> emailExistsCheck) where T : class
        {
            // Validate email
            if (string.IsNullOrWhiteSpace(dto.Email))
            {
                _logger.LogWarning("{Role} registration attempted with empty email", role);
                throw new ArgumentException("Email is required");
            }

            // Validate password
            if (string.IsNullOrWhiteSpace(dto.Password))
            {
                _logger.LogWarning("{Role} registration attempted with empty password", role);
                throw new ArgumentException("Password is required");
            }

            // Validate name (required for non-consumer roles)
            if (role != "consumer" && string.IsNullOrWhiteSpace(dto.Name))
            {
                _logger.LogWarning("{Role} registration attempted with empty name", role);
                throw new ArgumentException($"{role} name is required");
            }

            // Check for duplicate email
            if (await emailExistsCheck(dto.Email))
            {
                _logger.LogWarning("{Role} registration attempted with duplicate email: {Email}", role, dto.Email);
                throw new InvalidOperationException("An error has occurred");
            }

            // Create user
            var user = userFactory(dto);
            _context.Set<T>().Add(user);
            await _context.SaveChangesAsync();

            // Extract user data for JWT
            var userId = (Guid)user.GetType().GetProperty("Id")!.GetValue(user)!;
            var email = (string)user.GetType().GetProperty("Email")!.GetValue(user)!;

            _logger.LogInformation("{Role} registered successfully: {Email} with ID {UserId}", role, email, userId);

            return JwtTokenGenerator.Generate(userId, email, role, _config);
        }

        // Consumer Registration
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

            ValidatePassword(dto.Password);

            return await RegisterUserAsync(
                dto,
                "consumer",
                d => new Consumer
                {
                    Id = Guid.NewGuid(),
                    Name = d.Name?.Trim(),
                    Email = d.Email.Trim().ToLowerInvariant(),
                    PhoneNumber = d.PhoneNumber?.Trim(),
                    WalletAddress = d.WalletAddress?.Trim() ?? string.Empty,
                    PasswordHash = _hasher.HashPassword(new object(), d.Password),
                    CreatedAt = DateTime.UtcNow
                },
                async email => await _context.Consumers.AnyAsync(c => c.Email == email.ToLowerInvariant())
            );
        }

        // Reseller Registration
        public async Task<string> RegisterResellerAsync(AuthRegisterDTO dto)
        {
            return await RegisterUserAsync(
                dto,
                "reseller",
                d => new Reseller
                {
                    Id = Guid.NewGuid(),
                    CompanyName = d.Name.Trim(),
                    Email = d.Email.Trim().ToLowerInvariant(),
                    WalletAddress = d.WalletAddress?.Trim() ?? string.Empty,
                    PasswordHash = _hasher.HashPassword(new object(), d.Password),
                    CreatedAt = DateTime.UtcNow
                },
                async email => await _context.Resellers.AnyAsync(r => r.Email == email.ToLowerInvariant())
            );
        }

        // Manufacturer Registration
        public async Task<string> RegisterManufacturerAsync(AuthRegisterDTO dto)
        {
            return await RegisterUserAsync(
                dto,
                "manufacturer",
                d => new Manufacturer
                {
                    Id = Guid.NewGuid(),
                    CompanyName = d.Name.Trim(),
                    Email = d.Email.Trim().ToLowerInvariant(),
                    WalletAddress = d.WalletAddress?.Trim() ?? string.Empty,
                    PasswordHash = _hasher.HashPassword(new object(), d.Password),
                    CreatedAt = DateTime.UtcNow
                },
                async email => await _context.Manufacturers.AnyAsync(m => m.Email == email.ToLowerInvariant())
            );
        }

        // Regulator Registration
        public async Task<string> RegisterRegulatorAsync(AuthRegisterDTO dto)
        {
            return await RegisterUserAsync(
                dto,
                "regulator",
                d => new Regulator
                {
                    Id = Guid.NewGuid(),
                    AgencyName = d.Name.Trim(),
                    Email = d.Email.Trim().ToLowerInvariant(),
                    WalletAddress = d.WalletAddress?.Trim() ?? string.Empty,
                    PasswordHash = _hasher.HashPassword(new object(), d.Password),
                    CreatedAt = DateTime.UtcNow
                },
                async email => await _context.Regulators.AnyAsync(r => r.Email == email.ToLowerInvariant())
            );
        }

        // ==================== GENERIC LOGIN ====================
        private async Task<string> LoginUserAsync<T>(
            AuthLoginDTO dto,
            string role,
            Func<string, Task<T?>> findUserByEmail,
            Func<T, string> getPasswordHash) where T : class
        {
            // Validate credentials
            if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password))
            {
                _logger.LogWarning("{Role} login attempted with missing credentials", role);
                throw new ArgumentException("Email and password are required");
            }

            // Find user
            var user = await findUserByEmail(dto.Email.ToLowerInvariant());

            if (user == null)
            {
                _logger.LogWarning("{Role} login failed: user not found for email {Email}", role, dto.Email);
                throw new UnauthorizedAccessException("Invalid email or password");
            }

            // Verify password
            var result = _hasher.VerifyHashedPassword(new object(), getPasswordHash(user), dto.Password);

            if (result == PasswordVerificationResult.Failed)
            {
                _logger.LogWarning("{Role} login failed: incorrect password for email {Email}", role, dto.Email);
                throw new UnauthorizedAccessException("Invalid email or password");
            }

            // Extract user data
            var userId = (Guid)user.GetType().GetProperty("Id")!.GetValue(user)!;
            var email = (string)user.GetType().GetProperty("Email")!.GetValue(user)!;

            _logger.LogInformation("{Role} logged in successfully: {Email}", role, email);

            return JwtTokenGenerator.Generate(userId, email, role, _config);
        }

        // Consumer Login
        public async Task<string> LoginConsumerAsync(AuthLoginDTO dto)
        {
            return await LoginUserAsync(
                dto,
                "consumer",
                async email => await _context.Consumers.FirstOrDefaultAsync(c => c.Email == email),
                user => user.PasswordHash
            );
        }

        // Reseller Login
        public async Task<string> LoginResellerAsync(AuthLoginDTO dto)
        {
            return await LoginUserAsync(
                dto,
                "reseller",
                async email => await _context.Resellers.FirstOrDefaultAsync(r => r.Email == email),
                user => user.PasswordHash
            );
        }

        // Manufacturer Login
        public async Task<string> LoginManufacturerAsync(AuthLoginDTO dto)
        {
            return await LoginUserAsync(
                dto,
                "manufacturer",
                async email => await _context.Manufacturers.FirstOrDefaultAsync(m => m.Email == email),
                user => user.PasswordHash
            );
        }

        // Regulator Login
        public async Task<string> LoginRegulatorAsync(AuthLoginDTO dto)
        {
            return await LoginUserAsync(
                dto,
                "regulator",
                async email => await _context.Regulators.FirstOrDefaultAsync(r => r.Email == email),
                user => user.PasswordHash
            );
        }

        #endregion

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

            // REPLACE OLD LENGTH CHECK WITH FULL VALIDATION
            ValidatePassword(dto.NewPassword);

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
                        .CountAsync(); // Simplified - I'll join with actual recalls

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
                        .CountAsync(r => r.Product != null && r.Product.ResellerId == userId && r.Status == ReportStatus.Pending);

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
                        .CountAsync(t => t.Status == TicketStatus.UnderReview);
                    var reviewedTicketsCount = await _context.ResellerTickets
                        .CountAsync(t => t.Status == TicketStatus.Approved || t.Status == TicketStatus.Rejected);

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
                    {
                        var consumer = (Consumer)user;
                        var hasProducts = await _context.Products.AnyAsync(p => p.OwnerId == userId);
                        if (hasProducts)
                        {
                            _logger.LogWarning("Consumer {UserId} attempted to delete account with products in circulation", userId);
                            throw new InvalidOperationException("Cannot delete account while products are in circulation. Contact support.");
                        }
                        consumer.Email = deletedEmail;
                        consumer.Name = "Deleted User";
                        consumer.PasswordHash = string.Empty;
                        consumer.PhoneNumber = null;
                        consumer.WalletAddress = string.Empty;
                        consumer.IsDeleted = true;
                        consumer.DeletedAt = deletedAt;
                        consumer.DeletedBy = userId;
                        _context.Consumers.Update(consumer);
                        break;
                    }

                case "reseller":
                    {
                        var reseller = (Reseller)user;
                        reseller.Email = deletedEmail;
                        reseller.CompanyName = "Deleted Company";
                        reseller.PasswordHash = string.Empty;
                        reseller.WalletAddress = string.Empty;
                        reseller.IsDeleted = true;
                        reseller.DeletedAt = deletedAt;
                        reseller.DeletedBy = userId;
                        _context.Resellers.Update(reseller);
                        break;
                    }

                case "manufacturer":
                    {
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
                        manufacturer.IsDeleted = true;
                        manufacturer.DeletedAt = deletedAt;
                        manufacturer.DeletedBy = userId;
                        _context.Manufacturers.Update(manufacturer);
                        break;
                    }
                case "regulator":
                    {
                        var regulator = (Regulator)user;
                        regulator.Email = deletedEmail;
                        regulator.AgencyName = "Deleted Agency";
                        regulator.PasswordHash = string.Empty;
                        regulator.WalletAddress = string.Empty;
                        regulator.IsDeleted = true;
                        regulator.DeletedAt = deletedAt;
                        regulator.DeletedBy = userId;
                        _context.Regulators.Update(regulator);
                        break;
                    }
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Account soft deleted for user {UserId} with role {Role}", userId, role);
        }


        // PASSWORD VALIDATION HELPER
        private void ValidatePassword(string password)
        {
            var (isValid, errorMessage) = PasswordValidator.Validate(password);
            
            if (!isValid)
            {
                _logger.LogWarning("Password validation failed: {ErrorMessage}", errorMessage);
                throw new ArgumentException(errorMessage);
            }
        }
    }
}
