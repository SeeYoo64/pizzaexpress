using System.Net.Http.Json;
using Web.Models;

namespace Web.Services
{
    public class PizzaAdminService
    {
        private readonly HttpClient _http;

        public PizzaAdminService(HttpClient httpClient)
        {
            _http = httpClient;
        }

        public async Task<List<Pizza>> GetPizzasAsync()
        {
            return await _http.GetFromJsonAsync<List<Pizza>>("api/pizzas") ?? new List<Pizza>();
        }

        public async Task<Pizza> GetPizzaByIdAsync(int id)
        {
            return await _http.GetFromJsonAsync<Pizza>($"api/pizzas/{id}") ??
                   new Pizza { Description = new Description() };
        }


        public async Task CreatePizzaAsync(Pizza pizza)
        {
            await _http.PostAsJsonAsync("api/pizzas", pizza);
        }

        public async Task UpdatePizzaAsync(Pizza pizza)
        {
            await _http.PutAsJsonAsync($"api/pizzas/{pizza.Id}", pizza);
        }

        public async Task DeletePizzaAsync(int id)
        {
            await _http.DeleteAsync($"api/pizzas/{id}");
            
        }

    }
}
