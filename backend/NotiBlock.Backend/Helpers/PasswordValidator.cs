namespace NotiBlock.Backend.Helpers
{
    public static class PasswordValidator
    {
        public static (bool IsValid, string ErrorMessage) Validate(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                return (false, "Password is required");

            if (password.Length < 8)
                return (false, "Password must be at least 8 characters long");

            if (password.Length > 128)
                return (false, "Password must not exceed 128 characters");

            if (!password.Any(char.IsUpper))
                return (false, "Password must contain at least one uppercase letter");

            if (!password.Any(char.IsLower))
                return (false, "Password must contain at least one lowercase letter");

            if (!password.Any(char.IsDigit))
                return (false, "Password must contain at least one number");

            if (!password.Any(ch => !char.IsLetterOrDigit(ch)))
                return (false, "Password must contain at least one special character");

            // Check for common weak passwords
            var commonPasswords = new[] { "Password123!", "Welcome123!", "Admin123!" }; // I'm gonna update with a more comprehensive list later
            if (commonPasswords.Any(p => p.Equals(password, StringComparison.OrdinalIgnoreCase)))
                return (false, "Password is too common. Please choose a stronger password");

            return (true, string.Empty);
        }

        public static string GetPasswordRequirements()
        {
            return "Password must be at least 8 characters long and contain: " +
                   "one uppercase letter, one lowercase letter, one number, and one special character.";
        }
    }
}