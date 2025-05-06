using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Dtos;
using Domain;
using Infrastructure.Repositories;
using Infrastructure.Services;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Application.Services
{
    public interface IOrderService
    {
        Task<Order> PlaceOrderAsync(PlaceOrderRequestDto request);
        Task<IEnumerable<Order>> GetAllOrdersAsync();
        Task<OrderDto?> GetOrderByIdAsync(int id);
        Task UpdateOrderStatusAsync(int id, OrderStatus status);
    }
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IPizzaRepository _pizzaRepository;
        private readonly ITelegramBotService _telegramBotService;
        public OrderService(
            IOrderRepository orderRepository, 
            IPizzaRepository pizzaRepository,
            ITelegramBotService telegramBotService)
        {
            _orderRepository = orderRepository;
            _pizzaRepository = pizzaRepository;
            _telegramBotService = telegramBotService;
        }

        public async Task<Order> PlaceOrderAsync(PlaceOrderRequestDto request)
        {
            Validator.ValidateObject(request, new ValidationContext(request), true);

            var pizzaIds = request.Items.Select(i => i.PizzaId).Distinct().ToList();
            var pizzas = await _pizzaRepository.GetByIdsAsync(pizzaIds);
            var pizzaDict = pizzas.ToDictionary(p => p.Id);

            var missingPizzas = pizzaIds.Except(pizzaDict.Keys).ToList();
            if (missingPizzas.Any())
                throw new ValidationException($"Пиццы не найдены: {string.Join(", ", missingPizzas)}");

            var orderItems = request.Items.Select(i => new OrderItem
            {
                PizzaId = i.PizzaId,
                Quantity = i.Quantity,
                PriceAtOrder = pizzaDict[i.PizzaId].Price,
                Pizza = pizzaDict[i.PizzaId]
            }).ToList();

            var order = new Order
            {
                CustomerName = request.CustomerName,
                Phone = request.Phone,
                Address = request.Address,
                CreatedAt = DateTime.Now,
                Items = orderItems,
                TotalPrice = orderItems.Sum(i => i.Quantity * i.PriceAtOrder)
            };

            try
            {
                await _telegramBotService.SendOrderNotificationAsync(order);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString(), "Ошибка при отправке в Telegram");
            }

            return await _orderRepository.CreateAsync(order);
        }

        public async Task<IEnumerable<Order>> GetAllOrdersAsync()
        {
            return await _orderRepository.GetAllAsync();
        }

        public async Task<OrderDto?> GetOrderByIdAsync(int id)
        {
            var order = await _orderRepository.GetByIdAsync(id);
            if (order == null)
            {
                return null;
            }

            return new OrderDto
            {
                Id = order.Id,
                CustomerName = order.CustomerName,
                Phone = order.Phone,
                Address = order.Address,
                TotalPrice = order.TotalPrice,
                CreatedAt = order.CreatedAt,
                Status = order.Status.ToString(),
                Items = order.Items.Select(i => new OrderItemDto
                {
                    PizzaId = i.PizzaId,
                    PizzaName = i.Pizza?.Name ?? "Unknown", // Проверка на null
                    Quantity = i.Quantity,
                    Price = i.PriceAtOrder
                }).ToList()
            };
        }

        public async Task UpdateOrderStatusAsync(int id, OrderStatus status)
        {
            await _orderRepository.UpdateStatusAsync(id, status);
        }




    }
}
