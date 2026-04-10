using AfsprakenbeheerPsycholoog.Data.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AfsprakenbeheerPsycholoog.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public DbSet<Patient> Patienten { get; set; }
        public DbSet<AfspraakType> AfspraakTypes { get; set; }
        public DbSet<Afspraak> Afspraken { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder); // ALTIJD voor Identity

            // Relatie: Afspraak → Patient
            builder.Entity<Afspraak>()
                .HasOne(a => a.Patient)
                .WithMany(p => p.Afspraken)
                .HasForeignKey(a => a.PatientId)
                .OnDelete(DeleteBehavior.NoAction);

            // Relatie: Afspraak → AfspraakType
            builder.Entity<Afspraak>()
                .HasOne(a => a.Type)
                .WithMany(t => t.Afspraken)
                .HasForeignKey(a => a.TypeId)
                .OnDelete(DeleteBehavior.NoAction);

            // --- SEEDING ---
            builder.Entity<AfspraakType>().HasData(
                new AfspraakType { Id = 1, Naam = "Intake", StandaardDuurMinuten = 90, Kleurcode = "#4A90D9" },
                new AfspraakType { Id = 2, Naam = "Therapie", StandaardDuurMinuten = 60, Kleurcode = "#7ED321" },
                new AfspraakType { Id = 3, Naam = "Evaluatie", StandaardDuurMinuten = 45, Kleurcode = "#F5A623" },
                new AfspraakType { Id = 4, Naam = "Crisis", StandaardDuurMinuten = 30, Kleurcode = "#D0021B" }
            );

            builder.Entity<Patient>().HasData(
                new Patient
                {
                    Id = 1,
                    Voornaam = "Jan",
                    Achternaam = "Janssens",
                    Geboortedatum = new DateOnly(1985, 3, 12),
                    Email = "jan@test.be",
                    Telefoonnummer = "0471000001",
                    DossierNummer = "DOS-001"
                },
                new Patient
                {
                    Id = 2,
                    Voornaam = "Marie",
                    Achternaam = "Peeters",
                    Geboortedatum = new DateOnly(1992, 7, 4),
                    Email = "marie@test.be",
                    Telefoonnummer = "0471000002"
                },
                new Patient
                {
                    Id = 3,
                    Voornaam = "Pieter",
                    Achternaam = "De Smedt",
                    Geboortedatum = new DateOnly(1978, 11, 20),
                    Email = "pieter@test.be",
                    Telefoonnummer = "0471000003",
                    DossierNummer = "DOS-002"
                }
            );

            builder.Entity<Afspraak>().HasData(
                new Afspraak
                {
                    Id = 1,
                    PatientId = 1,
                    TypeId = 1,
                    Starttijd = new DateTime(2026, 4, 14, 10, 0, 0),
                    Eindtijd = new DateTime(2026, 4, 14, 11, 30, 0),
                    Status = Status.Gepland
                },
                new Afspraak
                {
                    Id = 2,
                    PatientId = 2,
                    TypeId = 2,
                    Starttijd = new DateTime(2026, 4, 15, 14, 0, 0),
                    Eindtijd = new DateTime(2026, 4, 15, 15, 0, 0),
                    Status = Status.Gepland
                },
                new Afspraak
                {
                    Id = 3,
                    PatientId = 1,
                    TypeId = 3,
                    Starttijd = new DateTime(2026, 4, 1, 9, 0, 0),
                    Eindtijd = new DateTime(2026, 4, 1, 9, 45, 0),
                    Status = Status.Voltooid,
                },
                new Afspraak
                {
                    Id = 4,
                    PatientId = 3,
                    TypeId = 2,
                    Starttijd = new DateTime(2026, 3, 20, 16, 0, 0),
                    Eindtijd = new DateTime(2026, 3, 20, 17, 0, 0),
                    Status = Status.Geannuleerd,
                    Opmerkingen = "Patiënt heeft afgezegd"
                }
                );
        }
    }
}
