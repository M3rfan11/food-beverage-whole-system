# 🚀 API Endpoint Documentation

## Overview
This document provides comprehensive documentation for all endpoints in the Food & Beverage Management System API. The API implements Authentication & Authorization with Role-Based Access Control (RBAC).

---

## 📋 Table of Contents
1. [Authentication Endpoints](#authentication-endpoints)
2. [User Profile Management](#user-profile-management)
3. [Admin Dashboard Endpoints](#admin-dashboard-endpoints)
4. [Error Responses](#error-responses)
5. [Authentication & Authorization](#authentication--authorization)

---

## 🔐 Authentication Endpoints

### 1. Register New User
**Endpoint:** `POST /api/auth/register`  
**Authentication:** None required (Public)  
**Purpose:** Allows new users to create accounts without admin approval

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
  "id": 6,
  "fullName": "John Doe",
  "email": "john@example.com",
  "isActive": true,
  "createdAt": "2025-09-26T15:30:00.000Z",
  "updatedAt": null,
  "roles": ["User"]
}
```

**Functionality:**
- Creates a new user account
- Automatically assigns "User" role
- Hashes password using BCrypt
- Validates email uniqueness
- Logs registration in audit trail

---

### 2. User Login
**Endpoint:** `POST /api/auth/login`  
**Authentication:** None required (Public)  
**Purpose:** Authenticates users and returns JWT tokens

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
  "refreshToken": "7NjZzpKJIYbeuwN0Cly4ZXl7IkvTxxHsoDxWFwkmyziHOd140St0MqstT/+gHrcHXQCWWXcjBGXSXbillGGZDA==",
  "expiresAt": "2025-09-26T16:30:00.000Z",
  "user": {
    "id": 5,
    "fullName": "System Administrator",
    "email": "admin@example.com",
    "isActive": true,
    "createdAt": "2025-09-26T14:13:21.735072",
    "roles": ["Admin"]
  }
}
```

**Functionality:**
- Validates email and password
- Returns JWT access token (1 hour expiry)
- Returns refresh token (7 days expiry)
- Includes user information and roles
- Logs login attempt in audit trail

---

### 3. Refresh Token
**Endpoint:** `POST /api/auth/refresh`  
**Authentication:** None required (Public)  
**Purpose:** Generates new access token using refresh token

**Request Body:**
```json
{
  "refreshToken": "your-refresh-token-here"
}
```

**Response:**
```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "new-refresh-token-here",
  "expiresAt": "2025-09-26T17:30:00.000Z"
}
```

**Functionality:**
- Validates refresh token
- Generates new access token
- Generates new refresh token
- Extends session without re-login

---

### 4. Get Current User Info
**Endpoint:** `GET /api/auth/me`  
**Authentication:** Required (Any authenticated user)  
**Purpose:** Returns current authenticated user's information

**Response:**
```json
{
  "id": 5,
  "fullName": "System Administrator",
  "email": "admin@example.com",
  "isActive": true,
  "createdAt": "2025-09-26T14:13:21.735072",
  "roles": ["Admin"]
}
```

**Functionality:**
- Extracts user ID from JWT token
- Returns current user's profile
- Includes user roles
- Useful for frontend user context

---

## 👤 User Profile Management

### 5. Get User Profile
**Endpoint:** `GET /api/userprofile/profile`  
**Authentication:** Required (Any authenticated user)  
**Purpose:** Users can view their own profile information

**Response:**
```json
{
  "id": 5,
  "fullName": "System Administrator",
  "email": "admin@example.com",
  "isActive": true,
  "createdAt": "2025-09-26T14:13:21.735072",
  "updatedAt": null,
  "roles": ["Admin"]
}
```

**Functionality:**
- Returns authenticated user's profile
- Self-service profile viewing
- Includes role information
- Logs profile access

---

### 6. Update User Profile
**Endpoint:** `PATCH /api/userprofile/profile`  
**Authentication:** Required (Any authenticated user)  
**Purpose:** Users can update their own profile information

**Request Body:**
```json
{
  "fullName": "John Smith",
  "email": "johnsmith@example.com"
}
```

**Response:**
```json
{
  "id": 5,
  "fullName": "John Smith",
  "email": "johnsmith@example.com",
  "isActive": true,
  "createdAt": "2025-09-26T14:13:21.735072",
  "updatedAt": "2025-09-26T15:30:00.000Z",
  "roles": ["Admin"]
}
```

**Functionality:**
- Updates user's own profile only
- Validates email uniqueness
- Updates timestamp
- Logs profile changes in audit trail
- Cannot change roles (admin-only function)

---

### 7. Deactivate User Account
**Endpoint:** `PATCH /api/userprofile/deactivate`  
**Authentication:** Required (Any authenticated user)  
**Purpose:** Users can deactivate their own account (soft delete)

**Response:**
```json
{
  "id": 5,
  "fullName": "System Administrator",
  "email": "admin@example.com",
  "isActive": false,
  "createdAt": "2025-09-26T14:13:21.735072",
  "updatedAt": "2025-09-26T15:30:00.000Z",
  "roles": ["Admin"]
}
```

**Functionality:**
- Sets user's `IsActive` to false
- Prevents login attempts
- Soft delete (data preserved)
- Logs deactivation in audit trail
- User can be reactivated by admin

---

## 🏢 Admin Dashboard Endpoints

### 8. Get System Statistics
**Endpoint:** `GET /api/admin/stats`  
**Authentication:** Required (Admin role only)  
**Purpose:** Provides system overview and metrics for admin dashboard

**Response:**
```json
{
  "totalUsers": 3,
  "activeUsers": 2,
  "inactiveUsers": 1,
  "totalRoles": 3,
  "totalAuditLogs": 113,
  "usersByRole": [
    {"role": "Admin", "count": 1},
    {"role": "User", "count": 2}
  ]
}
```

**Functionality:**
- Counts total users (active/inactive)
- Counts total roles
- Counts audit log entries
- Groups users by role
- Provides dashboard metrics
- Admin-only access

---

### 9. Get All Users
**Endpoint:** `GET /api/admin/users`  
**Authentication:** Required (Admin role only)  
**Purpose:** Admin can view all users in the system

**Response:**
```json
[
  {
    "id": 2,
    "fullName": "Ahmed",
    "email": "Ahmed@gmail.com",
    "isActive": true,
    "createdAt": "2025-09-24T12:27:38.063535",
    "updatedAt": null,
    "roles": ["User"]
  },
  {
    "id": 5,
    "fullName": "System Administrator",
    "email": "admin@example.com",
    "isActive": true,
    "createdAt": "2025-09-26T14:13:21.735072",
    "updatedAt": null,
    "roles": ["Admin"]
  }
]
```

**Functionality:**
- Lists all users in system
- Includes user roles
- Shows active/inactive status
- Admin-only access
- Logs user listing

---

### 10. Get User by ID
**Endpoint:** `GET /api/admin/users/{id}`  
**Authentication:** Required (Admin role only)  
**Purpose:** Admin can view specific user details

**Response:**
```json
{
  "id": 5,
  "fullName": "System Administrator",
  "email": "admin@example.com",
  "isActive": true,
  "createdAt": "2025-09-26T14:13:21.735072",
  "updatedAt": null,
  "roles": ["Admin"]
}
```

**Functionality:**
- Returns specific user by ID
- Includes user roles
- Admin-only access
- Logs user access
- Returns 404 if user not found

---

### 11. Create User (Admin)
**Endpoint:** `POST /api/admin/users`  
**Authentication:** Required (Admin role only)  
**Purpose:** Admin can create new user accounts

**Request Body:**
```json
{
  "fullName": "Store Manager",
  "email": "storemanager@example.com",
  "password": "StoreManager123!",
  "isActive": true
}
```

**Response:**
```json
{
  "id": 6,
  "fullName": "Store Manager",
  "email": "storemanager@example.com",
  "isActive": true,
  "createdAt": "2025-09-26T15:30:00.000Z",
  "updatedAt": null,
  "roles": ["User"]
}
```

**Functionality:**
- Creates new user account
- Assigns default "User" role
- Hashes password securely
- Validates email uniqueness
- Admin-only access
- Logs user creation

---

### 12. Assign Role to User
**Endpoint:** `POST /api/admin/users/{id}/roles`  
**Authentication:** Required (Admin role only)  
**Purpose:** Admin can assign roles to users

**Request Body:**
```json
{
  "roleId": 3
}
```

**Response:**
```json
{
  "message": "Role assigned successfully",
  "user": {
    "id": 2,
    "fullName": "Ahmed",
    "email": "Ahmed@gmail.com",
    "roles": ["User", "Manager"]
  }
}
```

**Functionality:**
- Assigns role to specific user
- Validates role exists
- Prevents duplicate role assignment
- Admin-only access
- Logs role assignment

---

### 13. Get All Roles
**Endpoint:** `GET /api/admin/roles`  
**Authentication:** Required (Admin role only)  
**Purpose:** Admin can view all available roles

**Response:**
```json
[
  {
    "id": 1,
    "name": "Admin",
    "description": "System Administrator",
    "createdAt": "2025-09-24T11:17:06.427411"
  },
  {
    "id": 2,
    "name": "User",
    "description": "Regular User",
    "createdAt": "2025-09-24T11:17:06.42756"
  },
  {
    "id": 3,
    "name": "Manager",
    "description": "Team Manager",
    "createdAt": "2025-09-24T11:17:06.427561"
  }
]
```

**Functionality:**
- Lists all system roles
- Shows role descriptions
- Admin-only access
- Used for role assignment UI
- Logs role listing

---

### 14. Create New Role
**Endpoint:** `POST /api/admin/roles`  
**Authentication:** Required (Admin role only)  
**Purpose:** Admin can create new roles for the system

**Request Body:**
```json
{
  "name": "Supervisor",
  "description": "Supervisor role for overseeing operations"
}
```

**Response:**
```json
{
  "id": 4,
  "name": "Supervisor",
  "description": "Supervisor role for overseeing operations",
  "createdAt": "2025-09-26T15:30:00.000Z"
}
```

**Functionality:**
- Creates new role
- Validates role name uniqueness
- Admin-only access
- Logs role creation
- Used for expanding role system

---

## ❌ Error Responses

### Common Error Codes

**400 Bad Request:**
```json
{
  "message": "Invalid request data",
  "errors": {
    "email": ["Email is required"],
    "password": ["Password must be at least 8 characters"]
  }
}
```

**401 Unauthorized:**
```json
{
  "message": "Invalid email or password"
}
```

**403 Forbidden:**
```json
{
  "message": "Access denied. Admin role required."
}
```

**404 Not Found:**
```json
{
  "message": "User not found"
}
```

**409 Conflict:**
```json
{
  "message": "Email already exists"
}
```

---

## 🔒 Authentication & Authorization

### JWT Token Structure
- **Access Token:** 1 hour expiry, contains user ID, email, roles
- **Refresh Token:** 7 days expiry, used to generate new access tokens
- **Bearer Token:** Include in Authorization header: `Bearer {token}`

### Role-Based Access Control
- **Admin:** Full access to all endpoints
- **User:** Access to own profile and public endpoints
- **Other Roles:** Can be assigned by admin for specific permissions

### Security Features
- Password hashing with BCrypt
- JWT token validation
- Role-based endpoint protection
- Audit logging for all actions
- Email uniqueness validation
- Soft delete for user accounts

---

## 🎯 Use Cases

### For Regular Users:
1. Register account → Login → View/Update profile → Deactivate account

### For Admins:
1. Login → View system stats → Manage users → Assign roles → Create roles

### For Food & Beverage System:
- **User Management:** Staff registration and role assignment
- **Role Management:** Define permissions for different staff types
- **Audit Trail:** Track all system changes
- **Self-Service:** Users manage their own profiles

---

## 📝 Notes

- All mutating operations are logged in the audit trail
- Passwords are never returned in responses
- Email addresses must be unique
- User deactivation is reversible by admin
- JWT tokens should be stored securely on client side
- Refresh tokens should be rotated on each use

---

*This API serves as the foundation for the Food & Beverage Management System, providing secure authentication and user management capabilities.*
