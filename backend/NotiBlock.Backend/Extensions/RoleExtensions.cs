using NotiBlock.Backend.Models;

namespace NotiBlock.Backend.Extensions
{
    public static class RoleExtensions
    {
        public static string ToRoleString(this UserRole role)
        {
            return role.ToString().ToLowerInvariant();
        }

        public static UserRole ToUserRole(this string roleString)
        {
            return roleString.ToLowerInvariant() switch
            {
                "consumer" => UserRole.Consumer,
                "reseller" => UserRole.Reseller,
                "manufacturer" => UserRole.Manufacturer,
                "regulator" => UserRole.Regulator,
                _ => throw new ArgumentException($"Invalid role: {roleString}")
            };
        }

        public static bool TryParseUserRole(this string roleString, out UserRole role)
        {
            try
            {
                role = roleString.ToUserRole();
                return true;
            }
            catch
            {
                role = default;
                return false;
            }
        }
    }
}