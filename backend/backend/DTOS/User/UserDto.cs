using System;

namespace backend.DTOS.User
{
    public class UserDto
    {
        public Guid Id { get; set; }
        public string Email { get; set; }
        public string? Name { get; set; }
        public List<string> Roles { get; set; } = new List<string>();

        public int ProjectCount { get; set; }
        public int TaskCount { get; set; }

    }
}