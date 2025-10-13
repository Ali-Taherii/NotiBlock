namespace NotiBlock.Backend.DTOs
{
    public class AuthDTO
    {
        public class AuthRegisterDto
        {
            public string Email { get; set; } = string.Empty;
            public string Password { get; set; } = string.Empty;
            public string Role { get; set; } = "Consumer"; // Default role is Consumer
        }

        public class AuthLoginDto
        {
            public string Email { get; set; } = string.Empty;
            public string Password { get; set; } = string.Empty;
        }

        public class AuthGetRoleDto
        {
            public string Email { get; set; } = string.Empty;
            public string Password { get; set; } = string.Empty;
        }
    }
}