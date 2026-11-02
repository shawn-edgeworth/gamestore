using System.ComponentModel;

namespace GameStore.Api.Models
{
    public class UserLoginRequest
    {
        [DefaultValue("admin@gmail.com")]
        public string Email { get; set; } = string.Empty;
        [DefaultValue("admin123")]
        public string Password { get; set; } = string.Empty;
    }
}