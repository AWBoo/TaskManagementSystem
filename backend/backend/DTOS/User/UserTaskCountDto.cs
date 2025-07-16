namespace backend.DTOS.User
{
    public class UserTaskCountDto
    {
        public Guid UserId { get; set; }
        public string UserEmail { get; set; } = "";
        public int TaskCount { get; set; }
    }
}
