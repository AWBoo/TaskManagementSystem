Task Management System

This repository contains a full-stack Task Management System developed as a technical assessment for Claer & Volker. It features an ASP.NET Core REST API backend and a modern Single-Page Application (SPA) frontend.

Table of Contents

    Features

    Technology Stack

    Repository Structure

    Setup Instructions

        Prerequisites

        Cloning the Repository

        Backend Setup

        Frontend Setup

    Running the Applications

    API Endpoints

    Bonus Features

    Code Quality & Best Practices

Features

The system provides the following core functionalities:

    Authentication & Authorization: User registration, login, and JWT-based authentication for protecting API endpoints. All users are authenticated, and specific administrative functions are restricted to Admin users.

User Management (Admin Role): Admins can list all users, change user roles (User/Admin), and activate/deactivate user accounts.

Project Management: Users can create, list, retrieve details for, update, and delete their own projects. Deleting a project cascades to its associated tasks.

Task Management: Users can create, list, update, and delete tasks within their projects.

Technology Stack

Backend 

    Language: C# (.NET 8.0) 

Framework: ASP.NET Core Web API 

Authentication: JWT tokens (using Microsoft.AspNetCore.Authentication.JwtBearer) 

Data Access: Entity Framework Core (Code-First migrations) 

Dependency Injection: Built-in ASP.NET Core DI container 

Logging: Microsoft.Extensions.Logging (configured for console and potentially file output) 

Documentation: Swagger/OpenAPI 

Password Hashing: BCrypt.Net.BCrypt for storing hashed and salted passwords 

Frontend 

    Framework: React with TypeScript 

State Management: (Optional - if used, specify here e.g., React Context API, Redux) 

UI Library: (Optional - if used, specify here e.g., Tailwind CSS, Material-UI, custom styling) 

Database 

    Choice: SQL Server (or specify if you changed to PostgreSQL/MySQL) 

Schema: Code-First via EF Core migrations, including foreign keys and indexes 

Repository Structure 

