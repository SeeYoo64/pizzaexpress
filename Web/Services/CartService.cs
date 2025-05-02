using System.Net.Http.Json;
using Blazored.LocalStorage;
using System.Text.Json;
using Web.Models;

namespace Web.Services;

public class CartService
{
    private readonly ILocalStorageService _localStorage;

    public event Action OnChange;

    public CartService(ILocalStorageService localStorage)
    {
        _localStorage = localStorage;
    }

    public async Task<List<CartItem>> GetCartItemsAsync()
    {
        var storedCart = await _localStorage.GetItemAsStringAsync("cart");
        return string.IsNullOrEmpty(storedCart)
            ? new List<CartItem>()
            : JsonSerializer.Deserialize<List<CartItem>>(storedCart);
    }

    public async Task AddItemAsync(Pizza pizza, int quantity = 1)
    {
        var cart = await GetCartItemsAsync();
        var existingItem = cart.FirstOrDefault(c => c.Pizza.Id == pizza.Id);

        if (existingItem != null)
        {
            existingItem.Quantity += quantity;
        }
        else
        {
            cart.Add(new CartItem { Pizza = pizza, Quantity = quantity });
        }

        await _localStorage.SetItemAsStringAsync("cart", JsonSerializer.Serialize(cart));
        NotifyStateChanged();
    }

    public async Task RemoveItemAsync(int pizzaId)
    {
        var cart = await GetCartItemsAsync();
        var item = cart.FirstOrDefault(c => c.Pizza.Id == pizzaId);
        if (item != null)
        {
            cart.Remove(item);
            await _localStorage.SetItemAsStringAsync("cart", JsonSerializer.Serialize(cart));
            NotifyStateChanged();
        }
    }

    public async Task ClearCartAsync()
    {
        await _localStorage.RemoveItemAsync("cart");
        NotifyStateChanged();
    }

    private void NotifyStateChanged() => OnChange?.Invoke();
}
