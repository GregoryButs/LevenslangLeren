using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using StartTestApi.Data;
using StartTestApi.Data.Repositories;
using StartTestApi.DTO;
using StartTestApi.Profiles;
using StartTestApi.Services;
using System.Text.Json.Serialization;

namespace StartTestApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddAuthorization();

            // profiles voor automapper laden
            builder.Services.AddAutoMapper(
                _ => { },
                (typeof(EventProfile).Assembly),
                (typeof(InschrijvingProfile).Assembly));

            // voorkom problemen met circular references bij het serialiseren van JSON (reference loops)
            builder.Services.ConfigureHttpJsonOptions(options =>
            {
                options.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
            });

            // connection string ophalen uit appsettings.json en DbContext registreren
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString));

            // repositories en services registreren voor dependency injection
            builder.Services.AddScoped<IEventRepository, EventRepository>();
            builder.Services.AddScoped<IInschrijvingRepository, InschrijvingRepository>();
            builder.Services.AddScoped<IEventService, EventService>();
            builder.Services.AddScoped<IInschrijvingService, InschrijvingService>();

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();
            app.UseRouting();

            /* Schrijf hier je endpoints code */

            // entity van event gebruiken
            app.MapGet("/api/event/all", (IEventRepository eventRepository) =>
            {
                var events = eventRepository.GetAllWithInschrijvingen();
                return Results.Ok(events);
            });

            app.MapGet("/api/event/{id}", (int id, IEventRepository eventRepository) =>
            {
                var ev = eventRepository.GetByIdWithInschrijvingen(id);
                if (ev == null)
                {
                    return Results.NotFound();
                }
                return Results.Ok(ev);
            });

            app.MapPatch("/api/event/{id}/cancel", (int id, IEventRepository eventRepository) =>
            {
                var success = eventRepository.Cancel(id);
                if (!success)
                {
                    return Results.NotFound();
                }
                return Results.NoContent();
            });

            // Dto's voor events en inschrijvingen gebruiken
            app.MapGet("/api/eventDTO/all", (IEventService eventService) =>
            {
                var evDto = eventService.GetAllEventsWithInschrijvingen();
                return Results.Ok(evDto);
            });

            app.MapGet("/api/eventDTO/{id}", (int id, IEventService eventService) =>
            {
                var evDto = eventService.GetEventByIdWithInschrijvingen(id);
                if (evDto == null)
                {
                    return Results.NotFound();
                }
                return Results.Ok(evDto);
            });

            app.MapGet("/api/inschrijving/{id}", (int id, IInschrijvingService inschrijvingService) =>
            {
                var inDto = inschrijvingService.GetInschrijvingById(id);
                if (inDto == null)
                {
                    return Results.NotFound();
                }
                return Results.Ok(inDto);
            });

            app.MapPost("/api/inschrijving", (CreateInschrijvingDTO dto, IInschrijvingService inschrijvingService, IEventRepository eventRepository) =>
            {
                // wat hulp van AI gezocht omdat ik toch een soort van Modelstate.IsValid wilde hebben voor de dto Post, maar dat is niet automatisch beschikbaar in een minimal API endpoint zoals deze
                // omdat we buiten controllers werken, moeten we zelf validatie doen op de binnenkomende data (dto)
                // we maken een dictionary aan om eventuele validatiefouten in op te slaan, waarbij de key de naam van het veld is en de value een array van foutmeldingen voor dat veld
                var errors = new Dictionary<string, string[]>();

                // validatie voor deelnemerNaam: verplicht en minstens 2 tekens
                if (string.IsNullOrWhiteSpace(dto.DeelnemerNaam) || dto.DeelnemerNaam.Trim().Length < 2)
                {
                    errors["deelnemerNaam"] = new[] { "DeelnemerNaam is verplicht en moet minstens 2 tekens bevatten." };
                }

                // validatie voor deelnemerEmail: verplicht en geldig e-mailadresformaat
                if (string.IsNullOrWhiteSpace(dto.DeelnemerEmail))
                {
                    errors["deelnemerEmail"] = new[] { "DeelnemerEmail is verplicht." };
                }
                else
                {
                    // eenvoudige regex voor e-mailvalidatie, controleert op aanwezigheid van '@' en een domein
                    var isEmailValid = System.Text.RegularExpressions.Regex.IsMatch(
                        dto.DeelnemerEmail,
                        @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
                        System.Text.RegularExpressions.RegexOptions.IgnoreCase);

                    // als het e-mailadres niet voldoet aan het patroon, voegen we een foutmelding toe aan de errors dictionary
                    if (!isEmailValid)
                    {
                        errors["deelnemerEmail"] = new[] { "DeelnemerEmail heeft geen geldig formaat." };
                    }
                }

                // validatie voor eventId: verplicht en groter dan 0
                if (dto.EventId <= 0)
                {
                    errors["eventId"] = new[] { "EventId moet groter zijn dan 0." };
                }

                // als er validatiefouten zijn, retourneren we een 400 Bad Request met de details van de fouten
                if (errors.Count > 0)
                {
                    return Results.ValidationProblem(errors);
                }

                // controleer of het eventId in de DTO verwijst naar een bestaand event in de database, daarmee dat de IEventRepository nodig is in deze endpoint
                var eventExists = eventRepository.GetById(dto.EventId) is not null;
                if (!eventExists)
                {
                    return Results.BadRequest(new { message = $"Event with id {dto.EventId} does not exist." });
                }

                // als alle validaties slagen, kunnen we de inschrijving aanmaken via de service
                inschrijvingService.CreateInschrijving(dto);
                return Results.NoContent();
            });

            app.MapDelete("/api/inschrijving/{id}", (int id, IInschrijvingService inschrijvingService) =>
            {
                var inDto = inschrijvingService.GetInschrijvingById(id);
                if (inDto == null)
                {
                    return Results.NotFound();
                }
                inschrijvingService.DeleteInschrijving(id);
                return Results.NoContent();
            });

            app.Run();
        }

    }
}


