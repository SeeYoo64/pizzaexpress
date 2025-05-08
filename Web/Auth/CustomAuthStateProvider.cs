using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json;

namespace Web.Auth
{
    public class CustomAuthStateProvider : AuthenticationStateProvider
    {
        private readonly IJSRuntime _jsRuntime;
        private readonly HttpClient _http;
        private readonly ILocalStorageService _localStorage;

        public CustomAuthStateProvider(IJSRuntime jsRuntime, HttpClient http, ILocalStorageService localStorage)
        {
            _jsRuntime = jsRuntime;
            _http = http;
            _localStorage = localStorage;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var token = await _localStorage.GetItemAsStringAsync("authToken");

            if (string.IsNullOrEmpty(token))
            {
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }

            var user = ParseClaimsFromJwt(token);
            var identity = new ClaimsIdentity(user, "Bearer");
            var principal = new ClaimsPrincipal(identity);

            _http.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            return new AuthenticationState(principal);
        }

        public async Task Login(string username, string password)
        {
            var response = await _http.PostAsJsonAsync("api/account/login", new Pages.Admin.Login.LoginDto { Username = username, Password = password });

            if (response.IsSuccessStatusCode)
            {
                var token = await response.Content.ReadAsStringAsync();
                await _localStorage.SetItemAsStringAsync("authToken", token);
                NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
            }
            else
            {
                throw new Exception("Invalid login attempt.");
            }
        }

        public async Task Logout()
        {
            await _localStorage.RemoveItemAsync("authToken");
            _http.DefaultRequestHeaders.Authorization = null;

            await _jsRuntime.InvokeVoidAsync("logoutHelper.clearCookies");

            try
            {
                await _http.PostAsync("api/account/logout", null);
            }
            catch
            {
                // Ignore errors
            }

            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()))));
        }

        private IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
        {
            var payload = jwt.Split('.')[1];
            var jsonBytes = ParseBase64WithoutPadding(payload);
            var keyValuePairs = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonBytes);

            var claims = new List<Claim>();

            if (keyValuePairs != null)
            {
                foreach (var pair in keyValuePairs)
                {
                    if (pair.Key == "role")
                    {
                        claims.Add(new Claim(ClaimTypes.Role, pair.Value.ToString()));
                    }
                    else if (pair.Key == "unique_name")
                    {
                        claims.Add(new Claim(ClaimTypes.Name, pair.Value.ToString()));
                    }
                    else if (pair.Key == "nameid")
                    {
                        claims.Add(new Claim(ClaimTypes.NameIdentifier, pair.Value.ToString()));
                    }
                    else if (pair.Key.StartsWith("http"))
                    {
                        claims.Add(new Claim(pair.Key, pair.Value.ToString()));
                    }
                    else
                    {
                        claims.Add(new Claim(pair.Key, pair.Value.ToString()));
                    }
                }
            }

            return claims;
        }

        private byte[] ParseBase64WithoutPadding(string base64)
        {
            switch (base64.Length % 4)
            {
                case 2: base64 += "=="; break;
                case 3: base64 += "="; break;
            }

            return Convert.FromBase64String(base64);
        }
    }

}
