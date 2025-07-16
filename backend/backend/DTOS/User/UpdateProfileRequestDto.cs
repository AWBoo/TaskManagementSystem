using System.ComponentModel.DataAnnotations;

namespace backend.DTOS.User
{
    public class UpdateProfileRequestDto
    {
        [StringLength(100, ErrorMessage = "First name cannot exceed 100 characters.")]
        public string? Name { get; set; }

        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public string? Email { get; set; }
    }
}
