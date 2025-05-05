using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Web;
using Web.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");
builder.Services.AddScoped<CartService>();
builder.Services.AddScoped<PizzasService>();
builder.Services.AddScoped<PizzaAdminService>();


builder.Services.AddScoped(sp => new HttpClient { 
    BaseAddress = new Uri("http://localhost:5013") 
});



builder.Services.AddBlazoredLocalStorage(config =>
{
    config.JsonSerializerOptions.WriteIndented = false;
});
await builder.Build().RunAsync();
