# 🏢 Office Desk Reservation System

**Office Desk Reservation System** is a comprehensive full-stack web application designed for seamless management of office spaces, rooms, desks, and their bookings. The project features a robust RESTful API built with **ASP.NET Core (.NET 8)** and a modern, responsive user interface developed with **Angular**.

## ✨ Key Features

* **🔐 Authentication & Authorization:** Secure user registration and login using JWT tokens. Supports Role-Based Access Control (RBAC) with Admin and User roles, protected by Angular Guards (`auth.guard.ts`, `role.guard.ts`).
* **🚪 Room Management:** Full CRUD operations for managing office rooms (add, view, edit, delete).
* **🪑 Desk Management:** Associate desks with specific rooms, manage their properties, and track their availability status.
* **📅 Reservation System:** Book desks for specific time slots, view active reservations, and check booking history.
* **👥 User Management:** Dedicated admin panel to view registered users and update their roles.
* **📄 Pagination & Filtering:** Built-in pagination support for efficient data loading and display across large datasets (`PagedResult`, `QueryParameters`).
* **🛡 Validation & Error Handling:** Robust input validation using Data Transfer Objects (DTOs) and centralized global exception handling on the backend.

## 🛠 Tech Stack

### Backend (.NET API)
* **Framework:** .NET 10 / C#
* **Database:** Entity Framework Core (Code-First Migrations)
* **Architecture:** REST API, Dependency Injection, Service/Repository Pattern
* **Mapping:** AutoMapper
* **Testing:** xUnit, Moq

### Frontend (Angular)
* **Framework:** Angular (TypeScript, HTML, SCSS/CSS)
* **Routing:** Angular Router
* **HTTP:** Angular HttpClient with Auth Interceptors
* **Testing:** Karma, Jasmine

### Infrastructure & CI/CD
* **Containerization:** Docker, Docker Compose
* **CI/CD:** GitHub Actions (Automated build and test pipeline)

## 📂 Project Structure

* `OfficeDeskReservation.API/` — Backend source code (Controllers, Services, Models, DTOs, EF Core Configurations).
* `OfficeDeskReservation.UI/` — Frontend source code (Angular Components, Services, Guards, Interceptors).
* `OfficeDeskReservation.Tests/` — Comprehensive unit tests for backend services, controllers, and validators.
* `docker-compose.yml` — Docker configuration for spinning up the entire application stack.

## 🚀 Getting Started

### Prerequisites
Before running the project, ensure you have the following installed:
* [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
* [Node.js](https://nodejs.org/) (v18+ recommended) and npm
* [Docker Desktop](https://www.docker.com/products/docker-desktop/) (if running via containers)

### Method 1: Run with Docker Compose (Recommended)
The easiest way to get everything up and running:
1. Clone the repository.
2. Open a terminal in the root directory (where `docker-compose.yml` is located).
3. Run the following command:
   ```bash
   docker-compose up --build
   ```
4. Once the containers are built and running, the backend API and the Angular UI will be accessible on their respective mapped ports (e.g., UI on `http://localhost:4200`).

### Method 2: Local Development Setup

#### 1. Backend (API) Setup
```bash
cd OfficeDeskReservation.API
# Apply database migrations
dotnet ef database update
# Run the application
dotnet run
```
*The API will typically start on `http://localhost:5000` or `https://localhost:5001`.*

#### 2. Frontend (Angular UI) Setup
Open a new terminal window:
```bash
cd OfficeDeskReservation.UI
# Install dependencies
npm install
# Start the development server
npm start
```
*Navigate to `http://localhost:4200/` in your browser.*

## 🧪 Testing

The project includes unit tests to ensure business logic reliability. 

**To run Backend Tests:**
```bash
cd OfficeDeskReservation.Tests
dotnet test
```

**To run Frontend Tests:**
```bash
cd OfficeDeskReservation.UI
npm run test
```

## 🔄 CI/CD Pipeline

This repository includes a GitHub Actions workflow (`.github/workflows/dotnet-ci.yml`) that automatically triggers on pushes or pull requests to the main branch. It ensures code quality by building the .NET project and running all backend tests automatically.
