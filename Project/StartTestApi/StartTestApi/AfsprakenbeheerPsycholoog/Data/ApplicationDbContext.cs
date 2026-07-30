using AfsprakenbeheerPsycholoog.Authentication;
using AfsprakenbeheerPsycholoog.Data.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using AfsprakenbeheerPsycholoog.Models.ViewModels.Afspraak;
using AfsprakenbeheerPsycholoog.Models.ViewModels.Patient;

namespace AfsprakenbeheerPsycholoog.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public DbSet<Patient> Patienten { get; set; }
        public DbSet<AfspraakType> AfspraakTypes { get; set; }
        public DbSet<Afspraak> Afspraken { get; set; }
        public DbSet<PraktijkInstelling> PraktijkInstellingen { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configure UTC DateTime Converter for SQLite
            var utcConverter = new Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter<DateTime, DateTime>(
                v => v,
                v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

            var nullableUtcConverter = new Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter<DateTime?, DateTime?>(
                v => v,
                v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : (DateTime?)null);

            foreach (var entityType in builder.Model.GetEntityTypes())
            {
                foreach (var property in entityType.GetProperties())
                {
                    if (property.ClrType == typeof(DateTime))
                    {
                        property.SetValueConverter(utcConverter);
                    }
                    else if (property.ClrType == typeof(DateTime?))
                    {
                        property.SetValueConverter(nullableUtcConverter);
                    }
                }
            }

            // is actief - Soft delete configureren   
            builder.Entity<Patient>()
                .HasQueryFilter(p => p.IsActief);

            builder.Entity<Afspraak>()
                .HasQueryFilter(a => a.PatientId == null || a.Patient.IsActief);

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
                .OnDelete(DeleteBehavior.SetNull);

            // Relatie: Patient → Afspraak
            builder.Entity<Patient>()
                .HasMany(p => p.Afspraken)
                .WithOne(a => a.Patient)
                .HasForeignKey(a => a.PatientId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<Afspraak>()
                .Property(a => a.Status)
                .HasConversion<string>(); // Sla enum op als string in de database

            builder.Entity<Afspraak>()
                .HasIndex(a => a.GoogleEventId)
                .IsUnique();

            // Composite Performance Indexes
            builder.Entity<Afspraak>()
                .HasIndex(a => new { a.Starttijd, a.Status });

            builder.Entity<Afspraak>()
                .HasIndex(a => new { a.PatientId, a.Starttijd });

            builder.Entity<Patient>()
                .HasIndex(p => p.Email);

            // Relatie: ApplicationUser → Patient
            builder.Entity<ApplicationUser>()
                .HasOne(u => u.Patient)
                .WithMany()
                .HasForeignKey(u => u.PatientId)
                .OnDelete(DeleteBehavior.SetNull);

            // --- SEEDING ---
            builder.Entity<PraktijkInstelling>().HasData(
                new PraktijkInstelling
                {
                    Id = 1,
                    GoogleCalendarId = "primary",
                    
                    MaandagActief = true, MaandagStart = "09:00", MaandagEinde = "12:00",
                    Maandag2Actief = true, MaandagStart2 = "13:00", MaandagEinde2 = "17:00",
                    
                    DinsdagActief = true, DinsdagStart = "09:00", DinsdagEinde = "12:00",
                    Dinsdag2Actief = true, DinsdagStart2 = "13:00", DinsdagEinde2 = "17:00",
                    
                    WoensdagActief = true, WoensdagStart = "09:00", WoensdagEinde = "12:00",
                    Woensdag2Actief = true, WoensdagStart2 = "13:00", WoensdagEinde2 = "17:00",
                    
                    DonderdagActief = true, DonderdagStart = "09:00", DonderdagEinde = "12:00",
                    Donderdag2Actief = true, DonderdagStart2 = "13:00", DonderdagEinde2 = "17:00",
                    
                    VrijdagActief = true, VrijdagStart = "09:00", VrijdagEinde = "12:00",
                    Vrijdag2Actief = true, VrijdagStart2 = "13:00", VrijdagEinde2 = "17:00",
                    
                    ZaterdagActief = false, ZaterdagStart = "10:00", ZaterdagEinde = "12:00",
                    Zaterdag2Actief = false, ZaterdagStart2 = "13:00", ZaterdagEinde2 = "17:00",
                    
                    ZondagActief = false, ZondagStart = "10:00", ZondagEinde = "12:00",
                    Zondag2Actief = false, ZondagStart2 = "13:00", ZondagEinde2 = "17:00",
                    
                    SlotDuurMinuten = 60,
                    BufferMinuten = 15,
                    LocatiePraktijk = true,
                    LocatieGoogleMeet = true,
                    LocatieTelefoon = true,
                    MinimaalVoorafUren = 12,
                    MaximaleToekomstDagen = 30
                }
            );

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
                    DossierNummer = "DOS-001",
                    IsActief = true,
                    EmotioneleStabiliteit = 6.2,
                    VerwijderdOp = null,
                    VerwijderdReden = null
                },
                new Patient
                {
                    Id = 2,
                    Voornaam = "Marie",
                    Achternaam = "Peeters",
                    Geboortedatum = new DateOnly(1992, 7, 4),
                    Email = "marie@test.be",
                    Telefoonnummer = "0471000002",
                    IsActief = true,
                    EmotioneleStabiliteit = 4.8,
                    VerwijderdOp = null,
                    VerwijderdReden = null
                },
                new Patient
                {
                    Id = 3,
                    Voornaam = "Pieter",
                    Achternaam = "De Smedt",
                    Geboortedatum = new DateOnly(1978, 11, 20),
                    Email = "pieter@test.be",
                    Telefoonnummer = "0471000003",
                    DossierNummer = "DOS-002",
                    IsActief = true,
                    EmotioneleStabiliteit = 7.5,
                    VerwijderdOp = null,
                    VerwijderdReden = null
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
                    Status = AfspraakStatus.Gepland,
                    SentimentScore = 0.0
                },
                new Afspraak
                {
                    Id = 2,
                    PatientId = 2,
                    TypeId = 2,
                    Starttijd = new DateTime(2026, 4, 15, 14, 0, 0),
                    Eindtijd = new DateTime(2026, 4, 15, 15, 0, 0),
                    Status = AfspraakStatus.Gepland,
                    SentimentScore = 0.0
                },
                new Afspraak
                {
                    Id = 3,
                    PatientId = 1,
                    TypeId = 3,
                    Starttijd = new DateTime(2026, 4, 1, 9, 0, 0),
                    Eindtijd = new DateTime(2026, 4, 1, 9, 45, 0),
                    Status = AfspraakStatus.Voltooid,
                    SentimentScore = 0.45
                },
                new Afspraak
                {
                    Id = 4,
                    PatientId = 3,
                    TypeId = 2,
                    Starttijd = new DateTime(2026, 3, 20, 16, 0, 0),
                    Eindtijd = new DateTime(2026, 3, 20, 17, 0, 0),
                    Status = AfspraakStatus.Geannuleerd,
                    SentimentScore = -0.3,
                    Opmerkingen = "Patiënt heeft afgezegd"
                }
                );
        }
    }
}
