# Food & Beverage Management System - Database ERD Documentation

## Overview
This document provides a comprehensive overview of the Entity Relationship Diagram (ERD) for the Food & Beverage Management System. The system is built using .NET Core with Entity Framework Core and SQLite database.

## Database Schema Summary

### Core Tables
- **Users** - User accounts and authentication
- **Roles** - System roles for RBAC
- **UserRoles** - Many-to-many relationship between users and roles
- **AuditLogs** - System audit trail
- **Categories** - Product categories
- **Products** - Product catalog
- **Warehouses** - Storage locations
- **ProductInventories** - Stock quantities per warehouse

---

## Entity Details

### 1. Users Table
**Purpose**: Stores user account information and authentication data

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| Id | INTEGER | PRIMARY KEY, AUTOINCREMENT | Unique user identifier |
| FullName | TEXT(100) | NOT NULL | User's full name |
| Email | TEXT(255) | NOT NULL, UNIQUE | User's email address |
| PasswordHash | TEXT | NOT NULL | BCrypt hashed password |
| IsActive | INTEGER | DEFAULT 1 | Account status (1=active, 0=inactive) |
| CreatedAt | TEXT | DEFAULT datetime('now') | Account creation timestamp |
| UpdatedAt | TEXT | NULL | Last update timestamp |

**Relationships**:
- One-to-Many with UserRoles
- One-to-Many with AuditLogs (as ActorUser)
- One-to-Many with Warehouses (as ManagerUser)

**Indexes**:
- UNIQUE INDEX on Email

---

### 2. Roles Table
**Purpose**: Defines system roles for Role-Based Access Control (RBAC)

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| Id | INTEGER | PRIMARY KEY, AUTOINCREMENT | Unique role identifier |
| Name | TEXT(50) | NOT NULL, UNIQUE | Role name |
| Description | TEXT(200) | NULL | Role description |
| CreatedAt | TEXT | DEFAULT datetime('now') | Role creation timestamp |

**Relationships**:
- One-to-Many with UserRoles

**Indexes**:
- UNIQUE INDEX on Name

**Seed Data**:
- Admin (System Administrator)
- User (Regular User)
- StoreManager (Store Manager)
- WarehouseManager (Warehouse Manager)
- SalesStaff (Sales Staff)
- PurchaseStaff (Purchase Staff)

---

### 3. UserRoles Table (Junction Table)
**Purpose**: Many-to-many relationship between Users and Roles

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| UserId | INTEGER | FOREIGN KEY | Reference to Users.Id |
| RoleId | INTEGER | FOREIGN KEY | Reference to Roles.Id |
| AssignedAt | TEXT | DEFAULT datetime('now') | Role assignment timestamp |

**Relationships**:
- Many-to-One with Users
- Many-to-One with Roles

**Constraints**:
- Composite PRIMARY KEY (UserId, RoleId)
- CASCADE DELETE on both foreign keys

---

### 4. AuditLogs Table
**Purpose**: Tracks all system operations for audit trail

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| Id | INTEGER | PRIMARY KEY, AUTOINCREMENT | Unique audit log identifier |
| ActorUserId | INTEGER | FOREIGN KEY, NULL | User who performed the action |
| Entity | TEXT(100) | NOT NULL | Entity type (User, Product, etc.) |
| EntityId | TEXT(50) | NOT NULL | ID of the affected entity |
| Action | TEXT(50) | NOT NULL | Action performed (CREATE, UPDATE, DELETE) |
| Before | TEXT | NULL | JSON of entity state before action |
| After | TEXT | NULL | JSON of entity state after action |
| At | TEXT | DEFAULT datetime('now') | Action timestamp |
| IpAddress | TEXT(200) | NULL | IP address of the actor |
| UserAgent | TEXT(500) | NULL | User agent string |

**Relationships**:
- Many-to-One with Users (as ActorUser)

**Indexes**:
- INDEX on (Entity, EntityId)
- INDEX on ActorUserId
- INDEX on At

---

