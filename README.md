# Azure SQL Database Token Authentication Demo

This repository contains two implementations (Java Spring Boot and .NET) demonstrating Azure AD token-based authentication with Azure SQL Database. Both implementations showcase best practices for token management and connection pooling.

## Features

- Azure AD token-based authentication
- Automatic token refresh before expiration
- Connection pooling with best practices
- Thread-safe token and connection management
- Basic CRUD operations demo

## Project Structure

```
az-token-sqldb/
├── dotnet/                      # .NET 8 implementation
│   └── AzureSqlTokenAuth/
│       ├── src/
│       └── tests/
└── java/                        # Java Spring Boot implementation
    └── azure-sql-token-auth/
        ├── src/
        └── tests/
```

## Prerequisites

- Azure SQL Database instance
- Azure AD application registration with appropriate permissions
- For .NET implementation:
  - .NET 8 SDK
  - Visual Studio 2022 or VS Code
- For Java implementation:
  - Java 17 or later
  - Maven
  - Spring Boot 3.2.x

## Configuration

### Azure Setup

1. Create an Azure SQL Database instance
2. Register an application in Azure AD
3. Grant the application appropriate permissions to access the SQL Database
4. Note down the following values:
   - Azure AD Tenant ID
   - Client ID
   - Client Secret
   - SQL Server name
   - Database name

### .NET Application Configuration

Configure the following in `appsettings.json` or environment variables:

```json
{
  "AzureAd": {
    "TenantId": "your-tenant-id",
    "ClientId": "your-client-id",
    "ClientSecret": "your-client-secret"
  },
  "SqlDatabase": {
    "Server": "your-server-name",
    "Database": "your-database-name",
    "TokenRefreshIntervalMinutes": 45
  }
}
```

### Java Application Configuration

Configure the following in `application.yml` or environment variables:

```yaml
spring:
  security:
    oauth2:
      client:
        registration:
          azure-ad:
            client-id: your-client-id
            client-secret: your-client-secret

azure:
  activedirectory:
    tenant-id: your-tenant-id
  sql:
    database:
      server: your-server-name
      name: your-database-name
      refresh-interval-minutes: 45
```

## Running the Applications

### .NET Application

```bash
cd dotnet/AzureSqlTokenAuth/src/AzureSqlTokenAuth
dotnet build
dotnet run
```

The application will be available at `https://localhost:5001`

### Java Application

```bash
cd java/azure-sql-token-auth/src/azure-sql-token-auth
mvn clean install
mvn spring-boot:run
```

The application will be available at `http://localhost:8080`

## API Endpoints

Both implementations provide the following REST endpoints:

- `POST /api/users` - Create a new user
- `GET /api/users/{id}` - Get a user by ID
- `GET /api/users` - Get all users
- `PUT /api/users/{id}` - Update a user
- `DELETE /api/users/{id}` - Delete a user

### Example Request

Create a new user:

```bash
curl -X POST \
  http://localhost:8080/api/users \
  -H 'Content-Type: application/json' \
  -d '{
    "name": "John Doe",
    "email": "john@example.com"
}'
```

## Implementation Details

### Token Management

Both implementations feature:
- Singleton pattern for token management
- Automatic token refresh before expiration
- Thread-safe token operations
- Connection pool integration

### Connection Pooling

- .NET: Custom connection pool implementation with SqlConnection
- Java: HikariCP connection pool with token integration

## Security Considerations

1. Always use environment variables or secure configuration management for sensitive values
2. Implement proper error handling and logging
3. Follow the principle of least privilege when setting up Azure AD permissions
4. Use HTTPS in production environments
5. Implement proper authentication and authorization in production

## Contributing

1. Fork the repository
2. Create a feature branch
3. Commit your changes
4. Push to the branch
5. Create a pull request