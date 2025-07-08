using System;

namespace JuniorTechTest;

public class Order
{
    public required string Id { get; set; }
    public required int CustomerId { get; set; }
    public required bool ContainsAllergens { get; set; }
    public required List<OrderItem> Items { get; set; } 
    public required DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public decimal? TotalCost { get; set; }
}

public class OrderItem
{
    public int OrderItemId { get; set; }
    public required string Name { get; set; }
    public required decimal Cost { get; set; }
}

// this is a model of how our DB will seed data