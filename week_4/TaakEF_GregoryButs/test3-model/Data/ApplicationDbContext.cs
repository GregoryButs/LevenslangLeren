using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using test3_model.Data.Entities;
using test3_model.Models.Viewmodels;

namespace test3_model.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions options) : base(options)
        {
        }

      

        public DbSet<Begeleider> Begeleiders { get; set; } = null!;
        public DbSet<Locatie> Locaties { get; set; } = null!;
        public DbSet<Event> Events { get; set; } = null!;



        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // --- Relaties via Fluent API ---

            // Begeleider 1 - * Event
            modelBuilder.Entity<Event>()
                .HasOne(e => e.Begeleider)
                .WithMany(b => b.Events)
                .HasForeignKey(e => e.BegeleiderId);

            // Locatie 1 - * Event
            modelBuilder.Entity<Event>()
                .HasOne(e => e.Locatie)
                .WithMany(l => l.Events)
                .HasForeignKey(e => e.LocatieId);

            // --- Seeding ---

            modelBuilder.Entity<Begeleider>().HasData(
                new Begeleider
                {
                    Id = 1,
                    Naam = "Mohammed Ali",
                    Functie = "Assertiviteitscoach",
                    UrlFoto = "https://media.newyorker.com/photos/59097775c14b3c606c108c32/master/pass/Hobbs-Ali.jpg"
                },
                new Begeleider
                {
                    Id = 2,
                    Naam = "Jani Janssens",
                    Functie = "Yogadocent",
                    UrlFoto = "https://newnatureyoga.com/wp-content/uploads/2024/05/Opleiding-Yin-Yoga-Docent-1024x865.png"
                }
            );

            modelBuilder.Entity<Locatie>().HasData(
                new Locatie
                {
                    Id = 1,
                    Naam = "Zaal Mimosa"
                },
                new Locatie
                {
                    Id = 2,
                    Naam = "Zaal Lelie"
                }
            );

            modelBuilder.Entity<Event>().HasData(
               new Event
               {
                   Id = 1,
                   Titel = "Assertiviteitstraining voor beginners",
                   Beschrijving = "Leer grenzen aangeven en zelfzeker communiceren.",
                   StartDatumTijd = new DateTime(2026, 3, 10, 18, 00, 00),
                   EindDatumTijd = new DateTime(2026, 3, 10, 20, 00, 00),
                   IsGeannuleerd = false,
                   BegeleiderId = 1,   // Mohammed Ali
                   LocatieId = 1       // Zaal Mimosa
               },
               new Event
               {
                   Id = 2,
                   Titel = "Avond-Yoga voor ontspanning",
                   Beschrijving = "Rustige yogasessie om de dag af te sluiten.",
                   StartDatumTijd = new DateTime(2026, 3, 12, 19, 30, 00),
                   EindDatumTijd = new DateTime(2026, 3, 12, 21, 00, 00),
                   IsGeannuleerd = false,
                   BegeleiderId = 2,
                   LocatieId = 2
               }
             );

        }
  
    }
}

