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


    }
}
