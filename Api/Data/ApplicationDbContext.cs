using Microsoft.EntityFrameworkCore;
using Api.Models;

namespace Api.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<UserRole> UserRoles { get; set; }
    public DbSet<AuditLog> AuditLogs { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<Warehouse> Warehouses { get; set; }
    public DbSet<ProductInventory> ProductInventories { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure User entity
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.Email).IsRequired();
            entity.Property(e => e.FullName).IsRequired();
            entity.Property(e => e.PasswordHash).IsRequired();
        });

        // Configure Role entity
        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Name).IsUnique();
            entity.Property(e => e.Name).IsRequired();
        });

        // Configure UserRole entity (many-to-many)
        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.RoleId });
            
            entity.HasOne(e => e.User)
                .WithMany(u => u.UserRoles)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(e => e.Role)
                .WithMany(r => r.UserRoles)
                .HasForeignKey(e => e.RoleId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure AuditLog entity
        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.Entity, e.EntityId });
            entity.HasIndex(e => e.ActorUserId);
            entity.HasIndex(e => e.At);
            
            entity.HasOne(e => e.ActorUser)
                .WithMany()
                .HasForeignKey(e => e.ActorUserId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Configure Category entity
        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Name).IsUnique();
            entity.Property(e => e.Name).IsRequired();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("datetime('now')");
        });

        // Configure Product entity
        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired();
            entity.Property(e => e.Price).HasColumnType("decimal(18,2)");
            entity.Property(e => e.Weight).HasColumnType("decimal(18,2)");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("datetime('now')");
            entity.HasIndex(e => e.SKU).IsUnique();
            
            entity.HasOne(e => e.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(e => e.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure Warehouse entity
        modelBuilder.Entity<Warehouse>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("datetime('now')");
            
            // Configure relationship with User (Manager)
            entity.HasOne(e => e.ManagerUser)
                .WithMany()
                .HasForeignKey(e => e.ManagerUserId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Configure ProductInventory entity
        modelBuilder.Entity<ProductInventory>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Quantity).HasColumnType("decimal(18,2)");
            entity.Property(e => e.MinimumStockLevel).HasColumnType("decimal(18,2)");
            entity.Property(e => e.MaximumStockLevel).HasColumnType("decimal(18,2)");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("datetime('now')");
            
            entity.HasOne(e => e.Product)
                .WithMany(p => p.ProductInventories)
                .HasForeignKey(e => e.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(e => e.Warehouse)
                .WithMany(w => w.ProductInventories)
                .HasForeignKey(e => e.WarehouseId)
                .OnDelete(DeleteBehavior.Cascade);
                
            // Ensure unique combination of Product and Warehouse
            entity.HasIndex(e => new { e.ProductId, e.WarehouseId }).IsUnique();
        });

        // Seed initial data
        SeedData(modelBuilder);
    }

    private static void SeedData(ModelBuilder modelBuilder)
    {
        // Seed roles for Food & Beverage Management System
        modelBuilder.Entity<Role>().HasData(
            new Role { Id = 1, Name = "Admin", Description = "System Administrator - Full access to everything" },
            new Role { Id = 2, Name = "User", Description = "Regular User - Can manage own account and make requests" },
            new Role { Id = 3, Name = "StoreManager", Description = "Store Manager - Manages assigned store, products, and inventory" },
            new Role { Id = 4, Name = "WarehouseManager", Description = "Warehouse Manager - Manages warehouse operations and stock" },
            new Role { Id = 5, Name = "SalesStaff", Description = "Sales Staff - Handles sales and customer orders" },
            new Role { Id = 6, Name = "PurchaseStaff", Description = "Purchase Staff - Handles purchase orders and supplier management" }
        );

        // Seed admin user (password: Admin123!)
        modelBuilder.Entity<User>().HasData(
            new User
            {
                Id = 1,
                FullName = "System Administrator",
                Email = "admin@example.com",
                PasswordHash = "$2a$11$92IXUNpkjO0rOQ5byMi.Ye4oKoEa3Ro9llC/.og/at2.uheWG/igi", // Password: password
                IsActive = true,
                CreatedAt = new DateTime(2023, 1, 1, 10, 0, 0, DateTimeKind.Utc)
            }
        );

        // Assign admin role to admin user
        modelBuilder.Entity<UserRole>().HasData(
            new UserRole { UserId = 1, RoleId = 1, AssignedAt = new DateTime(2023, 1, 1, 10, 0, 0, DateTimeKind.Utc) }
        );

        // Seed initial categories for Food & Beverage Management System
        modelBuilder.Entity<Category>().HasData(
            new Category { Id = 1, Name = "Beverages", Description = "All types of drinks including soft drinks, juices", IsActive = true, CreatedAt = new DateTime(2023, 1, 1, 10, 0, 0, DateTimeKind.Utc) },
            new Category { Id = 2, Name = "Food Items", Description = "Ready-to-eat food items and snacks", IsActive = true, CreatedAt = new DateTime(2023, 1, 1, 10, 0, 0, DateTimeKind.Utc) },
            new Category { Id = 3, Name = "Raw Materials", Description = "Ingredients and raw materials for food preparation", IsActive = true, CreatedAt = new DateTime(2023, 1, 1, 10, 0, 0, DateTimeKind.Utc) },
            new Category { Id = 4, Name = "Packaging", Description = "Packaging materials and containers", IsActive = true, CreatedAt = new DateTime(2023, 1, 1, 10, 0, 0, DateTimeKind.Utc) },
            new Category { Id = 5, Name = "Cleaning Supplies", Description = "Cleaning and sanitization supplies", IsActive = true, CreatedAt = new DateTime(2023, 1, 1, 10, 0, 0, DateTimeKind.Utc) }
        );

        // Seed initial warehouses
        modelBuilder.Entity<Warehouse>().HasData(
            new Warehouse { Id = 1, Name = "Main Warehouse", Address = "123 Industrial St", City = "Cairo", PhoneNumber = "+20-123-456-7890", ManagerName = "Ahmed Hassan", IsActive = true, CreatedAt = new DateTime(2023, 1, 1, 10, 0, 0, DateTimeKind.Utc) },
            new Warehouse { Id = 2, Name = "Store Branch 1", Address = "456 Commercial Ave", City = "Alexandria", PhoneNumber = "+20-123-456-7891", ManagerName = "Fatma Ali", IsActive = true, CreatedAt = new DateTime(2023, 1, 1, 10, 0, 0, DateTimeKind.Utc) },
            new Warehouse { Id = 3, Name = "Store Branch 2", Address = "789 Business Blvd", City = "Giza", PhoneNumber = "+20-123-456-7892", ManagerName = "Mohamed Salah", IsActive = true, CreatedAt = new DateTime(2023, 1, 1, 10, 0, 0, DateTimeKind.Utc) }
        );
    }
}