### 5. Categories Table
**Purpose**: Product categorization system

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| Id | INTEGER | PRIMARY KEY, AUTOINCREMENT | Unique category identifier |
| Name | TEXT(100) | NOT NULL, UNIQUE | Category name |
| Description | TEXT(500) | NULL | Category description |
| IsActive | INTEGER | DEFAULT 1 | Category status (1=active, 0=inactive) |
| CreatedAt | TEXT | DEFAULT datetime('now') | Category creation timestamp |
| UpdatedAt | TEXT | NULL | Last update timestamp |

**Relationships**:
- One-to-Many with Products

**Indexes**:
- UNIQUE INDEX on Name

**Seed Data**:
- Beverages
- Food Items
- Raw Materials
- Packaging
- Cleaning Supplies

---

### 6. Products Table
**Purpose**: Product catalog with detailed specifications

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| Id | INTEGER | PRIMARY KEY, AUTOINCREMENT | Unique product identifier |
| Name | TEXT(100) | NOT NULL | Product name |
| Description | TEXT(500) | NULL | Product description |
| Price | REAL | NOT NULL | Product price (decimal 18,2) |
| Unit | TEXT(50) | NULL | Unit of measurement (piece, box, kg, liter) |
| SKU | TEXT(50) | UNIQUE | Stock Keeping Unit |
| Brand | TEXT(100) | NULL | Product brand |
| Weight | REAL | NULL | Product weight in kg (decimal 18,2) |
| Dimensions | TEXT(50) | NULL | Product dimensions (e.g., "10x20x30 cm") |
| CategoryId | INTEGER | FOREIGN KEY | Reference to Categories.Id |
| IsActive | INTEGER | DEFAULT 1 | Product status (1=active, 0=inactive) |
| CreatedAt | TEXT | DEFAULT datetime('now') | Product creation timestamp |
| UpdatedAt | TEXT | NULL | Last update timestamp |

**Relationships**:
- Many-to-One with Categories
- One-to-Many with ProductInventories

**Indexes**:
- UNIQUE INDEX on SKU

**Constraints**:
- RESTRICT DELETE on CategoryId (prevents deletion of categories with products)

---

### 7. Warehouses Table
**Purpose**: Storage locations and warehouse management

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| Id | INTEGER | PRIMARY KEY, AUTOINCREMENT | Unique warehouse identifier |
| Name | TEXT(100) | NOT NULL | Warehouse name |
| Address | TEXT(200) | NULL | Warehouse address |
| City | TEXT(50) | NULL | Warehouse city |
| PhoneNumber | TEXT(20) | NULL | Warehouse phone number |
| ManagerName | TEXT(100) | NULL | Warehouse manager name (display only) |
| ManagerUserId | INTEGER | FOREIGN KEY, NULL | Reference to Users.Id (assigned manager) |
| IsActive | INTEGER | DEFAULT 1 | Warehouse status (1=active, 0=inactive) |
| CreatedAt | TEXT | DEFAULT datetime('now') | Warehouse creation timestamp |
| UpdatedAt | TEXT | NULL | Last update timestamp |

**Relationships**:
- Many-to-One with Users (as ManagerUser)
- One-to-Many with ProductInventories

**Seed Data**:
- Main Warehouse (Cairo)
- Store Branch 1 (Alexandria)
- Store Branch 2 (Giza)

---

### 8. ProductInventories Table
**Purpose**: Tracks product quantities in specific warehouses

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| Id | INTEGER | PRIMARY KEY, AUTOINCREMENT | Unique inventory record identifier |
| ProductId | INTEGER | FOREIGN KEY | Reference to Products.Id |
| WarehouseId | INTEGER | FOREIGN KEY | Reference to Warehouses.Id |
| Quantity | REAL | NOT NULL | Current stock quantity (decimal 18,2) |
| Unit | TEXT(50) | NULL | Unit of measurement for this inventory |
| MinimumStockLevel | REAL | NULL | Minimum stock alert level (decimal 18,2) |
| MaximumStockLevel | REAL | NULL | Maximum stock level (decimal 18,2) |
| CreatedAt | TEXT | DEFAULT datetime('now') | Inventory record creation timestamp |
| UpdatedAt | TEXT | NULL | Last update timestamp |

