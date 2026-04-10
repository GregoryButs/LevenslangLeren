using Microsoft.EntityFrameworkCore;
using StartTestApi.Data.Entities;
using System;

namespace StartTestApi.Data
{
    public class ApplicationDbContext: DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }
        public DbSet<Entities.Event> Events { get; set; }
        public DbSet<Entities.Inschrijving> Inschrijvingen { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Inschrijving>()
                .HasOne(x => x.Event)
                .WithMany(x => x.Inschrijvingen)
                .IsRequired()
                .OnDelete(DeleteBehavior.NoAction);
            modelBuilder.Entity<Event>()
                .HasMany(x => x.Inschrijvingen)
                .WithOne(x => x.Event)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Event>().HasData(
                new Event
                {
                    Id = 1,
                    Titel = "Assertiviteitstraining voor beginners",
                    Beschrijving = "Leer grenzen aangeven en zelfzeker communiceren.",
                    StartDatumTijd = new DateTime(2026, 3, 10, 18, 00, 00),
                    EindDatumTijd = new DateTime(2026, 3, 10, 20, 00, 00),
                    IsGeannuleerd = false,

                },
                new Event
                {
                    Id = 2,
                    Titel = "Avond-Yoga voor ontspanning",
                    Beschrijving = "Rustige yogasessie om de dag af te sluiten.",
                    StartDatumTijd = new DateTime(2026, 3, 12, 19, 30, 00),
                    EindDatumTijd = new DateTime(2026, 3, 12, 21, 00, 00),
                    IsGeannuleerd = false,

                }
              );

            modelBuilder.Entity<Inschrijving>().HasData(
                new Inschrijving
                {
                    Id = 1,
                    EventId = 1,
                    DeelnemerNaam = "Jan Jansen",
                    DeelnemerEmail = "jan@jansen.com",
                    InschrijfDatumTijd = new DateTime(2026, 2, 20, 14, 15, 00)
                },
                new Inschrijving
                {
                    Id = 2,
                    EventId = 1,
                    DeelnemerNaam = "Piet Pietersen",
                    DeelnemerEmail = "piet@gmail.com",
                    InschrijfDatumTijd = new DateTime(2026, 2, 22, 10, 30, 00)

                }

            );
        }

          
    }
}
