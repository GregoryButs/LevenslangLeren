using Microsoft.EntityFrameworkCore;

namespace ShopAPI.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }
        public DbSet<Entities.Category> Categories { get; set; }
        public DbSet<Entities.Product> Products { get; set; }
        public DbSet<Entities.Tax> Taxes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // Configure relationships and constraints if needed
            modelBuilder.Entity<Entities.Product>()
                .Property(p => p.BuyPrice)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Entities.Product>()
                .HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Entities.Product>()
                .HasOne(p => p.TaxLevel)
                .WithMany(t => t.Products)
                .HasForeignKey(p => p.TaxesLevelId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Entities.Tax>().HasData(
                new Entities.Tax { Id = 1, Name = "Standaard", TaxLevel = Entities.TaxLevel.Standaard },
                new Entities.Tax { Id = 2, Name = "Uitzonderingen en maaltijden", TaxLevel = Entities.TaxLevel.UitzonderingenEnMaaltijden },
                new Entities.Tax { Id = 3, Name = "Basisnoden", TaxLevel = Entities.TaxLevel.Basisnoden }
                );

            modelBuilder.Entity<Entities.Product>().HasData(
                new Entities.Product { Id = 1, Code = "X1", Name = "XReal One Pro", BuyPrice = 589.99m, CategoryId = 1, TaxesLevelId = 1, AmountStock = 12, Active = true },
                new Entities.Product { Id = 2, Code = "XAir", Name = "XReal Air Pro", BuyPrice = 399.99m, CategoryId = 1, TaxesLevelId = 1, AmountStock = 20, Active = false },
                new Entities.Product { Id = 3, Code = "FoodApple", Name = "Apple", BuyPrice = 1.00m, CategoryId = 2, TaxesLevelId = 2, AmountStock = 12, Active = true },
                new Entities.Product { Id = 4, Code = "DrinkWWater", Name = "Water", BuyPrice = 0.50m, CategoryId = 3, TaxesLevelId = 3, AmountStock = 12, Active = true }

                );

            modelBuilder.Entity<Entities.Category>().HasData(
                new Entities.Category { Id = 1, Name = "AI Glasses" },
                new Entities.Category { Id = 2, Name = "Food" },
                new Entities.Category { Id = 3, Name = "Drinks" }
                );
        }
    }
}
