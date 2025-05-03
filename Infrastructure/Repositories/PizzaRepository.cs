using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public interface IPizzaRepository
    {
        Task<IEnumerable<Pizza>> GetAllAsync();
        Task<Pizza?> GetByIdAsync(int id);
        Task<Pizza> AddAsync(Pizza pizza);
        Task UpdateAsync(Pizza pizza);
        Task DeleteAsync(int id);
    }
    public class PizzaRepository : IPizzaRepository
    {

        private readonly AppDbContext _context;

        public PizzaRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Pizza>> GetAllAsync()
        {
            return await _context.Pizzas.ToListAsync();
        }

        public async Task<Pizza?> GetByIdAsync(int id)
        {
            return await _context.Pizzas.FindAsync(id);
        }

        public async Task<Pizza> AddAsync(Pizza pizza)
        {
            _context.Pizzas.Add(pizza);
            await _context.SaveChangesAsync();
            return pizza;
        }

        public async Task UpdateAsync(Pizza pizza)
        {
            _context.Pizzas.Update(pizza);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var pizza = await _context.Pizzas.FindAsync(id);
            if (pizza != null)
            {
                _context.Pizzas.Remove(pizza);
                await _context.SaveChangesAsync();
            }
        }

    }
}
