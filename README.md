# Auth & RBAC API

A comprehensive .NET 9 Web API implementation featuring JWT authentication, role-based access control (RBAC), and comprehensive audit logging.

## Features

- 🔐 **JWT Authentication** - Secure token-based authentication with access and refresh tokens
- 👥 **Role-Based Access Control** - Fine-grained authorization with Admin, Manager, and User roles
- 👤 **User Management** - Complete CRUD operations for user management
- 🎭 **Role Management** - Create, update, and delete roles
- 🔗 **User-Role Assignment** - Assign and remove roles from users
- 📝 **Audit Logging** - Comprehensive logging of all mutating operations
- 🛡️ **Security** - Password hashing with BCrypt, input validation, and CORS support

## Quick Start

### Prerequisites
- .NET 9 SDK
- SQL Server (LocalDB or full instance)

### Installation

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd gradproject/Api
   ```

2. **Update Configuration**
   - Update the connection string in `appsettings.json` for your database
   - Update the JWT key in `appsettings.json` (use a secure, random key)

3. **Run the Application**
   ```bash
   dotnet run
   ```

4. **Access the API**
   - API: `https://localhost:7000`
   - Swagger UI: `https://localhost:7000/swagger`

### Default Admin Credentials
- **Email**: admin@example.com
- **Password**: Admin123!
- **Role**: Admin

## API Endpoints

### Authentication
- `POST /api/auth/login` - Login with email and password
- `POST /api/auth/refresh` - Refresh access token

### User Management (Admin/Manager roles required)
- `GET /api/users` - Get all users
- `GET /api/users/{id}` - Get user by ID
- `POST /api/users` - Create new user
- `PATCH /api/users/{id}` - Update user
- `DELETE /api/users/{id}` - Delete user
- `POST /api/users/{id}/roles` - Assign role to user
- `DELETE /api/users/{id}/roles/{roleId}` - Remove role from user

### Role Management (Admin role required)
- `GET /api/roles` - Get all roles
- `GET /api/roles/{id}` - Get role by ID
- `POST /api/roles` - Create new role
- `PATCH /api/roles/{id}` - Update role
- `DELETE /api/roles/{id}` - Delete role

## Database Schema

The application uses Entity Framework Core with the following entities:

- **User** - User accounts with authentication information
- **Role** - System roles (Admin, Manager, User)
- **UserRole** - Many-to-many relationship between users and roles
- **AuditLog** - Comprehensive audit trail of all operations

## Security Features

- **Password Hashing**: BCrypt for secure password storage
- **JWT Tokens**: Industry-standard token-based authentication
- **Role-based Authorization**: Fine-grained access control
- **Audit Logging**: Complete audit trail for compliance
- **Input Validation**: Comprehensive request validation
- **CORS Support**: Configurable cross-origin resource sharing

## Testing

Use the provided `Api.http` file with Visual Studio Code REST Client extension or any HTTP client to test the API endpoints.

## Configuration

### JWT Settings
```json
{
  "Jwt": {
    "Key": "YourSuperSecretKeyThatIsAtLeast32CharactersLong!",
    "Issuer": "AuthRBACApi",
    "Audience": "AuthRBACApiUsers",
    "AccessTokenExpirationMinutes": "60"
  }
}
```

### Database Connection
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=AuthRBACDb;Trusted_Connection=true;MultipleActiveResultSets=true"
  }
}
```

## Architecture

The application follows clean architecture principles with:

- **Models** - Entity models and DTOs
- **Services** - Business logic and authentication services
- **Controllers** - API endpoints and request handling
- **Middleware** - Custom audit logging middleware
- **Data** - Entity Framework DbContext and database configuration

## Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests if applicable
5. Submit a pull request

## License

This project is licensed under the MIT License.
