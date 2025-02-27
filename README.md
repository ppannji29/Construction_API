📘 Construction_API Documentation

1️⃣ Project Overview
Construction_API is a RESTful API built with .NET Core 8 that manages construction projects:

- Dapper for lightweight, high-performance database operations.
- LINQ for querying data.
- Stored Procedures to optimize SQL execution.
- JWT Authentication for securing API endpoints.

2️⃣ Technologies Used

- Framework: .NET Core 8
- Database: SQL Server
- ORM: Dapper & LINQ
- Authentication: JWT (JSON Web Token)
- API Format: RESTful
- Logging: Serilog

3️⃣ Setup & Installation

Ensure you have the following installed:

- .NET SDK 8
- SQL Server
- Postman (for API testing)
- Swagger UI
- Visual Studio 2022 / VS Code

# Database Configuration & Restoration

Restore Database
- Download file 'App_Contruction_DB' backup file (find in this repository)
- Restore Database
- Select Device
- Select '...'
- Click 'Add', find based on All files
- Select Backup File 'App_Contruction'
- Click OK

## 🔧 Update Database Connection
Ensure your `appsettings.Development.json` & `appsettings.json` file is configured correctly. Update the connection string with your local database:

```json
{
  "ConnectionStrings": {
    "SqlConnection": "Server=your_db; Database=App_Construction; Integrated Security=True; TrustServerCertificate=True;"
  }
}

