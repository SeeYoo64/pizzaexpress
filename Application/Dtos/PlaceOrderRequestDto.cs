using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Dtos
{
    public class PlaceOrderRequestDto
    {
        [Required(ErrorMessage = "Имя клиента обязательно.")]
        [StringLength(100, ErrorMessage = "Имя не должно превышать 100 символов.")]
        public string CustomerName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Телефон обязателен.")]
        [Phone(ErrorMessage = "Некорректный формат телефона.")]
        public string Phone { get; set; } = string.Empty;

        [Required(ErrorMessage = "Адрес обязателен.")]
        [StringLength(200, ErrorMessage = "Адрес не должен превышать 200 символов.")]
        public string Address { get; set; } = string.Empty;

        [Required(ErrorMessage = "Список элементов заказа обязателен.")]
        [MinLength(1, ErrorMessage = "Заказ должен содержать хотя бы один элемент.")]
        public List<OrderItemDto> Items { get; set; } = new();

    }


    public class OrderItemDto
    {
        [Required(ErrorMessage = "ID пиццы обязателен.")]
        [Range(1, int.MaxValue, ErrorMessage = "ID пиццы должен быть больше 0.")]
        public int PizzaId { get; set; }

        [Required(ErrorMessage = "Количество обязательно.")]
        [Range(1, 100, ErrorMessage = "Количество должно быть от 1 до 100.")]
        public int Quantity { get; set; }
    }







}
