# Auth & RBAC API Documentation

## Overview
This API provides comprehensive authentication and role-based access control (RBAC) functionality with audit logging.

## Features
- JWT-based authentication with access and refresh tokens
- Role-based authorization (Admin, Manager, User roles)
- User management (CRUD operations)
- Role management (CRUD operations)
- User-role assignment
- Comprehensive audit logging for all mutating operations

## Default Admin User
- **Email**: admin@example.com
- **Password**: Admin123!
- **Role**: Admin

## Available User Roles
- **Admin**: System Administrator - Full access to everything
- **User**: Regular User - Can manage own account and make requests
- **StoreManager**: Store Manager - Manages assigned store, products, and inventory
- **WarehouseManager**: Warehouse Manager - Manages warehouse operations and stock
- **SalesStaff**: Sales Staff - Handles sales and customer orders
- **PurchaseStaff**: Purchase Staff - Handles purchase orders and supplier management

## Authentication Endpoints

### POST /api/auth/register
Register a new user account (No authentication required).

**Request Body:**
```json
{
  "fullName": "John Doe",
  "email": "john@example.com",
  "password": "SecurePassword123!",
  "isActive": true
}
```

**Response:**
```json
{
  "id": 2,
  "fullName": "John Doe",
  "email": "john@example.com",
  "isActive": true,
  "createdAt": "2024-01-01T00:00:00Z",
  "updatedAt": null,
  "roles": ["User"]
}
```

### POST /api/auth/login
Login with email and password to get access and refresh tokens.

**Request Body:**
```json
{
  "email": "admin@example.com",
  "password": "Admin123!"
}
```

**Response:**
```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "base64-encoded-refresh-token",
  "expiresAt": "2024-01-01T12:00:00Z",
  "user": {
    "id": 1,
    "fullName": "System Administrator",
    "email": "admin@example.com",
    "isActive": true,
    "createdAt": "2024-01-01T00:00:00Z",
    "roles": ["Admin"]
  }
}
```

### POST /api/auth/refresh
Refresh access token using refresh token.

**Request Body:**
```json
{
  "refreshToken": "base64-encoded-refresh-token"
}
```

### GET /api/auth/me
Get current user information (Authentication required).

**Headers:**
```
Authorization: Bearer <access-token>
```

**Response:**
```json
{
  "id": 2,
  "fullName": "John Doe",
  "email": "john@example.com",
  "isActive": true,
  "createdAt": "2024-01-01T00:00:00Z",
  "updatedAt": null,
  "roles": ["User"]
}
```

## User Profile Endpoints (Authentication required)

### GET /api/userprofile/profile
Get current user's profile information.

**Headers:**
```
Authorization: Bearer <access-token>
```

### PATCH /api/userprofile/profile
Update current user's profile information.

**Headers:**
```
Authorization: Bearer <access-token>
```

**Request Body:**
```json
{
  "fullName": "John Smith",
  "email": "johnsmith@example.com",
  "password": "NewPassword123!"
}
```

### DELETE /api/userprofile/account
Delete current user's account permanently.

**Headers:**
```
Authorization: Bearer <access-token>
```

**Response:**
```json
{
  "message": "Account deleted successfully"
}
```

### PATCH /api/userprofile/deactivate
Deactivate current user's account (soft delete - can be reactivated by admin).

**Headers:**
```
Authorization: Bearer <access-token>
```

**Response:**
```json
{
  "id": 2,
  "fullName": "John Doe",
  "email": "john@example.com",
  "isActive": false,
  "createdAt": "2024-01-01T00:00:00Z",
  "updatedAt": "2024-01-01T12:00:00Z",
  "roles": ["User"]
}
```

## Admin Dashboard Endpoints (Admin Role Required)

### User Management
- `GET /api/admin/users` - Get all users
- `GET /api/admin/users/{id}` - Get user by ID
- `POST /api/admin/users` - Create new user
- `PATCH /api/admin/users/{id}` - Update user
- `DELETE /api/admin/users/{id}` - Delete user
- `POST /api/admin/users/{id}/roles` - Assign role to user
- `DELETE /api/admin/users/{id}/roles/{roleId}` - Remove role from user

