using Microsoft.EntityFrameworkCore;
using MVC_Bibliotheek.Data.Entities;

namespace MVC_Bibliotheek.Data
{
    public class BibDbContext : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(@"Server=(localdb)\mssqllocaldb;Database=BibDb;Trusted_Connection=True;");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Book>()
                .HasOne(b => b.Author)
                .WithMany(a => a.Books)
                .HasForeignKey(b => b.AuthorId)
                .OnDelete(DeleteBehavior.NoAction);
            modelBuilder.Entity<Book>()
                .HasOne(b => b.Genre)
                .WithMany(g => g.Books)
                .HasForeignKey(b => b.GenreId)
                .OnDelete(DeleteBehavior.NoAction);
            modelBuilder.Entity<Genre>()
                .HasMany(g => g.Books)
                .WithOne(b => b.Genre)
                .HasForeignKey(b => b.GenreId)
                .OnDelete(DeleteBehavior.NoAction);
            modelBuilder.Entity<Author>()
                .HasMany(a => a.Books)
                .WithOne(b => b.Author)
                .HasForeignKey(b => b.AuthorId)
                .OnDelete(DeleteBehavior.NoAction);

            // seed data
            modelBuilder.Entity<Genre>().HasData(
                new Genre { GenreId = 1, Name = "Science Fiction" },
                new Genre { GenreId = 2, Name = "Fantasy" },
                new Genre { GenreId = 3, Name = "Thriller" },
                new Genre { GenreId = 4, Name = "Romantiek" },
                new Genre { GenreId = 5, Name = "Horror" },
                new Genre { GenreId = 6, Name = "Non-Fictie" },
                new Genre { GenreId = 7, Name = "Poëzie" },
                new Genre { GenreId = 8, Name = "Biografie"}
                );
        }
        public DbSet<Author> Authors { get; set; }
        public DbSet<Book> Books { get; set; }
        public DbSet<Genre> Genres { get; set; }
    }
}
