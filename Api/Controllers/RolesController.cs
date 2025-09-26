using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Api.Data;
using Api.DTOs;
using Api.Services;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class RolesController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IAuditService _auditService;
    private readonly ILogger<RolesController> _logger;

    public RolesController(ApplicationDbContext context, IAuditService auditService, ILogger<RolesController> logger)
    {
        _context = context;
        _auditService = auditService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<RoleResponse>>> GetRoles()
    {
        try
        {
            var roles = await _context.Roles
                .Select(r => new RoleResponse
                {
                    Id = r.Id,
                    Name = r.Name,
                    Description = r.Description,
                    CreatedAt = r.CreatedAt
                })
                .ToListAsync();

            return Ok(roles);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving roles");
            return StatusCode(500, new { message = "An error occurred while retrieving roles" });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<RoleResponse>> GetRole(int id)
    {
        try
        {
            var role = await _context.Roles
                .Where(r => r.Id == id)
                .Select(r => new RoleResponse
                {
                    Id = r.Id,
                    Name = r.Name,
                    Description = r.Description,
                    CreatedAt = r.CreatedAt
                })
                .FirstOrDefaultAsync();

            if (role == null)
            {
                return NotFound(new { message = "Role not found" });
            }

            return Ok(role);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving role with ID: {RoleId}", id);
            return StatusCode(500, new { message = "An error occurred while retrieving the role" });
        }
    }

    [HttpPost]
    public async Task<ActionResult<RoleResponse>> CreateRole([FromBody] CreateRoleRequest request)
    {
        try
        {
            // Check if role name already exists
            if (await _context.Roles.AnyAsync(r => r.Name == request.Name))
            {
                return Conflict(new { message = "Role name already exists" });
            }

            var role = new Api.Models.Role
            {
                Name = request.Name,
                Description = request.Description,
                CreatedAt = DateTime.UtcNow
            };

            _context.Roles.Add(role);
            await _context.SaveChangesAsync();

            var response = new RoleResponse
            {
                Id = role.Id,
                Name = role.Name,
                Description = role.Description,
                CreatedAt = role.CreatedAt
            };

            // Audit log
            var currentUserId = GetCurrentUserId();
            await _auditService.LogAsync(
                "Role",
                role.Id.ToString(),
                "Create",
                after: System.Text.Json.JsonSerializer.Serialize(response),
                actorUserId: currentUserId
            );

            return CreatedAtAction(nameof(GetRole), new { id = role.Id }, response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating role with name: {RoleName}", request.Name);
            return StatusCode(500, new { message = "An error occurred while creating the role" });
        }
    }

    [HttpPatch("{id}")]
    public async Task<ActionResult<RoleResponse>> UpdateRole(int id, [FromBody] UpdateRoleRequest request)
    {
        try
        {
            var role = await _context.Roles.FindAsync(id);
            if (role == null)
            {
                return NotFound(new { message = "Role not found" });
            }

            var before = System.Text.Json.JsonSerializer.Serialize(new RoleResponse
            {
                Id = role.Id,
                Name = role.Name,
                Description = role.Description,
                CreatedAt = role.CreatedAt
            });

            // Update fields if provided
            if (!string.IsNullOrEmpty(request.Name))
            {
                if (await _context.Roles.AnyAsync(r => r.Name == request.Name && r.Id != id))
                {
                    return Conflict(new { message = "Role name already exists" });
                }
                role.Name = request.Name;
            }
            
            if (request.Description != null)
                role.Description = request.Description;

            await _context.SaveChangesAsync();

            var after = System.Text.Json.JsonSerializer.Serialize(new RoleResponse
            {
                Id = role.Id,
                Name = role.Name,
                Description = role.Description,
                CreatedAt = role.CreatedAt
            });

            // Audit log
            var currentUserId = GetCurrentUserId();
            await _auditService.LogAsync(
                "Role",
                role.Id.ToString(),
                "Update",
                before: before,
                after: after,
                actorUserId: currentUserId
            );

            return Ok(new RoleResponse
            {
                Id = role.Id,
                Name = role.Name,
                Description = role.Description,
                CreatedAt = role.CreatedAt
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating role with ID: {RoleId}", id);
            return StatusCode(500, new { message = "An error occurred while updating the role" });
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteRole(int id)
    {
        try
        {
            var role = await _context.Roles.FindAsync(id);
            if (role == null)
            {
                return NotFound(new { message = "Role not found" });
            }

            // Check if role is assigned to any users
            var hasUsers = await _context.UserRoles.AnyAsync(ur => ur.RoleId == id);
            if (hasUsers)
            {
                return Conflict(new { message = "Cannot delete role that is assigned to users" });
            }

            var before = System.Text.Json.JsonSerializer.Serialize(new RoleResponse
            {
                Id = role.Id,
                Name = role.Name,
                Description = role.Description,
                CreatedAt = role.CreatedAt
            });

            _context.Roles.Remove(role);
            await _context.SaveChangesAsync();

            // Audit log
            var currentUserId = GetCurrentUserId();
            await _auditService.LogAsync(
                "Role",
                id.ToString(),
                "Delete",
                before: before,
                actorUserId: currentUserId
            );

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting role with ID: {RoleId}", id);
            return StatusCode(500, new { message = "An error occurred while deleting the role" });
        }
    }

    private int? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        return userIdClaim != null && int.TryParse(userIdClaim.Value, out var userId) ? userId : null;
    }
}
