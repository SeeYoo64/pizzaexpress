using System.ComponentModel.DataAnnotations;
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
        private readonly IWebHostEnvironment _environment;

        public PizzasController(AppDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        /// <summary>
        /// Получить список всех пицц.
        /// </summary>
        /// <returns>Список пицц с полными URL для изображений.</returns>
        /// <response code="200">Возвращает список пицц.</response>
        /// <response code="500">Ошибка сервера.</response>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<Pizza>>> GetPizzas()
        {
            try
            {
                var pizzas = await _context.Pizzas.ToListAsync();
                // Формируем полные URL для PhotoPath
                foreach (var pizza in pizzas)
                {
                    if (!string.IsNullOrEmpty(pizza.PhotoPath))
                    {
                        pizza.PhotoPath = $"{Request.Scheme}://{Request.Host}/images/{pizza.PhotoPath}";
                    }
                }
                return Ok(pizzas);
            }
            catch (Exception ex)
            {
                // Логирование ошибки (предполагается, что Serilog настроен)
                Console.WriteLine($"Ошибка при получении пицц: {ex.Message}");
                return StatusCode(500, "Внутренняя ошибка сервера.");
            }
        }



        /// <summary>
        /// Получить пиццу по ID.
        /// </summary>
        /// <param name="id">ID пиццы.</param>
        /// <returns>Пицца с полным URL для изображения.</returns>
        /// <response code="200">Возвращает пиццу.</response>
        /// <response code="404">Пицца не найдена.</response>
        /// <response code="500">Ошибка сервера.</response>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Pizza>> GetPizza(int id)
        {
            try
            {
                var pizza = await _context.Pizzas.FindAsync(id);
                if (pizza == null)
                {
                    return NotFound($"Пицца с ID {id} не найдена.");
                }

                if (!string.IsNullOrEmpty(pizza.PhotoPath))
                {
                    pizza.PhotoPath = $"{Request.Scheme}://{Request.Host}/images/{pizza.PhotoPath}";
                }
                return Ok(pizza);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при получении пиццы с ID {id}: {ex.Message}");
                return StatusCode(500, "Внутренняя ошибка сервера.");
            }
        }



        /// <summary>
        /// Создать новую пиццу (только для админов).
        /// </summary>
        /// <param name="pizzaDto">Данные пиццы.</param>
        /// <returns>Созданная пицца.</returns>
        /// <response code="201">Пицца создана.</response>
        /// <response code="400">Некорректные данные.</response>
        /// <response code="500">Ошибка сервера.</response>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Pizza>> CreatePizza([FromBody] PizzaDto pizzaDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var pizza = new Pizza
                {
                    Name = pizzaDto.Name,
                    Description = new Description
                    {
                        Text = pizzaDto.Description.Text,
                        Ingredients = pizzaDto.Description.Ingredients ?? new List<string>(),
                        Weight = pizzaDto.Description.Weight
                    },
                    Price = pizzaDto.Price,
                    IsVegetarian = pizzaDto.IsVegetarian,
                    PhotoPath = pizzaDto.PhotoPath
                };

                _context.Pizzas.Add(pizza);
                await _context.SaveChangesAsync();

                // Формируем полный URL для PhotoPath
                if (!string.IsNullOrEmpty(pizza.PhotoPath))
                {
                    pizza.PhotoPath = $"{Request.Scheme}://{Request.Host}/images/{pizza.PhotoPath}";
                }

                return CreatedAtAction(nameof(GetPizza), new { id = pizza.Id }, pizza);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при создании пиццы: {ex.Message}");
                return StatusCode(500, "Внутренняя ошибка сервера.");
            }
        }



        /// <summary>
        /// Обновить пиццу (только для админов).
        /// </summary>
        /// <param name="id">ID пиццы.</param>
        /// <param name="pizzaDto">Обновленные данные пиццы.</param>
        /// <returns>Нет содержимого.</returns>
        /// <response code="204">Пицца обновлена.</response>
        /// <response code="400">Некорректные данные.</response>
        /// <response code="404">Пицца не найдена.</response>
        /// <response code="500">Ошибка сервера.</response>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdatePizza(int id, [FromBody] PizzaDto pizzaDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var pizza = await _context.Pizzas.FindAsync(id);
                if (pizza == null)
                {
                    return NotFound($"Пицца с ID {id} не найдена.");
                }

                pizza.Name = pizzaDto.Name;
                pizza.Description.Text = pizzaDto.Description.Text;
                pizza.Description.Ingredients = pizzaDto.Description.Ingredients ?? new List<string>();
                pizza.Description.Weight = pizzaDto.Description.Weight;
                pizza.Price = pizzaDto.Price;
                pizza.IsVegetarian = pizzaDto.IsVegetarian;
                pizza.PhotoPath = pizzaDto.PhotoPath;

                _context.Pizzas.Update(pizza);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при обновлении пиццы с ID {id}: {ex.Message}");
                return StatusCode(500, "Внутренняя ошибка сервера.");
            }
        }



        /// <summary>
        /// Удалить пиццу (только для админов).
        /// </summary>
        /// <param name="id">ID пиццы.</param>
        /// <returns>Нет содержимого.</returns>
        /// <response code="204">Пицца удалена.</response>
        /// <response code="404">Пицца не найдена.</response>
        /// <response code="500">Ошибка сервера.</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeletePizza(int id)
        {
            try
            {
                var pizza = await _context.Pizzas.FindAsync(id);
                if (pizza == null)
                {
                    return NotFound($"Пицца с ID {id} не найдена.");
                }

                _context.Pizzas.Remove(pizza);
                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при удалении пиццы с ID {id}: {ex.Message}");
                return StatusCode(500, "Внутренняя ошибка сервера.");
            }
        }
    }

    /// <summary>
    /// DTO для создания и обновления пиццы.
    /// </summary>
    public class PizzaDto
    {
        [Required(ErrorMessage = "Название пиццы обязательно.")]
        [StringLength(100, ErrorMessage = "Название не должно превышать 100 символов.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Описание пиццы обязательно.")]
        public DescriptionDto Description { get; set; } = new();

        [Required(ErrorMessage = "Цена обязательна.")]
        [Range(0.01, 10000, ErrorMessage = "Цена должна быть от 0.01 до 10000.")]
        public decimal Price { get; set; }

        public bool IsVegetarian { get; set; }

        [StringLength(200, ErrorMessage = "Путь к изображению не должен превышать 200 символов.")]
        public string PhotoPath { get; set; } = string.Empty;
    }

    public class DescriptionDto
    {
        [Required(ErrorMessage = "Текст описания обязателен.")]
        [StringLength(500, ErrorMessage = "Текст описания не должен превышать 500 символов.")]
        public string Text { get; set; } = string.Empty;

        public List<string> Ingredients { get; set; } = new();

        [StringLength(50, ErrorMessage = "Вес не должен превышать 50 символов.")]
        public string Weight { get; set; } = string.Empty;
    }
}