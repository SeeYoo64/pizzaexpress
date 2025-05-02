using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Infrastructure.Data;
using Domain;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly AppDbContext _context;
        public OrdersController(AppDbContext context) => _context = context;


        /// <summary>
        /// Создать новый заказ
        /// </summary>
        [HttpPost]
        public async Task<ActionResult> CreateOrder([FromBody] CreateOrderDto orderDto)
        {
            var order = new Order
            {
                TotalPrice = orderDto.TotalPrice,
                Items = orderDto.Items.Select(i => new OrderItem { PizzaId = i.PizzaId, Quantity = i.Quantity }).ToList()
            };
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(CreateOrder), new { id = order.Id }, order);
        }

    }

    public class CreateOrderDto
    {
        public List<OrderItemDto> Items { get; set; } = new();
        public decimal TotalPrice { get; set; }
    }

    public class OrderItemDto
    {
        public int PizzaId { get; set; }
        public int Quantity { get; set; }
    }


}
