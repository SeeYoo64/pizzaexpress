using Domain;
using Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PizzasController : ControllerBase
    {
        private readonly AppDbContext _context;
        public PizzasController(AppDbContext context) => _context = context;

        /// <summary>
        /// Получить список всех пицц
        /// </summary>
        /// <returns>Список пицц</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Pizza>>> GetPizzas()
        {
            return await _context.Pizzas.ToListAsync();
        }

        /// <summary>
        /// Создать новую пиццу (только для админов)
        /// </summary>
        /// <param name="pizza">Данные пиццы</param>
        /// <returns>Созданная пицца</returns>
        [HttpPost]
        public async Task<ActionResult<Pizza>> CreatePizza(Pizza pizza)
        {
            _context.Pizzas.Add(pizza);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetPizzas), new { id = pizza.Id }, pizza);
        }
    }
}
