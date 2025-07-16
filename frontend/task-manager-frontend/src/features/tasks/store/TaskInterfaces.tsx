// TaskStatus Type: Defines the possible statuses for a task.
export type TaskStatus = 'Not Started' | 'In Progress' | 'Completed' | 'On Hold' | 'Blocked' | 'To Do' | 'Done';

// ITask Interface: Represents a task as received from the backend.
export interface ITask {
  id: string;
  projectId: string;
  projectName: string;
  userId?: string | null;
  assignedUserEmail?: string | null;
  title: string;
  description?: string | null; 
  dueDate: string; 
  status: TaskStatus;
  createdAt: string;
  updatedAt: string;
}

// ICreateTaskRequest Interface: Data sent to the backend to create a new task.
export interface ICreateTaskRequest {
  assignedTo: any; 
  title: string;
  description?: string | null;
  dueDate: string; 
  projectId: string; 
  status?: TaskStatus; 
  userId?: string | null;
}

// IUpdateTaskRequest Interface: Data sent to the backend to update an existing task.
export interface IUpdateTaskRequest {
  id: string;
  title?: string; 
  description?: string | null; 
  dueDate?: string | null; 
  status?: TaskStatus; 
  projectId?: string; 
  userId?: string | null; 
}

// IProject Interface: Basic structure for project data.
export interface IProject {
  id: string;
  name: string;
  description?: string | null; 
  createdAt: string;
  updatedAt: string;
}