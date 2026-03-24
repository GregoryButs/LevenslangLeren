using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Movie_Store.Models;
using MovieStore_StartHier_OK.Authorisation;

namespace MovieStore_StartHier_OK.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Genre>().HasData(
                new Genre() { Id = 1, Name = "Action" },
                new Genre() { Id = 2, Name = "Comedy" },
                new Genre() { Id = 3, Name = "Horror" },
                new Genre() { Id = 4, Name = "Drama" },
                new Genre() { Id = 5, Name = "Fantasy" }
            );

            builder.Entity<Movie>().HasData(
                new Movie() { Id = 1, Title = "Avengers: End Game", AgeRestriction = AgeRestriction.Plus12, GenreId = 1, Price = 19.99 },
                new Movie() { Id = 2, Title = "Mr Bean's Holiday", GenreId = 2, Price = 7.89 },
                new Movie() { Id = 3, Title = "The Conjuring", AgeRestriction = AgeRestriction.Plus18, GenreId = 3, Price = 24.95 },
                new Movie() { Id = 4, Title = "Titanic", GenreId = 4, Price = 14.95 },
                new Movie() { Id = 5, Title = "Harry Potter and the Philosopher's Stone", GenreId = 5, Price = 19.99 }
                );
        }

        public DbSet<Movie> Movies { get; set; }
        public DbSet<Genre> Genres { get; set; }

    }
}