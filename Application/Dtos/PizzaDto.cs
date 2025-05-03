using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Dtos
{
    public class PizzaDto
    {
        [Required(ErrorMessage = "Название пиццы обязательно.")]
        [StringLength(150, ErrorMessage = "Название не должно превышать 150 символов.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Описание пиццы обязательно.")]
        public DescriptionDto Description { get; set; } = new();

        [Required(ErrorMessage = "Цена обязательна.")]
        [Range(0.01, 10000, ErrorMessage = "Цена должна быть от 0.01 до 10000.")]
        public decimal Price { get; set; }

        public bool IsVegetarian { get; set; }

        public string PhotoPath { get; set; } = string.Empty; // Только для чтения
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
