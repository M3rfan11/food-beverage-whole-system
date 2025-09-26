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
public class CategoryController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IAuditService _auditService;

    public CategoryController(ApplicationDbContext context, IAuditService auditService)
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
    /// Get all categories
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CategoryListResponse>>> GetCategories()
    {
        var categories = await _context.Categories
            .Where(c => c.IsActive)
            .Select(c => new CategoryListResponse
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                IsActive = c.IsActive,
                ProductCount = c.Products.Count(p => p.IsActive)
            })
            .OrderBy(c => c.Name)
            .ToListAsync();

        return Ok(categories);
    }

    /// <summary>
    /// Get all categories including inactive ones (Admin only)
    /// </summary>
    [HttpGet("all")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<IEnumerable<CategoryResponse>>> GetAllCategories()
    {
        var categories = await _context.Categories
            .Select(c => new CategoryResponse
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                IsActive = c.IsActive,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt,
                ProductCount = c.Products.Count(p => p.IsActive)
            })
            .OrderBy(c => c.Name)
            .ToListAsync();

        return Ok(categories);
    }

    /// <summary>
    /// Get a specific category by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<CategoryResponse>> GetCategory(int id)
    {
        var category = await _context.Categories
            .Where(c => c.Id == id)
            .Select(c => new CategoryResponse
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                IsActive = c.IsActive,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt,
                ProductCount = c.Products.Count(p => p.IsActive)
            })
            .FirstOrDefaultAsync();

        if (category == null)
        {
            return NotFound($"Category with ID {id} not found.");
        }

        return Ok(category);
    }

    /// <summary>
    /// Create a new category (Admin only)
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<CategoryResponse>> CreateCategory([FromBody] CreateCategoryRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest("Category name is required.");
        }

        // Check if category name already exists
        var existingCategory = await _context.Categories
            .FirstOrDefaultAsync(c => c.Name.ToLower() == request.Name.ToLower());

        if (existingCategory != null)
        {
            return BadRequest($"A category with the name '{request.Name}' already exists.");
        }

        var category = new Category
        {
            Name = request.Name.Trim(),
            Description = request.Description?.Trim(),
            IsActive = request.IsActive,
            CreatedAt = DateTime.UtcNow
        };

        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        // Audit log
        await _auditService.LogAsync(
            entity: "Category",
            entityId: category.Id.ToString(),
            action: "CREATE",
            actorUserId: GetCurrentUserId(),
            before: null,
            after: $"Name: {category.Name}, Description: {category.Description}, IsActive: {category.IsActive}"
        );

        var response = new CategoryResponse
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description,
            IsActive = category.IsActive,
            CreatedAt = category.CreatedAt,
            UpdatedAt = category.UpdatedAt,
            ProductCount = 0
        };

        return CreatedAtAction(nameof(GetCategory), new { id = category.Id }, response);
    }

    /// <summary>
    /// Update a category (Admin only)
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<CategoryResponse>> UpdateCategory(int id, [FromBody] UpdateCategoryRequest request)
    {
        var category = await _context.Categories.FindAsync(id);
        if (category == null)
        {
            return NotFound($"Category with ID {id} not found.");
        }

        var before = $"Name: {category.Name}, Description: {category.Description}, IsActive: {category.IsActive}";

        // Update fields if provided
        if (!string.IsNullOrWhiteSpace(request.Name))
        {
            // Check if new name already exists (excluding current category)
            var existingCategory = await _context.Categories
                .FirstOrDefaultAsync(c => c.Name.ToLower() == request.Name.ToLower() && c.Id != id);

            if (existingCategory != null)
            {
                return BadRequest($"A category with the name '{request.Name}' already exists.");
            }

            category.Name = request.Name.Trim();
        }

        if (request.Description != null)
        {
            category.Description = request.Description.Trim();
        }

        if (request.IsActive.HasValue)
        {
            category.IsActive = request.IsActive.Value;
        }

        category.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        // Audit log
        var after = $"Name: {category.Name}, Description: {category.Description}, IsActive: {category.IsActive}";
        await _auditService.LogAsync(
            entity: "Category",
            entityId: category.Id.ToString(),
            action: "UPDATE",
            actorUserId: GetCurrentUserId(),
            before: before,
            after: after
        );

        var response = new CategoryResponse
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description,
            IsActive = category.IsActive,
            CreatedAt = category.CreatedAt,
            UpdatedAt = category.UpdatedAt,
            ProductCount = await _context.Products.CountAsync(p => p.CategoryId == category.Id && p.IsActive)
        };

        return Ok(response);
    }

    /// <summary>
    /// Soft delete a category (Admin only)
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        var category = await _context.Categories.FindAsync(id);
        if (category == null)
        {
            return NotFound($"Category with ID {id} not found.");
        }

        // Check if category has active products
        var hasActiveProducts = await _context.Products.AnyAsync(p => p.CategoryId == id && p.IsActive);
        if (hasActiveProducts)
        {
            return BadRequest("Cannot delete category that has active products. Please deactivate or reassign the products first.");
        }

        var before = $"Name: {category.Name}, Description: {category.Description}, IsActive: {category.IsActive}";

        // Soft delete by setting IsActive to false
        category.IsActive = false;
        category.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        // Audit log
        var after = $"Name: {category.Name}, Description: {category.Description}, IsActive: {category.IsActive}";
        await _auditService.LogAsync(
            entity: "Category",
            entityId: category.Id.ToString(),
            action: "DELETE",
            actorUserId: GetCurrentUserId(),
            before: before,
            after: after
        );

        return NoContent();
    }

    /// <summary>
    /// Restore a soft-deleted category (Admin only)
    /// </summary>
    [HttpPatch("{id}/restore")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<CategoryResponse>> RestoreCategory(int id)
    {
        var category = await _context.Categories.FindAsync(id);
        if (category == null)
        {
            return NotFound($"Category with ID {id} not found.");
        }

        if (category.IsActive)
        {
            return BadRequest("Category is already active.");
        }

        var before = $"Name: {category.Name}, Description: {category.Description}, IsActive: {category.IsActive}";

        category.IsActive = true;
        category.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        // Audit log
        var after = $"Name: {category.Name}, Description: {category.Description}, IsActive: {category.IsActive}";
        await _auditService.LogAsync(
            entity: "Category",
            entityId: category.Id.ToString(),
            action: "RESTORE",
            actorUserId: GetCurrentUserId(),
            before: before,
            after: after
        );

        var response = new CategoryResponse
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description,
            IsActive = category.IsActive,
            CreatedAt = category.CreatedAt,
            UpdatedAt = category.UpdatedAt,
            ProductCount = await _context.Products.CountAsync(p => p.CategoryId == category.Id && p.IsActive)
        };

        return Ok(response);
    }
}
