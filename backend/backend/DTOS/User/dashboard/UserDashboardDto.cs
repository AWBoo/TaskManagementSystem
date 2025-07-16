using backend.DTOS.Tasks;

namespace backend.DTOS.User.dashboard
{
    public class UserDashboardDto
    {
        // Card Stats for Tasks
        public int MyTotalTasks { get; set; }
        public int MyTasksDueSoon { get; set; }
        public int MyOverdueTasks { get; set; }

        // Task Status Counts for Pie Chart
        public List<TaskStatusCountDto> MyTaskStatusCounts { get; set; } = new List<TaskStatusCountDto>();

        // Project Task Counts for Bar Chart
        public List<ProjectTaskCountDto> MyProjectTaskCounts { get; set; } = new List<ProjectTaskCountDto>();
    }
}
