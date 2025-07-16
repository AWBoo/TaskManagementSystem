using System.ComponentModel.DataAnnotations;

namespace backend.DTOS.Auth
{
    public class RegisterRequest
    {
        [Required, EmailAddress]
        public string Email { get; set; }

        [Required, MinLength(6)]
        public string Password { get; set; }
    }
}
