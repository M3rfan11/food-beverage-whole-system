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
public class AdminController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IAuditService _auditService;
    private readonly ILogger<AdminController> _logger;

    public AdminController(ApplicationDbContext context, IAuditService auditService, ILogger<AdminController> logger)
    {
        _context = context;
        _auditService = auditService;
        _logger = logger;
    }

    #region User Management

    [HttpGet("users")]
    public async Task<ActionResult<IEnumerable<UserResponse>>> GetAllUsers()
    {
        try
        {
            var users = await _context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .Select(u => new UserResponse
                {
                    Id = u.Id,
                    FullName = u.FullName,
                    Email = u.Email,
                    IsActive = u.IsActive,
                    CreatedAt = u.CreatedAt,
                    UpdatedAt = u.UpdatedAt,
                    Roles = u.UserRoles.Select(ur => ur.Role.Name).ToList()
                })
                .ToListAsync();

            return Ok(users);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all users");
            return StatusCode(500, new { message = "An error occurred while retrieving users" });
        }
    }

    [HttpGet("users/{id}")]
    public async Task<ActionResult<UserResponse>> GetUser(int id)
    {
        try
        {
            var user = await _context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .Where(u => u.Id == id)
                .Select(u => new UserResponse
                {
                    Id = u.Id,
                    FullName = u.FullName,
                    Email = u.Email,
                    IsActive = u.IsActive,
                    CreatedAt = u.CreatedAt,
                    UpdatedAt = u.UpdatedAt,
                    Roles = u.UserRoles.Select(ur => ur.Role.Name).ToList()
                })
                .FirstOrDefaultAsync();

            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }

            return Ok(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user with ID: {UserId}", id);
            return StatusCode(500, new { message = "An error occurred while retrieving the user" });
        }
    }

    [HttpPost("users")]
    public async Task<ActionResult<UserResponse>> CreateUser([FromBody] CreateUserRequest request)
    {
        try
        {
            // Check if email already exists
            if (await _context.Users.AnyAsync(u => u.Email == request.Email))
            {
                return Conflict(new { message = "Email already exists" });
            }

            var user = new Api.Models.User
            {
                FullName = request.FullName,
                Email = request.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                IsActive = request.IsActive,
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Load the user with roles for response
            await _context.Entry(user)
                .Collection(u => u.UserRoles)
                .LoadAsync();

            var response = new UserResponse
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt,
                Roles = user.UserRoles.Select(ur => ur.Role.Name).ToList()
            };

            // Audit log
            var currentUserId = GetCurrentUserId();
            await _auditService.LogAsync(
                "User",
                user.Id.ToString(),
                "Create",
                after: System.Text.Json.JsonSerializer.Serialize(response),
                actorUserId: currentUserId
            );

            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user with email: {Email}", request.Email);
            return StatusCode(500, new { message = "An error occurred while creating the user" });
        }
    }

    [HttpPatch("users/{id}")]
    public async Task<ActionResult<UserResponse>> UpdateUser(int id, [FromBody] UpdateUserRequest request)
    {
        try
        {
            var user = await _context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }

            var before = System.Text.Json.JsonSerializer.Serialize(new UserResponse
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt,
                Roles = user.UserRoles.Select(ur => ur.Role.Name).ToList()
            });

            // Update fields if provided
            if (!string.IsNullOrEmpty(request.FullName))
                user.FullName = request.FullName;
            
            if (!string.IsNullOrEmpty(request.Email))
            {
                if (await _context.Users.AnyAsync(u => u.Email == request.Email && u.Id != id))
                {
                    return Conflict(new { message = "Email already exists" });
                }
                user.Email = request.Email;
            }
            
            if (!string.IsNullOrEmpty(request.Password))
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
            
            if (request.IsActive.HasValue)
                user.IsActive = request.IsActive.Value;

            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            var after = System.Text.Json.JsonSerializer.Serialize(new UserResponse
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt,
                Roles = user.UserRoles.Select(ur => ur.Role.Name).ToList()
            });

            // Audit log
            var currentUserId = GetCurrentUserId();
            await _auditService.LogAsync(
                "User",
                user.Id.ToString(),
                "Update",
                before: before,
                after: after,
                actorUserId: currentUserId
            );

            return Ok(new UserResponse
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt,
                Roles = user.UserRoles.Select(ur => ur.Role.Name).ToList()
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user with ID: {UserId}", id);
            return StatusCode(500, new { message = "An error occurred while updating the user" });
        }
    }

    [HttpDelete("users/{id}")]
    public async Task<ActionResult> DeleteUser(int id)
    {
        try
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }

            var before = System.Text.Json.JsonSerializer.Serialize(new UserResponse
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt
            });

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            // Audit log
            var currentUserId = GetCurrentUserId();
            await _auditService.LogAsync(
                "User",
                id.ToString(),
                "Delete",
                before: before,
                actorUserId: currentUserId
            );

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user with ID: {UserId}", id);
            return StatusCode(500, new { message = "An error occurred while deleting the user" });
        }
    }

    [HttpPost("users/{id}/roles")]
    public async Task<ActionResult> AssignRole(int id, [FromBody] AssignRoleRequest request)
    {
        try
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }

            var role = await _context.Roles.FindAsync(request.RoleId);
            if (role == null)
            {
                return NotFound(new { message = "Role not found" });
            }

            // Check if user already has this role
            var existingUserRole = await _context.UserRoles
                .FirstOrDefaultAsync(ur => ur.UserId == id && ur.RoleId == request.RoleId);

            if (existingUserRole != null)
            {
                return Conflict(new { message = "User already has this role" });
            }

            var userRole = new Api.Models.UserRole
            {
                UserId = id,
                RoleId = request.RoleId,
                AssignedAt = DateTime.UtcNow
            };

            _context.UserRoles.Add(userRole);
            await _context.SaveChangesAsync();

            // Audit log
            var currentUserId = GetCurrentUserId();
            await _auditService.LogAsync(
                "UserRole",
                $"{id}:{request.RoleId}",
                "Assign",
                after: $"User {user.FullName} assigned role {role.Name}",
                actorUserId: currentUserId
            );

            return Ok(new { message = "Role assigned successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning role {RoleId} to user {UserId}", request.RoleId, id);
            return StatusCode(500, new { message = "An error occurred while assigning the role" });
        }
    }

    [HttpDelete("users/{id}/roles/{roleId}")]
    public async Task<ActionResult> RemoveRole(int id, int roleId)
    {
        try
        {
            var userRole = await _context.UserRoles
                .Include(ur => ur.User)
                .Include(ur => ur.Role)
                .FirstOrDefaultAsync(ur => ur.UserId == id && ur.RoleId == roleId);

            if (userRole == null)
            {
                return NotFound(new { message = "User role not found" });
            }

            _context.UserRoles.Remove(userRole);
            await _context.SaveChangesAsync();

            // Audit log
            var currentUserId = GetCurrentUserId();
            await _auditService.LogAsync(
                "UserRole",
                $"{id}:{roleId}",
                "Remove",
                before: $"User {userRole.User.FullName} had role {userRole.Role.Name}",
                actorUserId: currentUserId
            );

            return Ok(new { message = "Role removed successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing role {RoleId} from user {UserId}", roleId, id);
            return StatusCode(500, new { message = "An error occurred while removing the role" });
        }
    }

    #endregion

    #region Role Management

    [HttpGet("roles")]
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

    [HttpGet("roles/{id}")]
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

    [HttpPost("roles")]
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

    [HttpPatch("roles/{id}")]
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

    [HttpDelete("roles/{id}")]
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

    #endregion

    #region System Statistics

    [HttpGet("stats")]
    public async Task<ActionResult<object>> GetSystemStats()
    {
        try
        {
            var totalUsers = await _context.Users.CountAsync();
            var activeUsers = await _context.Users.CountAsync(u => u.IsActive);
            var totalRoles = await _context.Roles.CountAsync();
            var totalAuditLogs = await _context.AuditLogs.CountAsync();

            var usersByRole = await _context.UserRoles
                .Include(ur => ur.Role)
                .GroupBy(ur => ur.Role.Name)
                .Select(g => new { Role = g.Key, Count = g.Count() })
                .ToListAsync();

            return Ok(new
            {
                TotalUsers = totalUsers,
                ActiveUsers = activeUsers,
                InactiveUsers = totalUsers - activeUsers,
                TotalRoles = totalRoles,
                TotalAuditLogs = totalAuditLogs,
                UsersByRole = usersByRole
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving system statistics");
            return StatusCode(500, new { message = "An error occurred while retrieving system statistics" });
        }
    }

    #endregion

    private int? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        return userIdClaim != null && int.TryParse(userIdClaim.Value, out var userId) ? userId : null;
    }
}
