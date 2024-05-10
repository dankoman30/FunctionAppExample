namespace FunctionAppExample
{
    public class GroceryItem
    {
        public int Id { get; set; } // Auto-increment primary key
        public string ItemName { get; set; } // Name of the grocery item
        public int Quantity { get; set; } // Quantity of the item
        public bool IsPurchased { get; set; } // Status if the item is purchased
    }
}
