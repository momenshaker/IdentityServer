# Identity Server (OpenIddict Authentication & API Project)

This project demonstrates how to integrate **OpenIddict** for authentication and authorization, connect to a **SQL Server** database using **Entity Framework Core**, implement **Swagger/OpenAPI** documentation, provide **Health Checks**, and support **Localization** in an ASP.NET Core web application. It also includes automatic application of database migrations at startup and integration with **Azure Key Vault** for managing encryption and signing certificates.

## Features

- **OpenIddict Authentication**: Implements OpenIddict for OAuth2 and OpenID Connect authentication flows, including support for JWT tokens.
- **Entity Framework Core**: SQL Server integration with automatic database migrations on startup.
- **Swagger/OpenAPI**: API documentation via Swagger UI for easy testing and exploration.
- **Health Checks**: Health check endpoints to monitor application and service status.
- **Localization Support**: Configured for multiple cultures (English and Arabic) with customized date/time formatting.
- **Certificate Management**: Supports encryption and signing certificates from Azure Key Vault or file storage.
- **Exception Handling Middleware**: Graceful error handling and logging through custom middleware.

## Prerequisites

- **.NET 6 or later**: This project requires .NET 6 or later.
- **SQL Server**: The application uses SQL Server for database management.
- **Azure Key Vault (optional)**: Used for storing encryption and signing certificates securely.
- **Visual Studio or VS Code**: Recommended IDEs for working with .NET projects.

## Setup Instructions

### 1. Clone the repository

```
git clone https://github.com/your-repository.git
```

### 2\. Install Dependencies

Restore the necessary NuGet packages for the project.

`dotnet restore`

### 3\. Configure the Application

Edit `appsettings.json` to configure the database connection string and certificates.

Example `appsettings.json`:

`{
  "ConnectionStrings": {
    "DefaultConnection": "Server=your_server;Database=your_db;User Id=your_user;Password=your_password;"
  },
  "OpenIdServer": {
    "EncryptionCertificate": {
      "StoreType": "KeyVault",
      "CertificateName": "your-encryption-certificate",
      "StoreName": "your-keyvault-name"
    },
    "SigningCertificate": {
      "StoreType": "File",
      "CertificateName": "your-signing-certificate",
      "StoreName": "Certificates"
    }
  },
  "Url": {
    "ServerSiteUrl": "https://your-server-url"
  }
}`

### 4\. Run the Application

After configuring the settings, build and run the application with:

`dotnet run`

The application will be accessible at `https://localhost:5001` or the configured URL.

Features Overview
-----------------

### 1\. **OpenIddict Authentication**

-   **OAuth2 & OpenID Connect**: Configured to support password flow, client credentials flow, and refresh token flow.
-   **Token Management**: Tokens are issued with configurable expiration times and are validated using JWT tokens.
-   **Custom Scopes**: `email`, `profile`, and `roles` are registered as scopes in OpenIddict.
-   **Authentication Middleware**: The application uses JWT Bearer Authentication for secured endpoints.

### 2\. **Database Configuration (SQL Server)**

-   The application uses **Entity Framework Core** to interact with the SQL Server database.
-   Database migrations are automatically applied at startup using `context.Database.MigrateAsync()`.
-   **Retry Logic**: Configured to enable automatic retries for database operations in case of failure.

### 3\. **Swagger/OpenAPI Documentation**

-   **Swagger UI**: Swagger is configured to provide interactive API documentation during development.
-   **Testing**: You can test all the API endpoints directly from the Swagger UI.
-   Access the Swagger documentation at `https://localhost:5001/swagger`.

### 4\. **Health Checks**

-   The application provides health check endpoints to monitor the status of the application and services.
-   Access the health check at `https://localhost:5001/health`.

### 5\. **Localization Support**

-   The application supports **English** (`en`) and **Arabic** (`ar`) cultures.
-   Custom date/time formatting is provided using the **Gregorian Calendar** for both cultures.
-   Localization can be expanded by adding more cultures in the `_supportedCultures` array.

### 6\. **Certificate Management**

-   Supports **Azure Key Vault** or **File System** for storing encryption and signing certificates.
-   Encryption and signing certificates are retrieved based on the configuration in `appsettings.json`.

### 7\. **Exception Handling Middleware**

-   A custom **Exception Middleware** catches and logs exceptions globally.
-   Provides consistent error responses for better debugging and error tracking.

Directory Structure
-------------------
`.
├── Controllers
│   └── YourController.cs
├── Services
│   ├── AccountService.cs
│   └── IAccountService.cs
├── Middlewares
│   └── ExceptionMiddleware.cs
├── Core
│   ├── Context
│   │   └── ApplicationDbContext.cs
│   ├── Data
│   │   └── Models
│   ├── Options
│   └── DataSeed
├── Properties
├── appsettings.json
└── Program.cs`

Configuration Options
---------------------

-   **OpenIddict Authentication**:
    -   Can be configured for different token lifetime durations.
    -   Supports multiple authentication flows (password, client credentials, etc.).
-   **Database Connection**:
    -   Configured in `appsettings.json` under the `ConnectionStrings` section.
-   **Localization**:
    -   Add more cultures by expanding the `_supportedCultures` array in `Program.cs`.
-   **Certificates**:
    -   Configure whether to use certificates from Azure Key Vault or a file system in `appsettings.json` under the `OpenIdServer` section.

Troubleshooting
---------------

### Database Migrations Not Applying

-   If pending migrations are not being applied at startup, ensure the connection string is correct and the database is accessible.

### Culture Not Working Correctly

-   If cultures are not being applied correctly, check the `RequestLocalizationOptions` configuration in `Program.cs` to ensure that the supported cultures are correctly defined.

### Certificates Not Found

-   Ensure that the correct certificate store type (Azure Key Vault or File) is specified in `appsettings.json`.
-   If using file-based certificates, make sure the certificate files exist at the specified location.

License
-------

This project is licensed under the **MIT License**. See the LICENSE file for more information.

Acknowledgements
----------------

-   OpenIddict
-   Swagger/OpenAPI
-   Microsoft .NET
-   Azure SDK
