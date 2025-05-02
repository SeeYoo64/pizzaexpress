using System.Net.Http.Json;
using Web.Models;

namespace Web.Services
{
    public class PizzasService
    {
        private readonly HttpClient _httpClient;

        public PizzasService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<Pizza[]> GetPizzasAsync()
        {
            return await _httpClient.GetFromJsonAsync<Pizza[]>("http://localhost:5013/api/Pizzas");
        }

        public async Task AddToCartAsync(CartItem cartItem)
        {
            await _httpClient.PostAsJsonAsync("api/Carts", cartItem);
        }
    }
}
