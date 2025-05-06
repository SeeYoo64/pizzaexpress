using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Infrastructure.Data;
using Domain;
using Application.Dtos;
using System.ComponentModel.DataAnnotations;
using Application.Services;
using System.Text.Json;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrdersController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<Order>>> GetAllOrders()
        {
            try
            {
                var orders = await _orderService.GetAllOrdersAsync();
                return Ok(orders);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при получении заказов: {ex.Message}");
                return StatusCode(500, "Внутренняя ошибка сервера.");
            }
        }

        /// <summary>
        /// Создать новый заказ
        /// </summary>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> PlaceOrder([FromBody] PlaceOrderRequestDto request)
        {
            try
            {
                Console.WriteLine($"Получен PlaceOrderRequest: {JsonSerializer.Serialize(request)}");

                var order = await _orderService.PlaceOrderAsync(request);
                return CreatedAtAction(nameof(PlaceOrder), new { id = order.Id }, null);
            }
            catch (ValidationException ex)
            {
                Console.WriteLine($"Ошибка валидации: {ex.Message}");
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при создании заказа: {ex.Message}");
                return StatusCode(500, "Внутренняя ошибка сервера.");
            }
        }


        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<OrderDto>> GetOrderById(int id)
        {
            try
            {
                var order = await _orderService.GetOrderByIdAsync(id);
                if (order == null)
                {
                    return NotFound($"Заказ с ID {id} не найден.");
                }
                Console.WriteLine($"Order loaded successfully: {JsonSerializer.Serialize(order)}");
                return Ok(order);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при получении заказа с ID {id}: {ex.Message}");
                return StatusCode(500, "Внутренняя ошибка сервера.");
            }
        }

        [HttpPut("{id}/status")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateOrderStatus(int id, [FromBody] UpdateOrderStatusDto dto)
        {
            try
            {
                if (!Enum.IsDefined(typeof(OrderStatus), dto.Status))
                {
                    return BadRequest("Некорректный статус заказа.");
                }

                await _orderService.UpdateOrderStatusAsync(id, dto.Status);
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine($"Ошибка валидации: {ex.Message}");
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при обновлении статуса заказа с ID {id}: {ex.Message}");
                return StatusCode(500, "Внутренняя ошибка сервера.");
            }
        }



    }

    public class UpdateOrderStatusDto
    {
        [Required(ErrorMessage = "Статус обязателен.")]
        public OrderStatus Status { get; set; }
    }


}
