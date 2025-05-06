using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Dtos;
using Domain;
using Infrastructure.Repositories;

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

        public OrderService(IOrderRepository orderRepository, IPizzaRepository pizzaRepository)
        {
            _orderRepository = orderRepository;
            _pizzaRepository = pizzaRepository;
        }

        public async Task<Order> PlaceOrderAsync(PlaceOrderRequestDto request)
        {
            // Валидация DTO
            var context = new ValidationContext(request);
            Validator.ValidateObject(request, context, true);

            // Проверка существования пицц и сбор цен
            var pizzaIds = request.Items.Select(i => i.PizzaId).Distinct().ToList();
            var pizzas = await _pizzaRepository.GetByIdsAsync(pizzaIds);

            // Validate that all requested pizzas exist
            var missingPizzas = pizzaIds.Except(pizzas.Select(p => p.Id)).ToList();
            if (missingPizzas.Any())
            {
                throw new ValidationException($"The following pizzas were not found: {string.Join(", ", missingPizzas)}");
            }

            // Создание заказа
            var order = new Order
            {
                CustomerName = request.CustomerName,
                Phone = request.Phone,
                Address = request.Address,
                CreatedAt = DateTime.UtcNow,
                Items = request.Items.Select(i => new OrderItem
                {
                    PizzaId = i.PizzaId,
                    Quantity = i.Quantity,
                    PriceAtOrder = pizzas.First(p => p.Id == i.PizzaId).Price
                }).ToList()
            };

            // Вычисление общей стоимости
            order.TotalPrice = order.Items.Sum(i => i.Quantity * i.PriceAtOrder);

            // Сохранение заказа
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
