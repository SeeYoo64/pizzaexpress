namespace Web.Models
{
    public class CartItem
    {
        public Pizza Pizza { get; set; } = new();
        public int Quantity { get; set; }

    }
}
