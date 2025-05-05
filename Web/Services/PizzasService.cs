using System.Net.Http.Json;
using System.Text.Json;
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
            try
            {
                var response = await _httpClient.GetAsync("api/Pizzas");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();

                    try
                    {
                        var pizzas = await response.Content.ReadFromJsonAsync<Pizza[]>();
                        return pizzas ?? Array.Empty<Pizza>();
                    }
                    catch (JsonException ex)
                    {
                        Console.Error.WriteLine("JSON Deserialization Failed: " + ex.Message);
                        Console.Error.WriteLine("Raw Response Content: " + content);
                        return Array.Empty<Pizza>();
                    }
                }
                else
                {
                    Console.Error.WriteLine($"API Request Failed: {response.StatusCode}");
                    Console.Error.WriteLine("Response Body: " + await response.Content.ReadAsStringAsync());
                    return Array.Empty<Pizza>();
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("HTTP Request Exception: " + ex.Message);
                return Array.Empty<Pizza>();
            }
        }

        public async Task AddToCartAsync(CartItem cartItem)
        {
            await _httpClient.PostAsJsonAsync("api/Carts", cartItem);
        }
    }
}
