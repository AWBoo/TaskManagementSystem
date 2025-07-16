using System.ComponentModel.DataAnnotations;

namespace backend.DTOS.User
{
    public class AssignRoleRequest
    {
        [Required]
        public string RoleName { get; set; }
    }
}
