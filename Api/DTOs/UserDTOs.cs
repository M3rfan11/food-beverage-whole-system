using System.ComponentModel.DataAnnotations;

namespace Api.DTOs;

public class CreateUserRequest
{
    [Required]
    [MaxLength(100)]
    public string FullName { get; set; } = string.Empty;
    
    [Required]
    [EmailAddress]
    [MaxLength(255)]
    public string Email { get; set; } = string.Empty;
    
    [Required]
    [MinLength(6)]
    public string Password { get; set; } = string.Empty;
    
    public bool IsActive { get; set; } = true;
}

public class UpdateUserRequest
{
    [MaxLength(100)]
    public string? FullName { get; set; }
    
    [EmailAddress]
    [MaxLength(255)]
    public string? Email { get; set; }
    
    [MinLength(6)]
    public string? Password { get; set; }
    
    public bool? IsActive { get; set; }
}

public class UserResponse
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public List<string> Roles { get; set; } = new List<string>();
}