### Role Management
- `GET /api/admin/roles` - Get all roles
- `GET /api/admin/roles/{id}` - Get role by ID
- `POST /api/admin/roles` - Create new role
- `PATCH /api/admin/roles/{id}` - Update role
- `DELETE /api/admin/roles/{id}` - Delete role

### System Statistics
- `GET /api/admin/stats` - Get system statistics and user counts

## Legacy User Management Endpoints (Deprecated - Use Admin endpoints)

### GET /api/users
Get all users (Admin, Manager roles required).

**Headers:**
```
Authorization: Bearer <access-token>
```

### GET /api/users/{id}
Get user by ID (Admin, Manager roles required).

### POST /api/users
Create new user (Admin role required).

**Request Body:**
```json
{
  "fullName": "John Doe",
  "email": "john@example.com",
  "password": "SecurePassword123!",
  "isActive": true
}
```

### PATCH /api/users/{id}
Update user (Admin role required).

**Request Body:**
```json
{
  "fullName": "John Smith",
  "email": "johnsmith@example.com",
  "password": "NewPassword123!",
  "isActive": false
}
```

### DELETE /api/users/{id}
Delete user (Admin role required).

### POST /api/users/{id}/roles
Assign role to user (Admin role required).

**Request Body:**
```json
{
  "roleId": 2
}
```

### DELETE /api/users/{id}/roles/{roleId}
Remove role from user (Admin role required).

## Legacy Role Management Endpoints (Deprecated - Use Admin endpoints)

### GET /api/roles
Get all roles (Admin role required).

### GET /api/roles/{id}
Get role by ID (Admin role required).

### POST /api/roles
Create new role (Admin role required).

**Request Body:**
```json
{
  "name": "Editor",
  "description": "Content Editor Role"
}
```

### PATCH /api/roles/{id}
Update role (Admin role required).

**Request Body:**
```json
{
  "name": "SeniorEditor",
  "description": "Senior Content Editor Role"
}
```

### DELETE /api/roles/{id}
Delete role (Admin role required).

## Database Schema

### Users Table
- `Id` (int, Primary Key)
- `FullName` (string, Required)
- `Email` (string, Required, Unique)
- `PasswordHash` (string, Required)
- `IsActive` (bool, Default: true)
- `CreatedAt` (DateTime)
- `UpdatedAt` (DateTime?)

### Roles Table
- `Id` (int, Primary Key)
- `Name` (string, Required, Unique)
- `Description` (string?)
- `CreatedAt` (DateTime)

### UserRoles Table (Many-to-Many)
- `UserId` (int, Foreign Key)
- `RoleId` (int, Foreign Key)
- `AssignedAt` (DateTime)

### AuditLogs Table
- `Id` (int, Primary Key)
- `ActorUserId` (int?, Foreign Key)
- `Entity` (string, Required)
- `EntityId` (string, Required)
- `Action` (string, Required)
- `Before` (string?)
- `After` (string?)
- `At` (DateTime)
- `IpAddress` (string?)
- `UserAgent` (string?)

## Security Features

1. **Password Hashing**: Uses BCrypt for secure password hashing
2. **JWT Tokens**: Secure token-based authentication
3. **Role-based Authorization**: Fine-grained access control
4. **Audit Logging**: All mutating operations are logged
5. **Input Validation**: Comprehensive request validation
6. **CORS Support**: Configurable cross-origin resource sharing

## Getting Started

1. Update the connection string in `appsettings.json` for your database
2. Update the JWT key in `appsettings.json` (use a secure, random key)
3. Run the application - the database will be created automatically with seed data
4. Use the default admin credentials to log in and start managing users and roles

## Error Handling

All endpoints return appropriate HTTP status codes:
- `200 OK`: Successful operation
- `201 Created`: Resource created successfully
- `400 Bad Request`: Invalid request data
- `401 Unauthorized`: Authentication required
- `403 Forbidden`: Insufficient permissions
- `404 Not Found`: Resource not found
- `409 Conflict`: Resource already exists
- `500 Internal Server Error`: Server error
