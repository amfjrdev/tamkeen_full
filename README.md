# Tamkeen Full-Stack Monorepo

Welcome to the production-ready monorepo for the **Tamkeen Platform**. This project has been structured into a unified monorepo containing both the .NET API backend and the React Vite frontend dashboard, fully orchestrated using Docker Compose.

---

## Project Structure

```
desktop/tamkeen_full/
├── backend/                # ASP.NET Core 8 Web API
│   ├── src/                # Backend Source code (Domain, Application, Infrastructure, API)
│   ├── tests/              # Unit & integration tests
│   ├── Dockerfile          # Production-ready SDK & runtime multi-stage build
│   └── .env.example        # Environment variables template
├── dashboard/              # React + Vite + TS + Tailwind CSS dashboard frontend
│   ├── src/                # React components & services
│   ├── Dockerfile          # Production multi-stage build (Node build → Nginx static serving)
│   ├── nginx.conf          # Nginx configurations (supports React SPA routing)
│   └── .env.example        # Client API configuration template
├── docker-compose.yml      # Master Docker orchestration configuration
├── .gitignore              # Monorepo level git exclusions
└── README.md               # Setup and development guide (this file)
```

---

## Prerequisites

Ensure you have the following tools installed:
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (includes Docker Compose)
- [Git](https://git-scm.com/)

---

## Getting Started

### 1. Setup Environment Variables

Before starting the containers, copy the template `.env.example` files to `.env` in both folders and configure them.

**Backend & Database Configuration (Root .env):**
Docker Compose reads environment variables from a `.env` file at the root directory to interpolate them in `docker-compose.yml`.
```bash
cp backend/.env.example .env
```
Open the root `.env` file and configure your settings:
- Set your `MSSQL_SA_PASSWORD` (minimum 8 characters, containing uppercase, lowercase, digit, and special char).
- The default connection string (`DB_CONNECTION_STRING`) uses the hostname `database` which resolves automatically inside the Docker network. Make sure the password inside the connection string matches your `MSSQL_SA_PASSWORD`.
- Configure other services like `JWT_SECRET`, `SMTP` settings, and `Chargily` payment credentials if needed.

**Dashboard Configuration:**
```bash
cp dashboard/.env.example dashboard/.env
```
Open `dashboard/.env` and verify the backend API address:
- `VITE_API_URL` should point to `http://localhost:5000/api` (the address of the backend exposed on the host machine).

---

### 2. Build and Run the App

Launch all services in detached mode with:

```bash
docker compose up --build -d
```

This will automatically build the images, create a private network, and launch the following containers:
- **`database`** (SQL Server 2022) at port `1433`
- **`redis`** (Redis 7) at port `6379`
- **`backend`** (.NET API) at port `5000`
- **`dashboard`** (React + Nginx) at port `3000`

---

## Accessing the Platform Services

Once all containers are up and report healthy, you can access them at:

| Service | Port | Description |
| :--- | :--- | :--- |
| **Dashboard** | [http://localhost:3000](http://localhost:3000) | Frontend User Interface |
| **API Endpoints** | [http://localhost:5000](http://localhost:5000) | Backend Web API root |
| **API Documentation** | [http://localhost:5000/swagger](http://localhost:5000/swagger) | Interactive API Playground (Swagger UI) |
| **Health Checks** | [http://localhost:5000/health](http://localhost:5000/health) | Backend health status report |

---

## Helpful Docker Commands

### Stop and Clean Up
To stop the services and keep the data volumes intact:
```bash
docker compose down
```

To stop services and completely wipe out local database volume states:
```bash
docker compose down -v
```

### View Service Logs
To monitor live log output from all services:
```bash
docker compose logs -f
```

To view logs for a specific service:
```bash
docker compose logs -f backend
docker compose logs -f dashboard
```

### Run Backend Database Migrations / Seed
If the database needs to run migrations, the application automatically runs them on startup inside the `backend` container context when running in the Docker container.
