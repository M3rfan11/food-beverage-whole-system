namespace Api.DTOs
{
    public class CreateProductRequest
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public string? Unit { get; set; }
        public string? SKU { get; set; }
        public string? Brand { get; set; }
        public decimal? Weight { get; set; }
        public string? Dimensions { get; set; }
        public int CategoryId { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class UpdateProductRequest
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public decimal? Price { get; set; }
        public string? Unit { get; set; }
        public string? SKU { get; set; }
        public string? Brand { get; set; }
        public decimal? Weight { get; set; }
        public string? Dimensions { get; set; }
        public int? CategoryId { get; set; }
        public bool? IsActive { get; set; }
    }

    public class ProductResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public string? Unit { get; set; }
        public string? SKU { get; set; }
        public string? Brand { get; set; }
        public decimal? Weight { get; set; }
        public string? Dimensions { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public decimal TotalQuantity { get; set; }
        public List<ProductInventoryResponse> Inventories { get; set; } = new List<ProductInventoryResponse>();
    }

    public class ProductListResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public string? Unit { get; set; }
        public string? SKU { get; set; }
        public string? Brand { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public decimal TotalQuantity { get; set; }
    }

    public class ProductInventoryResponse
    {
        public int Id { get; set; }
        public int WarehouseId { get; set; }
        public string WarehouseName { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public string? Unit { get; set; }
        public decimal? MinimumStockLevel { get; set; }
        public decimal? MaximumStockLevel { get; set; }
    }

    public class UpdateInventoryRequest
    {
        public decimal Quantity { get; set; }
        public string? Unit { get; set; }
        public decimal? MinimumStockLevel { get; set; }
        public decimal? MaximumStockLevel { get; set; }
    }
}
