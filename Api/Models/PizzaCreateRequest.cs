using Application.Dtos;

namespace Api.Models
{
    public class PizzaCreateRequest
    {
        public PizzaDto PizzaDto { get; set; } = new PizzaDto();
        public IFormFile? Image { get; set; }
    }
}
