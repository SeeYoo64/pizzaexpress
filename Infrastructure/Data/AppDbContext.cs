using Microsoft.EntityFrameworkCore;
using Domain;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Text.Json;

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
                    desc.Property(d => d.Ingredients).HasColumnType("jsonb")
                    .HasConversion(
                        v => JsonSerializer.Serialize(v, new JsonSerializerOptions { WriteIndented = false }),
                        v => JsonSerializer.Deserialize<List<string>>(v, new JsonSerializerOptions()) ?? new List<string>(),
                        new ValueComparer<List<string>>(
                        (c1, c2) => c1.SequenceEqual(c2),
                        c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                        c => c.ToList()));

                });


            modelBuilder.Entity<Order>()
            .HasMany(o => o.Items)
            .WithOne(oi => oi.Order)
            .HasForeignKey(oi => oi.OrderId);

            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Pizza)
                .WithMany()
                .HasForeignKey(oi => oi.PizzaId);

        }



    }
}
