﻿using Microsoft.AspNetCore.Http;
using Domain;
using Infrastructure.Repositories;
using Application.Dtos;
using System.ComponentModel.DataAnnotations;
using System.IO;
using Infrastructure.Services;

namespace Application.Services
{
    public interface IPizzaService
    {
        Task<IEnumerable<Pizza>> GetAllAsync();
        Task<Pizza?> GetByIdAsync(int id);
        Task<Pizza> CreateAsync(PizzaDto pizzaDto, IFormFile? image);
        Task<Pizza> UpdateAsync(int id, PizzaDto pizzaDto, IFormFile? image);
        Task DeleteAsync(int id);
    }

    public class PizzaService : IPizzaService
    {
        private readonly IPizzaRepository _pizzaRepository;
        private readonly IFileStorageService _fileStorageService;

        public PizzaService(IPizzaRepository pizzaRepository, IFileStorageService fileStorageService)
        {
            _pizzaRepository = pizzaRepository;
            _fileStorageService = fileStorageService;
        }

        public async Task<IEnumerable<Pizza>> GetAllAsync()
        {
            var pizzas = await _pizzaRepository.GetAllAsync();
            foreach (var pizza in pizzas)
            {
                pizza.PhotoPath = _fileStorageService.GetFullUrl(pizza.PhotoPath);
            }
            return pizzas.OrderBy(o => o.Id);
        }

        public async Task<Pizza?> GetByIdAsync(int id)
        {
            var pizza = await _pizzaRepository.GetByIdAsync(id);
            if (pizza != null)
            {
                pizza.PhotoPath = _fileStorageService.GetFullUrl(pizza.PhotoPath);
            }
            return pizza;
        }

        public async Task<Pizza> CreateAsync(PizzaDto pizzaDto, IFormFile? image)
        {
            ValidatePizzaDto(pizzaDto);

            var pizza = new Pizza
            {
                Name = pizzaDto.Name,
                Description = new Description
                {
                    Text = pizzaDto.Description.Text,
                    Ingredients = pizzaDto.Description.Ingredients ?? new List<string>(),
                    Weight = pizzaDto.Description.Weight
                },
                Price = pizzaDto.Price,
                IsVegetarian = pizzaDto.IsVegetarian
            };

            // 1. Сохраняем пиццу без изображения
            var createdPizza = await _pizzaRepository.AddAsync(pizza);

            // 2. Если есть изображение — сохраняем по правильному пути
            if (image != null)
            {
                var photoPath = await _fileStorageService.SavePizzaImageAsync(createdPizza.Id, image);
                createdPizza.PhotoPath = photoPath;

                // 3. Обновляем пиццу с путём
                await _pizzaRepository.UpdateAsync(createdPizza);
            }

            // 4. Преобразуем в абсолютный URL
            if (!string.IsNullOrEmpty(createdPizza.PhotoPath))
            {
                createdPizza.PhotoPath = _fileStorageService.GetFullUrl(createdPizza.PhotoPath);
            }

            return createdPizza;
        }


        public async Task<Pizza> UpdateAsync(int id, PizzaDto pizzaDto, IFormFile? image)
        {
            ValidatePizzaDto(pizzaDto);

            var pizza = await _pizzaRepository.GetByIdAsync(id)
                ?? throw new ValidationException($"Пицца с ID {id} не найдена.");

            pizza.Name = pizzaDto.Name;
            pizza.Description.Text = pizzaDto.Description.Text;
            pizza.Description.Ingredients = pizzaDto.Description.Ingredients ?? new List<string>();
            pizza.Description.Weight = pizzaDto.Description.Weight;
            pizza.Price = pizzaDto.Price;
            pizza.IsVegetarian = pizzaDto.IsVegetarian;

            if (image != null)
            {
                // Удаляем старую фотку, если была
                if (!string.IsNullOrEmpty(pizza.PhotoPath))
                {
                    _fileStorageService.DeletePizzaImageFolder(id);
                }

                pizza.PhotoPath = await _fileStorageService.SavePizzaImageAsync(id, image);
            }

            await _pizzaRepository.UpdateAsync(pizza);

            if (!string.IsNullOrEmpty(pizza.PhotoPath))
            {
                pizza.PhotoPath = _fileStorageService.GetFullUrl(pizza.PhotoPath);
            }

            return pizza;

        }

        public async Task DeleteAsync(int id)
        {
            var pizza = await _pizzaRepository.GetByIdAsync(id)
                ?? throw new ValidationException($"Пицца с ID {id} не найдена.");

            _fileStorageService.DeletePizzaImageFolder(id);
            await _pizzaRepository.DeleteAsync(id);
        }

        private static void ValidatePizzaDto(PizzaDto pizzaDto)
        {
            var context = new ValidationContext(pizzaDto);
            Validator.ValidateObject(pizzaDto, context, true);

            if (pizzaDto.Description.Ingredients?.Any(string.IsNullOrWhiteSpace) == true)
            {
                throw new ValidationException("Ингредиенты не должны быть пустыми или содержать только пробелы.");
            }
        }
    }
}
