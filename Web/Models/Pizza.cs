namespace Web.Models
{
    public class Pizza
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public Description Description { get; set; } = new();
        public decimal Price { get; set; }
        public bool IsVegetarian { get; set; }
        public string PhotoPath { get; set; } = string.Empty;
    }


    public class Description
    {
        public string Text { get; set; } = string.Empty;
        public List<string> Ingredients { get; set; } = new();
        public string Weight { get; set; } = string.Empty;
    }
}
