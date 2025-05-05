using Microsoft.AspNetCore.Components.Forms;
using System.ComponentModel.DataAnnotations;

namespace Web.Models
{
    public class PizzaCreateRequest
    {
        public PizzaDto PizzaDto { get; set; } = new();
        public IBrowserFile? Image { get; set; }
    }

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

        public string? PhotoPath { get; set; } = string.Empty; // Только для чтения
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


    public class PizzaFormModel
    {
        [Required(ErrorMessage = "Название пиццы обязательно.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Описание пиццы обязательно.")]
        public string DescriptionText { get; set; } = string.Empty;

        public string Weight { get; set; } = string.Empty;

        public List<string> Ingredients { get; set; } = new();

        [Required(ErrorMessage = "Цена обязательна.")]
        [Range(0.01, 10000, ErrorMessage = "Цена должна быть от 0.01 до 10000.")]
        public decimal Price { get; set; }

        public bool IsVegetarian { get; set; }

        public IBrowserFile? Image { get; set; }
    }


}
