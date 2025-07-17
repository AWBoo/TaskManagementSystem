# Task Management System

This repository contains a full-stack Task Management System developed as a technical assessment for Claer & Volker. It features an ASP.NET Core REST API backend and a modern Single-Page Application (SPA) frontend.

## Table of Contents

* [Features](#features)
* [Technology Stack](#technology-stack)
* [Setup Instructions](#setup-instructions)
* [Running the Applications](#running-the-applications)
* [Getting Started with the Application](#getting-started-with-the-application)
* [API Endpoints](#api-endpoints)

## Features

The system provides the following core functionalities:

* **Authentication & Authorization**: User registration, login, and JWT-based authentication. All API endpoints are protected, with administrative functions restricted to Admin users.
* **User Management (Admin Role)**: Admins can list, manage roles (User/Admin), and activate/deactivate user accounts.
* **Project Management**: Users can perform CRUD operations on their own projects. Deleting a project cascades to its associated tasks.
* **Task Management**: Users can perform CRUD operations on tasks within their projects.

## Technology Stack

### Backend
* **Language**: C# (.NET 8.0)
* **Framework**: ASP.NET Core Web API
* **Authentication**: JWT tokens (`Microsoft.AspNetCore.Authentication.JwtBearer`)
* **Data Access**: Entity Framework Core (Code-First migrations)
* **Database**: SQL Server
* **Other**: Dependency Injection, Microsoft.Extensions.Logging, Swagger/OpenAPI, BCrypt.Net.BCrypt for password hashing.

### Frontend
* **Framework**: React with TypeScript
* **State Management**: Redux
* **UI Library**:  Custom Styling (Css)

## Setup Instructions

### Prerequisites
* .NET SDK 8.0+
* Node.js LTS (with npm or Yarn)
* SSMS
* Git

### Steps
1.  **Clone the Repository**:
    ```bash
    git clone [https://github.com/AWBoo/TaskManagementSystem.git](https://github.com/AWBoo/TaskManagementSystem.git)
    cd TaskManagementSystem
    ```
2.  **Backend Setup** (in `backend/backend`):
    * `dotnet restore`
    * Update `ConnectionStrings` in `appsettings.json` (and `appsettings.Development.json`)
      (Common issue was that Trusted_Connection=True wasn't included in the string)
      (ie: Server=(localdb)\\mssqllocaldb;Database=YourPrefferedDBName;Trusted_Connection=True;MultipleActiveResultSets=true)
      While your there feel free to disable the logging other wise it's going to get very               cluttered.
      
    * Configure CORS Policy: In Program.cs, locate the CORS policy definition (In Program.cs). Ensure the WithOrigins method includes the URL where your frontend application will run (e.g., http://localhost:5173).
      
    * Apply migrations: `dotnet ef database update`(If it doesn't exsit then it should create it ).
3.  **Frontend Setup** (in `frontend/task-manager-frontend`):
    * `npm install` (or `yarn install`)
      
    * Configure API base URL in `.env` (e.g., `VITE_API_BASE_URL=https://localhost:7071`).

## Running the Applications
NOTE: I prefer to run my frontend in VSCode and My Backend In Visual Studio

1.  **Start Backend API**:
    In `backend/backend` directory:
    ```bash
    dotnet run
    ```
    (API typically runs on `https://localhost:7071`).
2.  **Start Frontend SPA**:
    In `frontend/task-manager-frontend` directory:
    ```bash
    npm run dev
    # OR yarn dev
    ```
    (SPA typically opens on `http://localhost:5173` that was what my local host ended up being).

## Getting Started with the Application

Follow these steps to quickly understand and interact with the core functionalities of the Task Management System:

1.  **Access the Admin Account**:
    * Your backend's `Program.cs` injects a default Admin user. Please refer to your `Program.cs` file to find the initial Admin credentials (e.g., `Admin@Admin.com`, `Admin!`).
    * Log in to the frontend application using these Admin credentials.

2.  **Create a New Project (Admin Only)**:
    * As an Admin user, navigate to the "Projects" section (or equivalent).
    * Use the "New Project" or "Add Project" functionality to create your first project. Provide a name and optional description. Projects are essential as tasks must belong to a project.

3.  **Explore User Roles**:
    * You can now log out of the Admin account.
    * **Register a new standard user account** via the frontend's registration page.
    * **Log in as this standard user**.
    * You will observe that standard users can **view existing projects** (if they are assigned or public) and **create tasks within those projects**, but they **cannot create new projects** themselves.

4.  **Add and Manage Tasks**:
    * Whether logged in as Admin or a Standard User (with access to a project), navigate to a project's detail page.
    * Use the "Add Task" button (or similar) to create new tasks within this project.
    * From the project detail page, you can view, edit, update the status, or delete tasks.

5.  **Explore Admin Features (Optional)**:
    * Log back in as the Admin user.
    * Navigate to the "Admin Dashboard" section to view and manage other users' roles and statuses.

## API Endpoints

The API provides endpoints for Authentication, User Management (Admin), Projects, and Tasks. Full details and schema are available via **Swagger UI** once the backend is running.

* **Swagger UI**: `https://localhost:7071/swagger` (adjust port if different).



