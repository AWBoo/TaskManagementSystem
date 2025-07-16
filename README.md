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
* **State Management**: Redux
* **UI Library**:  Custom Styling (Css)

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
    * Configure CORS Policy: In Program.cs, locate the CORS policy definition (e.g., builder.Services.AddCors(...) and app.UseCors(...)). Ensure the WithOrigins method includes the URL where your frontend application         will run (e.g., http://localhost:5173).
    * Apply migrations: `dotnet ef database update`.
3.  **Frontend Setup** (in `frontend/task-manager-frontend`):
    * `npm install` (or `yarn install`)
    * Configure API base URL in `.env` (e.g., `VITE_API_BASE_URL=https://localhost:7071`)(there is a config file .envConfig. take this file copy it. rename and remove the COFNIG and then that the .env file ).

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
    (SPA typically opens on `http://localhost:5173` that was what my local host ended up being).

## API Endpoints

The API provides endpoints for Authentication, User Management (Admin), Projects, and Tasks. Full details and schema are available via **Swagger UI** once the backend is running.

* **Swagger UI**: `https://localhost:7071/swagger` (adjust port if different).



