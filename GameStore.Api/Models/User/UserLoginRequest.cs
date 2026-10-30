using System.ComponentModel;

namespace GameStore.Api.Models
{
    public class UserLoginRequest
    {
        [DefaultValue("123@gmail.com")]
        public string Email { get; set; } = string.Empty;
        [DefaultValue("123123")]
        public string Password { get; set; } = string.Empty;
    }
}