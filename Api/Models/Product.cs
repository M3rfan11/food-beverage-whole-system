using System.ComponentModel.DataAnnotations;

namespace Api.Models
{
    public class Product
    {
        public int Id { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;
        
        [MaxLength(500)]
        public string? Description { get; set; }
        
        [Required]
        public decimal Price { get; set; }
        
        [MaxLength(50)]
        public string? Unit { get; set; } // e.g., "piece", "box", "kg", "liter"
        
        [MaxLength(50)]
        public string? SKU { get; set; } // Stock Keeping Unit
        
        [MaxLength(100)]
        public string? Brand { get; set; }
        
        public decimal? Weight { get; set; } // in kg
        
        [MaxLength(50)]
        public string? Dimensions { get; set; } // e.g., "10x20x30 cm"
        
        public int CategoryId { get; set; }
        
        public bool IsActive { get; set; } = true;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? UpdatedAt { get; set; }
        
        // Navigation properties
        public virtual Category Category { get; set; } = null!;
        public virtual ICollection<ProductInventory> ProductInventories { get; set; } = new List<ProductInventory>();
    }
}