**Relationships**:
- Many-to-One with Products
- Many-to-One with Warehouses

**Indexes**:
- UNIQUE INDEX on (ProductId, WarehouseId)

**Constraints**:
- CASCADE DELETE on both foreign keys
- Ensures one inventory record per product per warehouse

---

## Entity Relationship Diagram (Visual)

```
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│     Users       │    │     Roles       │    │   UserRoles     │
├─────────────────┤    ├─────────────────┤    ├─────────────────┤
│ Id (PK)         │◄───┤ Id (PK)         │◄───┤ UserId (FK)     │
│ FullName        │    │ Name            │    │ RoleId (FK)     │
│ Email (UNIQUE)  │    │ Description     │    │ AssignedAt      │
│ PasswordHash    │    │ CreatedAt       │    └─────────────────┘
│ IsActive        │    └─────────────────┘
│ CreatedAt       │
│ UpdatedAt       │
└─────────────────┘
         │
         │ (1:N)
         ▼
┌─────────────────┐
│   AuditLogs     │
├─────────────────┤
│ Id (PK)         │
│ ActorUserId (FK)│
│ Entity          │
│ EntityId        │
│ Action          │
│ Before          │
│ After           │
│ At              │
│ IpAddress       │
│ UserAgent       │
└─────────────────┘

┌─────────────────┐    ┌─────────────────┐
│   Categories    │    │    Products      │
├─────────────────┤    ├─────────────────┤
│ Id (PK)         │◄───┤ Id (PK)         │
│ Name (UNIQUE)   │    │ Name            │
│ Description     │    │ Description     │
│ IsActive        │    │ Price           │
│ CreatedAt       │    │ Unit            │
│ UpdatedAt       │    │ SKU (UNIQUE)    │
└─────────────────┘    │ Brand           │
                       │ Weight          │
                       │ Dimensions      │
                       │ CategoryId (FK) │
                       │ IsActive        │
                       │ CreatedAt       │
                       │ UpdatedAt       │
                       └─────────────────┘
                                │
                                │ (1:N)
                                ▼
┌─────────────────┐    ┌─────────────────┐
│     Users       │    │   Warehouses    │
├─────────────────┤    ├─────────────────┤
│ Id (PK)         │◄───┤ Id (PK)         │
│ FullName        │    │ Name            │
│ Email (UNIQUE)  │    │ Address         │
│ PasswordHash    │    │ City            │
│ IsActive        │    │ PhoneNumber     │
│ CreatedAt       │    │ ManagerName     │
│ UpdatedAt       │    │ ManagerUserId(FK)│
└─────────────────┘    │ IsActive        │
         │              │ CreatedAt       │
         │ (1:N)        │ UpdatedAt       │
         ▼              └─────────────────┘
┌─────────────────┐              │
│   AuditLogs     │              │ (1:N)
├─────────────────┤              ▼
│ Id (PK)         │    ┌─────────────────┐
│ ActorUserId (FK)│    │ProductInventories│
│ Entity          │    ├─────────────────┤
│ EntityId        │    │ Id (PK)         │
│ Action          │    │ ProductId (FK)  │
│ Before          │    │ WarehouseId (FK) │
│ After           │    │ Quantity        │
│ At              │    │ Unit            │
│ IpAddress       │    │ MinStockLevel   │
│ UserAgent       │    │ MaxStockLevel   │
└─────────────────┘    │ CreatedAt       │
                       │ UpdatedAt       │
                       └─────────────────┘
```

---

## Business Rules and Constraints

### 1. User Management
- Each user must have a unique email address
- Users can have multiple roles (many-to-many relationship)
- User accounts can be deactivated (soft delete)
- Password must be hashed using BCrypt

