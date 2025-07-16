
export interface IProject {
  id: string; 
  name: string;
  description: string; // Backend sends "" for no description, not null/undefined
  createdAt: string;
  updatedAt: string;
}

// ICreateProjectRequest
export interface ICreateProjectRequest {
  name: string;
  description?: string; // Optional string, can be undefined
}

// IUpdateProjectRequest
export interface IUpdateProjectRequest {
  name: string;
  description?: string; // Optional string, can be undefined
}


export interface IFetchProjectsParams {
  startDate?: string; 
  endDate?: string;   
}