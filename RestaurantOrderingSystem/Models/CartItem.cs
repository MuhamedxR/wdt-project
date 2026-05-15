namespace RestaurantOrderingSystem.Models
{
    public class CartItem
    {
       // Session-based
            public int MenuItemId { get; set; }

            public string Name { get; set; }

            public decimal Price { get; set; }

            public int Quantity { get; set; }

            public decimal Total =>
                Price * Quantity;
        
    }
}
