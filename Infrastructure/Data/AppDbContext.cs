using Microsoft.EntityFrameworkCore;
using Domain;

namespace Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        public DbSet<Pizza> Pizzas { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Pizza>()
                .OwnsOne(p => p.Description, desc =>
                {
                    desc.Property(d => d.Text);
                    desc.Property(d => d.Weight);
                    desc.Property(d => d.Ingredients)
                    .HasConversion(
                        v => string.Join(",", v),
                        v => v.Split(",", StringSplitOptions.RemoveEmptyEntries).ToList()
                        );

                });


        }



    }
}
