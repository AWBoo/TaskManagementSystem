using System.ComponentModel.DataAnnotations;

namespace backend.DTOS.User
{
    public class UpdateUserStatusRequest
    {
        [Required]
        [MaxLength(50)]
        public string NewStatus { get; set; } // e.g., "Active", "Deactivated",
    }
}
