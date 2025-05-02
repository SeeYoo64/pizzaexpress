using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain
{
    public class Order
    {
        public int Id { get; set; }
        public List<OrderItem> Items { get; set; } = new();
        public decimal TotalPrice { get; set; }
        public string Status { get; set; } = "Pending";
    }

    public class OrderItem
    {
        public int Id { get; set; }
        public int PizzaId { get; set; }
        public int Quantity { get; set; }
    }
}