TaskManagementSystem/
├── backend/                  # ASP.NET Core API project [cite: 147]
│   ├── backend.csproj
│   ├── ... (C# source files, Controllers, Services, Repositories, Migrations)
│   └── appsettings.json
├── frontend/                 # SPA project (e.g., React, Angular, Vue) [cite: 148]
│   └── task-manager-frontend/ # Your specific frontend project root
│       ├── public/
│       ├── src/
│       ├── package.json
│       ├── tsconfig.json
│       └── ... (Frontend source files)
├── README.md                 # This file - setup & usage instructions 
└── .gitignore                # Global Git ignore rules for both projects

Setup Instructions 

Prerequisites 

Before you begin, ensure you have the following installed:

    .NET SDK 8.0 or later (for the backend). You can download it from dot.net.

    Node.js LTS (Long Term Support version, for the frontend). You can download it from nodejs.org. This usually includes npm.

    npm (Node Package Manager) or Yarn (comes with Node.js, or install Yarn globally via npm install -g yarn).

    SQL Server (or PostgreSQL/MySQL, depending on your choice). Ensure it's running and accessible. If using SQL Server LocalDB, it typically installs with Visual Studio.

    Git (for cloning the repository).

Cloning the Repository 

    Open your terminal or command prompt.

    Navigate to the directory where you want to store the project.

    Clone the repository:
    Bash

git clone https://github.com/AWBoo/TaskManagementSystem.git

Navigate into the cloned project directory:
Bash

    cd TaskManagementSystem

Backend Setup 

    Navigate to the Backend Directory:
    Bash

cd backend/backend

(Note: Assuming your ASP.NET Core project is named "backend" inside the "backend" folder)

Restore Dependencies:
Bash

dotnet restore

Configure Database Connection String:

    Open appsettings.json (and appsettings.Development.json for development).

    Locate the ConnectionStrings section and update the DefaultConnection to point to your SQL Server instance (or other database).
    Example for SQL Server LocalDB:
    JSON

    "ConnectionStrings": {
      "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=TaskManagerDb;Trusted_Connection=True;MultipleActiveResultSets=true"
    },

    Crucial: Ensure your database server is running and accessible.

Apply Database Migrations: 

Bash

dotnet ef database update

This will create the database schema and apply any pending migrations.

Run the Backend (Optional for Setup, but good to test):
Bash

    dotnet run

    The API should start, usually on https://localhost:7071 (check your console output). Swagger UI should be available at https://localhost:7071/swagger.

Frontend Setup 

    Navigate to the Frontend Project Directory:
    Bash

cd ../../frontend/task-manager-frontend

(Adjust path if your frontend project is directly under frontend/)

Install Node.js Dependencies:
Bash

npm install
# OR if you use Yarn:
# yarn install

Configure API Base URL:

    Open the .env file (or src/config.js or similar, depending on your React setup) in your frontend/task-manager-frontend directory.

    Set the environment variable for your backend API base URL, typically:

    VITE_API_BASE_URL=https://localhost:7071 # For Vite/React
    # Or REACT_APP_API_BASE_URL=https://localhost:7071 # For Create React App

    (Adjust the port to match your backend's running port).

Run the Frontend (Optional for Setup, but good to test):
Bash

    npm run dev
    # OR if you use Yarn:
    # yarn dev

    The frontend application should open in your browser, usually on http://localhost:5173 (check your console output).

Running the Applications

    Start the Backend API:
    Open a terminal in backend/backend and run:
    Bash

dotnet run

The API will usually be available at https://localhost:7071.

Start the Frontend SPA:
Open a separate terminal in frontend/task-manager-frontend and run:
Bash

    npm run dev
    # OR
    # yarn dev

    The frontend application will typically open in your default browser (e.g., http://localhost:5173).

API Endpoints 

You can interact with the API using tools like Postman, Insomnia, or curl. Swagger UI is also available at https://localhost:7071/swagger.

Base URL: https://localhost:7071/api (adjust port if different)

Authentication

    Register User:

    POST /api/auth/register 

JSON

{
  "email": "user@example.com",
  "password": "StrongPassword123!"
}

Login User:

POST /api/auth/login 

JSON

    {
      "email": "user@example.com",
      "password": "StrongPassword123!"
    }

    Returns JWT token.

User Management (Admin Only)

    List All Users:

    GET /api/admin/users 


(Requires Admin role token in Authorization: Bearer <token> header)

Change User Role:

PUT /api/admin/users/{id}/role 


(Requires Admin role token)
JSON

{
  "userId": "guid-of-user",
  "roleName": "Admin" // or "User"
}

Deactivate/Reactivate User:

PUT /api/admin/users/{id}/deactivate 


(Requires Admin role token)
JSON

    {
      "userId": "guid-of-user",
      "isActive": false // or true
    }

Projects

    List Current User's Projects:

    GET /api/projects 


(Requires authenticated user token)

Create Project:

POST /api/projects 

JSON

{
  "name": "New Project Name",
  "description": "Description of the project."
}

Get Project Details:

GET /api/projects/{id} 

Update Project:

PUT /api/projects/{id} 

JSON

{
  "name": "Updated Project Name",
  "description": "New description."
}

Delete Project:

DELETE /api/projects/{id} 

Tasks

    List Tasks for a Project:

    GET /api/projects/{projectId}/tasks 

Create Task:

POST /api/projects/{projectId}/tasks 

JSON

{
  "title": "Task Title",
  "description": "Task description.",
  "dueDate": "2025-12-31T23:59:59Z",
  "status": "ToDo" // or "InProgress", "Done"
}

Update Task:

PUT /api/tasks/{id} 

JSON

{
  "title": "Updated Task Title",
  "description": "New task description.",
  "dueDate": "2026-01-15T10:00:00Z",
  "status": "InProgress"
}

Delete Task:

DELETE /api/tasks/{id} 

Bonus Features 

This project includes the following additional features:

    Dashboard Statistics: The backend provides endpoints to retrieve dashboard-level statistics, such as task counts by status for projects and users, and overall user/project counts. (Endpoints for these would be in your AdminService or a dedicated DashboardController).

    Audit & History: Timestamps for created/updated entities are recorded. (If you implemented 

    GET /api/tasks/{id}/history, mention that here with endpoint details).

Code Quality & Best Practices

The project adheres to the following best practices:

    Layered Architecture: Clear separation of concerns with Controllers, Services (business logic), and Repositories/DbContext.

Dependency Injection: Use of interfaces for DI, enabling better testability and maintainability.

Security: Passwords are hashed and salted using BCrypt. JWTs are used for authentication with a strong symmetric key and sensible expiration.

Logging: Comprehensive structured logging (info, warning, error) using Microsoft.Extensions.Logging.

Naming Conventions: PascalCase for types and methods, camelCase for parameters and variables.

Self-Documenting Code: Emphasis on clear code with comments only for complex logic.

Migration Scripts: EF Core migrations are checked in and can be applied directly.
