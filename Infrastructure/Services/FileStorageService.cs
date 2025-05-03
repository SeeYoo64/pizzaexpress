using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;


namespace Infrastructure.Services
{
    public interface IFileStorageService
    {
        Task<string> SavePizzaImageAsync(int pizzaId, IFormFile image);
        void DeletePizzaImageFolder(int pizzaId);
        string GetFullUrl(string relativePath);
    }

    public class FileStorageService : IFileStorageService
    {
        private readonly string _webRootPath;
        private readonly string _baseUrl;

        public FileStorageService(IWebHostEnvironment environment, IHttpContextAccessor httpContextAccessor)
        {
            _webRootPath = environment.WebRootPath;
            _baseUrl = $"{httpContextAccessor.HttpContext!.Request.Scheme}://{httpContextAccessor.HttpContext!.Request.Host}";
        }

        public async Task<string> SavePizzaImageAsync(int pizzaId, IFormFile image)
        {
            var extension = Path.GetExtension(image.FileName).ToLower();
            if (!new[] { ".jpg", ".jpeg", ".png" }.Contains(extension))
            {
                throw new ValidationException("Поддерживаются только файлы .jpg, .jpeg, .png.");
            }

            if (image.Length > 5 * 1024 * 1024) // 5MB
            {
                throw new ValidationException("Размер изображения не должен превышать 5 МБ.");
            }

            var date = DateTime.UtcNow.ToString("yyyyMMdd");
            var fileName = $"photopizza{pizzaId}_{date}{extension}";
            var pizzaFolder = Path.Combine(_webRootPath, "Uploads", "Pizzas", pizzaId.ToString());
            var filePath = Path.Combine(pizzaFolder, fileName);

            Directory.CreateDirectory(pizzaFolder);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await image.CopyToAsync(stream);
            }

            return Path.Combine("Uploads", "Pizzas", pizzaId.ToString(), fileName).Replace("\\", "/");
        }

        public void DeletePizzaImageFolder(int pizzaId)
        {
            var pizzaFolder = Path.Combine(_webRootPath, "Uploads", "Pizzas", pizzaId.ToString());
            if (Directory.Exists(pizzaFolder))
            {
                Directory.Delete(pizzaFolder, true);
            }
        }

        public string GetFullUrl(string relativePath)
        {
            if (string.IsNullOrEmpty(relativePath))
                return string.Empty;
            return $"{_baseUrl}/{relativePath}";
        }
    }
}
