// Matches backend.DTOS.Dashboard.TaskStatusCountDto
export interface ITaskStatusCount {
  status: string;
  count: number;
}

// Matches backend.DTOS.Dashboard.ProjectTaskCountDto
export interface IProjectTaskCount {
  projectName: string;
  taskCount: number;
}

// Matches backend.DTOS.Dashboard.UserDashboardDto
export interface IUserDashboardData {
  myTotalTasks: number;
  myTasksDueSoon: number;
  myOverdueTasks: number;
  myTaskStatusCounts: ITaskStatusCount[];
  myProjectTaskCounts: IProjectTaskCount[];
}