using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Api.Data;
using Api.DTOs;
using Api.Models;
using Api.Services;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // All endpoints require authentication
public class ProductController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IAuditService _auditService;

    public ProductController(ApplicationDbContext context, IAuditService auditService)
    {
        _context = context;
        _auditService = auditService;
    }

    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");
        return userIdClaim != null ? int.Parse(userIdClaim.Value) : 0;
    }

    /// <summary>
    /// Get all active products
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProductListResponse>>> GetProducts()
    {
        var products = await _context.Products
            .Where(p => p.IsActive)
            .Include(p => p.Category)
            .Include(p => p.ProductInventories)
                .ThenInclude(pi => pi.Warehouse)
            .Select(p => new ProductListResponse
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                Unit = p.Unit,
                SKU = p.SKU,
                Brand = p.Brand,
                CategoryName = p.Category.Name,
                IsActive = p.IsActive,
                TotalQuantity = p.ProductInventories.Sum(pi => pi.Quantity)
            })
            .OrderBy(p => p.Name)
            .ToListAsync();

        return Ok(products);
    }

    /// <summary>
    /// Get all products including inactive ones (Admin only)
    /// </summary>
    [HttpGet("all")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<IEnumerable<ProductResponse>>> GetAllProducts()
    {
        var products = await _context.Products
            .Include(p => p.Category)
            .Include(p => p.ProductInventories)
                .ThenInclude(pi => pi.Warehouse)
            .Select(p => new ProductResponse
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                Unit = p.Unit,
                SKU = p.SKU,
                Brand = p.Brand,
                Weight = p.Weight,
                Dimensions = p.Dimensions,
                CategoryId = p.CategoryId,
                CategoryName = p.Category.Name,
                IsActive = p.IsActive,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt,
                TotalQuantity = p.ProductInventories.Sum(pi => pi.Quantity),
                Inventories = p.ProductInventories.Select(pi => new ProductInventoryResponse
                {
                    Id = pi.Id,
                    WarehouseId = pi.WarehouseId,
                    WarehouseName = pi.Warehouse.Name,
                    Quantity = pi.Quantity,
                    Unit = pi.Unit,
                    MinimumStockLevel = pi.MinimumStockLevel,
                    MaximumStockLevel = pi.MaximumStockLevel
                }).ToList()
            })
            .OrderBy(p => p.Name)
            .ToListAsync();

        return Ok(products);
    }

    /// <summary>
    /// Get a specific product by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ProductResponse>> GetProduct(int id)
    {
        var product = await _context.Products
            .Where(p => p.Id == id)
            .Include(p => p.Category)
            .Include(p => p.ProductInventories)
                .ThenInclude(pi => pi.Warehouse)
            .Select(p => new ProductResponse
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                Unit = p.Unit,
                SKU = p.SKU,
                Brand = p.Brand,
                Weight = p.Weight,
                Dimensions = p.Dimensions,
                CategoryId = p.CategoryId,
                CategoryName = p.Category.Name,
                IsActive = p.IsActive,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt,
                TotalQuantity = p.ProductInventories.Sum(pi => pi.Quantity),
                Inventories = p.ProductInventories.Select(pi => new ProductInventoryResponse
                {
                    Id = pi.Id,
                    WarehouseId = pi.WarehouseId,
                    WarehouseName = pi.Warehouse.Name,
                    Quantity = pi.Quantity,
                    Unit = pi.Unit,
                    MinimumStockLevel = pi.MinimumStockLevel,
                    MaximumStockLevel = pi.MaximumStockLevel
                }).ToList()
            })
            .FirstOrDefaultAsync();

        if (product == null)
        {
            return NotFound($"Product with ID {id} not found.");
        }

        return Ok(product);
    }

    /// <summary>
    /// Create a new product (Admin only)
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ProductResponse>> CreateProduct([FromBody] CreateProductRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest("Product name is required.");
        }

        // Check if SKU already exists
        if (!string.IsNullOrWhiteSpace(request.SKU))
        {
            var existingProduct = await _context.Products
                .FirstOrDefaultAsync(p => p.SKU == request.SKU);

            if (existingProduct != null)
            {
                return BadRequest($"A product with SKU '{request.SKU}' already exists.");
            }
        }

        // Verify category exists
        var category = await _context.Categories.FindAsync(request.CategoryId);
        if (category == null)
        {
            return BadRequest($"Category with ID {request.CategoryId} not found.");
        }

        var product = new Product
        {
            Name = request.Name.Trim(),
            Description = request.Description?.Trim(),
            Price = request.Price,
            Unit = request.Unit?.Trim(),
            SKU = request.SKU?.Trim(),
            Brand = request.Brand?.Trim(),
            Weight = request.Weight,
            Dimensions = request.Dimensions?.Trim(),
            CategoryId = request.CategoryId,
            IsActive = request.IsActive,
            CreatedAt = DateTime.UtcNow
        };

        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        // Audit log
        await _auditService.LogAsync(
            entity: "Product",
            entityId: product.Id.ToString(),
            action: "CREATE",
            actorUserId: GetCurrentUserId(),
            before: null,
            after: $"Name: {product.Name}, Price: {product.Price}, Category: {category.Name}, SKU: {product.SKU}"
        );

        var response = new ProductResponse
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            Unit = product.Unit,
            SKU = product.SKU,
            Brand = product.Brand,
            Weight = product.Weight,
            Dimensions = product.Dimensions,
            CategoryId = product.CategoryId,
            CategoryName = category.Name,
            IsActive = product.IsActive,
            CreatedAt = product.CreatedAt,
            UpdatedAt = product.UpdatedAt,
            TotalQuantity = 0,
            Inventories = new List<ProductInventoryResponse>()
        };

        return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, response);
    }

    /// <summary>
    /// Update a product (Admin only)
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ProductResponse>> UpdateProduct(int id, [FromBody] UpdateProductRequest request)
    {
        var product = await _context.Products
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (product == null)
        {
            return NotFound($"Product with ID {id} not found.");
        }

        var before = $"Name: {product.Name}, Price: {product.Price}, Category: {product.Category.Name}, SKU: {product.SKU}";

        // Update fields if provided
        if (!string.IsNullOrWhiteSpace(request.Name))
        {
            product.Name = request.Name.Trim();
        }

        if (request.Description != null)
        {
            product.Description = request.Description.Trim();
        }

        if (request.Price.HasValue)
        {
            product.Price = request.Price.Value;
        }

        if (request.Unit != null)
        {
            product.Unit = request.Unit.Trim();
        }

        if (request.SKU != null)
        {
            // Check if new SKU already exists (excluding current product)
            if (!string.IsNullOrWhiteSpace(request.SKU))
            {
                var existingProduct = await _context.Products
                    .FirstOrDefaultAsync(p => p.SKU == request.SKU && p.Id != id);

                if (existingProduct != null)
                {
                    return BadRequest($"A product with SKU '{request.SKU}' already exists.");
                }
            }
            product.SKU = request.SKU.Trim();
        }

        if (request.Brand != null)
        {
            product.Brand = request.Brand.Trim();
        }

        if (request.Weight.HasValue)
        {
            product.Weight = request.Weight.Value;
        }

        if (request.Dimensions != null)
        {
            product.Dimensions = request.Dimensions.Trim();
        }

        if (request.CategoryId.HasValue)
        {
            var category = await _context.Categories.FindAsync(request.CategoryId.Value);
            if (category == null)
            {
                return BadRequest($"Category with ID {request.CategoryId.Value} not found.");
            }
            product.CategoryId = request.CategoryId.Value;
        }

        if (request.IsActive.HasValue)
        {
            product.IsActive = request.IsActive.Value;
        }

        product.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        // Audit log
        var after = $"Name: {product.Name}, Price: {product.Price}, Category: {product.Category.Name}, SKU: {product.SKU}";
        await _auditService.LogAsync(
            entity: "Product",
            entityId: product.Id.ToString(),
            action: "UPDATE",
            actorUserId: GetCurrentUserId(),
            before: before,
            after: after
        );

        var response = new ProductResponse
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            Unit = product.Unit,
            SKU = product.SKU,
            Brand = product.Brand,
            Weight = product.Weight,
            Dimensions = product.Dimensions,
            CategoryId = product.CategoryId,
            CategoryName = product.Category.Name,
            IsActive = product.IsActive,
            CreatedAt = product.CreatedAt,
            UpdatedAt = product.UpdatedAt,
            TotalQuantity = await _context.ProductInventories.Where(pi => pi.ProductId == product.Id).SumAsync(pi => pi.Quantity),
            Inventories = await _context.ProductInventories
                .Where(pi => pi.ProductId == product.Id)
                .Include(pi => pi.Warehouse)
                .Select(pi => new ProductInventoryResponse
                {
                    Id = pi.Id,
                    WarehouseId = pi.WarehouseId,
                    WarehouseName = pi.Warehouse.Name,
                    Quantity = pi.Quantity,
                    Unit = pi.Unit,
                    MinimumStockLevel = pi.MinimumStockLevel,
                    MaximumStockLevel = pi.MaximumStockLevel
                }).ToListAsync()
        };

        return Ok(response);
    }

    /// <summary>
    /// Soft delete a product (Admin only)
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null)
        {
            return NotFound($"Product with ID {id} not found.");
        }

        var before = $"Name: {product.Name}, Price: {product.Price}, SKU: {product.SKU}";

        // Soft delete by setting IsActive to false
        product.IsActive = false;
        product.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        // Audit log
        var after = $"Name: {product.Name}, Price: {product.Price}, SKU: {product.SKU}, IsActive: {product.IsActive}";
        await _auditService.LogAsync(
            entity: "Product",
            entityId: product.Id.ToString(),
            action: "DELETE",
            actorUserId: GetCurrentUserId(),
            before: before,
            after: after
        );

        return NoContent();
    }

    /// <summary>
    /// Update product inventory in a specific warehouse (Admin only)
    /// </summary>
    [HttpPut("{id}/inventory/{warehouseId}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ProductInventoryResponse>> UpdateInventory(int id, int warehouseId, [FromBody] UpdateInventoryRequest request)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null)
        {
            return NotFound($"Product with ID {id} not found.");
        }

        var warehouse = await _context.Warehouses.FindAsync(warehouseId);
        if (warehouse == null)
        {
            return NotFound($"Warehouse with ID {warehouseId} not found.");
        }

        var inventory = await _context.ProductInventories
            .FirstOrDefaultAsync(pi => pi.ProductId == id && pi.WarehouseId == warehouseId);

        var before = inventory != null ? $"Quantity: {inventory.Quantity}, Unit: {inventory.Unit}" : "No inventory record";

        if (inventory == null)
        {
            // Create new inventory record
            inventory = new ProductInventory
            {
                ProductId = id,
                WarehouseId = warehouseId,
                Quantity = request.Quantity,
                Unit = request.Unit?.Trim(),
                MinimumStockLevel = request.MinimumStockLevel,
                MaximumStockLevel = request.MaximumStockLevel,
                CreatedAt = DateTime.UtcNow
            };
            _context.ProductInventories.Add(inventory);
        }
        else
        {
            // Update existing inventory record
            inventory.Quantity = request.Quantity;
            inventory.Unit = request.Unit?.Trim();
            inventory.MinimumStockLevel = request.MinimumStockLevel;
            inventory.MaximumStockLevel = request.MaximumStockLevel;
            inventory.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();

        // Audit log
        var after = $"Quantity: {inventory.Quantity}, Unit: {inventory.Unit}, MinLevel: {inventory.MinimumStockLevel}, MaxLevel: {inventory.MaximumStockLevel}";
        await _auditService.LogAsync(
            entity: "ProductInventory",
            entityId: inventory.Id.ToString(),
            action: inventory.CreatedAt == inventory.UpdatedAt ? "CREATE" : "UPDATE",
            actorUserId: GetCurrentUserId(),
            before: before,
            after: after
        );

        var response = new ProductInventoryResponse
        {
            Id = inventory.Id,
            WarehouseId = inventory.WarehouseId,
            WarehouseName = warehouse.Name,
            Quantity = inventory.Quantity,
            Unit = inventory.Unit,
            MinimumStockLevel = inventory.MinimumStockLevel,
            MaximumStockLevel = inventory.MaximumStockLevel
        };

        return Ok(response);
    }
}