### 2. Role-Based Access Control (RBAC)
- Six predefined roles: Admin, User, StoreManager, WarehouseManager, SalesStaff, PurchaseStaff
- Users can be assigned multiple roles
- Role assignments are tracked with timestamps

### 3. Product Management
- Products must belong to a category
- Each product must have a unique SKU
- Products can be deactivated (soft delete)
- Products support detailed specifications (weight, dimensions, brand)

### 4. Inventory Management
- Each product can have inventory in multiple warehouses
- Each warehouse can store multiple products
- Unique constraint ensures one inventory record per product per warehouse
- Stock levels can have minimum and maximum thresholds
- Inventory quantities are tracked with decimal precision

### 5. Audit Trail
- All system operations are logged
- Audit logs capture before/after states as JSON
- Actor information, timestamps, and request details are recorded
- Supports compliance and debugging requirements

---

## Database Configuration

### Connection String
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=authrbac.db"
  }
}
```

### Entity Framework Configuration
- Uses SQLite provider for cross-platform compatibility
- Automatic database creation with `EnsureCreated()`
- Seed data populated on first run
- Proper foreign key constraints and indexes

### Performance Considerations
- Indexes on frequently queried columns
- Unique constraints prevent data integrity issues
- Proper foreign key relationships with appropriate delete behaviors
- Decimal precision for financial and measurement data

---

## API Endpoints Summary

### Authentication & User Management
- `POST /api/auth/register` - Public user registration
- `POST /api/auth/login` - User authentication
- `GET /api/auth/me` - Get current user info
- `PATCH /api/userprofile/profile` - Update user profile
- `DELETE /api/userprofile/account` - Delete user account

### Admin Dashboard
- `GET /api/admin/users` - List all users
- `POST /api/admin/users` - Create user
- `PATCH /api/admin/users/{id}` - Update user
- `DELETE /api/admin/users/{id}` - Delete user
- `GET /api/admin/roles` - List all roles
- `GET /api/admin/statistics` - System statistics

### Category Management
- `GET /api/categories` - List active categories
- `GET /api/categories/{id}` - Get category details
- `POST /api/categories` - Create category (Admin)
- `PUT /api/categories/{id}` - Update category (Admin)
- `DELETE /api/categories/{id}` - Soft delete category (Admin)

### Product Management
- `GET /api/products` - List active products
- `GET /api/products/{id}` - Get product details
- `POST /api/products` - Create product
- `PUT /api/products/{id}` - Update product
- `DELETE /api/products/{id}` - Delete product
- `PUT /api/products/{id}/inventory` - Update product inventory

### Warehouse Management
- `GET /api/warehouses` - List active warehouses
- `GET /api/warehouses/{id}` - Get warehouse details
- `POST /api/warehouses` - Create warehouse
- `PUT /api/warehouses/{id}` - Update warehouse
- `DELETE /api/warehouses/{id}` - Delete warehouse
- `GET /api/warehouses/{id}/inventory` - Get warehouse inventory
- `POST /api/warehouses/{id}/assign-manager` - Assign manager to warehouse
- `DELETE /api/warehouses/{id}/remove-manager` - Remove manager from warehouse

---

## Security Features

### Authentication
- JWT-based authentication
- Refresh token mechanism
- Password hashing with BCrypt
- Token expiration handling

### Authorization
- Role-based access control (RBAC)
- Endpoint-level authorization attributes
- Admin-only operations properly protected
- User self-management capabilities

### Audit Trail
- Comprehensive logging of all operations
- IP address and user agent tracking
- Before/after state capture
- Compliance-ready audit logs

---

## Conclusion

This database design provides a robust foundation for a Food & Beverage Management System with:

- **Scalable Architecture**: Proper normalization and relationships
- **Security**: RBAC, audit trails, and secure authentication
- **Flexibility**: Support for multiple warehouses, categories, and inventory tracking
- **Maintainability**: Clear relationships, constraints, and documentation
- **Performance**: Appropriate indexes and constraints

The system is production-ready and supports all the core requirements for food and beverage business management.
