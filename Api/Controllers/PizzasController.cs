using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Api.Models;
using Application.Dtos;
using Application.Services;
using Domain;
using Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static System.Net.Mime.MediaTypeNames;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PizzasController : ControllerBase
    {
        private readonly IPizzaService _pizzaService;

        public PizzasController(IPizzaService pizzaService)
        {
            _pizzaService = pizzaService;
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
                var pizzas = await _pizzaService.GetAllAsync();

                return Ok(pizzas);
            }
            catch (Exception ex)
            {
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
                var pizza = await _pizzaService.GetByIdAsync(id);
                if (pizza == null)
                {
                    return NotFound($"Пицца с ID {id} не найдена.");
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
        /// <param name="pizzaDto">Данные пиццы в формате JSON.</param>
        /// <param name="image">Файл изображения (jpg, jpeg, png, опционально).</param>
        [HttpPost]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<ActionResult<Pizza>> CreatePizza([FromForm] string pizzaDto, [FromForm] IFormFile? image)
        {
            try
            {
                // Десериализуем JSON-строку в PizzaDto
                var dto = JsonSerializer.Deserialize<PizzaDto>(pizzaDto, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    PropertyNameCaseInsensitive = true
                });

                if (dto == null)
                {
                    return BadRequest("Некорректный формат PizzaDto.");
                }

                // Валидация модели
                var validationContext = new ValidationContext(dto);
                var validationResults = new List<ValidationResult>();
                if (!Validator.TryValidateObject(dto, validationContext, validationResults, true))
                {
                    var errors = validationResults.ToDictionary(
                        vr => vr.MemberNames.FirstOrDefault() ?? "Unknown",
                        vr => vr.ErrorMessage);
                    return BadRequest(new { Errors = errors });
                }

                var pizza = await _pizzaService.CreateAsync(dto, image);
                return CreatedAtAction(nameof(GetPizza), new { id = pizza.Id }, pizza);
            }
            catch (JsonException ex)
            {
                return BadRequest($"Ошибка десериализации PizzaDto: {ex.Message}");
            }
            catch (ValidationException ex)
            {
                return BadRequest(ex.Message);
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
        /// <param name="pizzaDto">Обновленные данные пиццы в формате JSON.</param>
        /// <param name="image">Новый файл изображения (опционально).</param>
        [HttpPut("{id}")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<IActionResult> UpdatePizza(int id, [FromForm] string pizzaDto, [FromForm] IFormFile? image)
        {
            try
            {
                Console.WriteLine($"Получен PizzaDto для обновления ID {id}: {pizzaDto}");
                var dto = JsonSerializer.Deserialize<PizzaDto>(pizzaDto, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    PropertyNameCaseInsensitive = true
                });

                if (dto == null)
                {
                    return BadRequest("Некорректный формат PizzaDto.");
                }

                Console.WriteLine($"Десериализован PizzaDto: Name={dto.Name}, Price={dto.Price}, Description.Text={dto.Description.Text}");

                var validationContext = new ValidationContext(dto);
                var validationResults = new List<ValidationResult>();
                if (!Validator.TryValidateObject(dto, validationContext, validationResults, true))
                {
                    var errors = validationResults.ToDictionary(
                        vr => vr.MemberNames.FirstOrDefault() ?? "Unknown",
                        vr => vr.ErrorMessage);
                    Console.WriteLine($"Ошибки валидации: {JsonSerializer.Serialize(errors)}");
                    return BadRequest(new { Errors = errors });
                }

                await _pizzaService.UpdateAsync(id, dto, image);
                return NoContent();
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"Ошибка десериализации: {ex.Message}");
                return BadRequest($"Ошибка десериализации PizzaDto: {ex.Message}");
            }
            catch (ValidationException ex)
            {
                Console.WriteLine($"Ошибка валидации: {ex.Message}");
                return ex.Message.Contains("не найдена") ? NotFound(ex.Message) : BadRequest(ex.Message);
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
                await _pizzaService.DeleteAsync(id);
                return NoContent();
            }
            catch (ValidationException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при удалении пиццы с ID {id}: {ex.Message}");
                return StatusCode(500, "Внутренняя ошибка сервера.");
            }
        }
    }
}