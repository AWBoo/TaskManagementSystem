# Task Management System

This repository contains a full-stack Task Management System developed as a technical assessment for Claer & Volker. It features an ASP.NET Core REST API backend and a modern Single-Page Application (SPA) frontend.

## Table of Contents

* [Features](#features)
* [Technology Stack](#technology-stack)
* [Repository Structure](#repository-structure)
* [Setup Instructions](#setup-instructions)
* [Running the Applications](#running-the-applications)
* [API Endpoints](#api-endpoints)
* [Bonus Features](#bonus-features)
* [Code Quality & Best Practices](#code-quality--best-practices)

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
* **State Management**: (Optional - *if used, specify here e.g., React Context API, Redux*)
* **UI Library**: (Optional - *if used, specify here e.g., Tailwind CSS, Material-UI, custom styling*)

## Setup Instructions

### Prerequisites
* .NET SDK 8.0+
* Node.js LTS (with npm or Yarn)
* SQL Server (or your chosen database)
* Git

### Steps
1.  **Clone the Repository**:
    ```bash
    git clone [https://github.com/AWBoo/TaskManagementSystem.git](https://github.com/AWBoo/TaskManagementSystem.git)
    cd TaskManagementSystem
    ```
2.  **Backend Setup** (in `backend/backend`):
    * `dotnet restore`
    * Update `ConnectionStrings` in `appsettings.json` (and `appsettings.Development.json`).
    * Apply migrations: `dotnet ef database update`[cite: 150].
3.  **Frontend Setup** (in `frontend/task-manager-frontend`):
    * `npm install` (or `yarn install`)
    * Configure API base URL in `.env` (e.g., `VITE_API_BASE_URL=https://localhost:7071`).

## Running the Applications

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
    (SPA typically opens on `http://localhost:5173`).

## API Endpoints

The API provides endpoints for Authentication, User Management (Admin), Projects, and Tasks. Full details and schema are available via **Swagger UI** once the backend is running.

* **Swagger UI**: `https://localhost:7071/swagger` (adjust port if different).

**Key Endpoints Overview:**

* **Authentication**: `POST /api/auth/register`, `POST /api/auth/login`
* **User Management (Admin)**: `GET /api/admin/users`, `PUT /api/admin/users/{id}/role`, `PUT /api/admin/users/{id}/deactivate`
* **Projects**: `GET /api/projects`, `POST /api/projects`, `GET /api/projects/{id}`, `PUT /api/projects/{id}`, `DELETE /api/projects/{id}`
* **Tasks**: `GET /api/projects/{projectId}/tasks`, `POST /api/projects/{projectId}/tasks`, `PUT /api/tasks/{id}`, `DELETE /api/tasks/{id}`

*(Example `curl` or Postman snippets can be added here if specifically requested, but Swagger provides better interaction and detail.)*

## Bonus Features

* **Dashboard Statistics**: Backend provides endpoints for aggregate data (e.g., task counts by status).
* **Audit & History**: Timestamps for created/updated entities are recorded.

## Code Quality & Best Practices
