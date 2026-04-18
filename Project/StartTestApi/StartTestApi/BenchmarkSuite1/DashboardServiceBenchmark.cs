using AfsprakenbeheerPsycholoog.Data;
using AfsprakenbeheerPsycholoog.Data.Entities;
using AfsprakenbeheerPsycholoog.Data.Repositories;
using AfsprakenbeheerPsycholoog.Models.ViewModels.Afspraak;
using AfsprakenbeheerPsycholoog.Profiles;
using AfsprakenbeheerPsycholoog.Services;
using AutoMapper;
using BenchmarkDotNet.Attributes;
using Microsoft.EntityFrameworkCore;
using Microsoft.VSDiagnostics;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AfsprakenbeheerPsycholoog.Benchmarks;
[CPUUsageDiagnoser]
public class DashboardServiceBenchmark
{
    private ApplicationDbContext _context = null !;
    private DashboardService _service = null !;
    [GlobalSetup]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlite("Data Source=:memory:").Options;
        _context = new ApplicationDbContext(options);
        _context.Database.OpenConnection();
        _context.Database.EnsureCreated();
        SeedBenchmarkData(_context);
        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<AfspraakProfile>();
            cfg.AddProfile<PatientProfile>();
        }, NullLoggerFactory.Instance);
        var mapper = mapperConfig.CreateMapper();
        var afspraakRepo = new AfspraakRepository(_context);
        _service = new DashboardService(afspraakRepo, mapper);
    }

    [Benchmark]
    public object GetDashboard()
    {
        return _service.GetDashboard("Psycholoog", DateTime.Today);
    }

    private static void SeedBenchmarkData(ApplicationDbContext context)
    {
        if (!context.AfspraakTypes.Any())
        {
            context.AfspraakTypes.AddRange(new AfspraakType { Naam = "Intake", StandaardDuurMinuten = 60, Kleurcode = "#4A90D9" }, new AfspraakType { Naam = "Therapie", StandaardDuurMinuten = 45, Kleurcode = "#7ED321" });
            context.SaveChanges();
        }

        if (!context.Patienten.Any())
        {
            var patienten = Enumerable.Range(1, 200).Select(i => new Patient { Voornaam = $"Voornaam{i}", Achternaam = $"Achternaam{i}", Geboortedatum = new DateOnly(1980, 1, 1), Email = $"patient{i}@test.local", Telefoonnummer = $"0471{i:000000}", IsActief = true }).ToList();
            context.Patienten.AddRange(patienten);
            context.SaveChanges();
            var typeIds = context.AfspraakTypes.Select(t => t.Id).ToList();
            var patientIds = context.Patienten.Select(p => p.Id).ToList();
            var start = DateTime.Today.AddDays(-60);
            var afspraken = new List<Afspraak>(4000);
            for (int i = 0; i < 4000; i++)
            {
                var s = start.AddMinutes(i * 30);
                afspraken.Add(new Afspraak { PatientId = patientIds[i % patientIds.Count], TypeId = typeIds[i % typeIds.Count], Starttijd = s, Eindtijd = s.AddMinutes(45), Status = i % 7 == 0 ? AfspraakStatus.Geannuleerd : AfspraakStatus.Gepland });
            }

            context.Afspraken.AddRange(afspraken);
            context.SaveChanges();
        }
    }
}