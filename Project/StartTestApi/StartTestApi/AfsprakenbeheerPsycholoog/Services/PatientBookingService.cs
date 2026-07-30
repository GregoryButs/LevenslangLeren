using AfsprakenbeheerPsycholoog.Data;
using AfsprakenbeheerPsycholoog.Data.Entities;
using AfsprakenbeheerPsycholoog.Data.Repositories;
using AfsprakenbeheerPsycholoog.Helpers;
using AfsprakenbeheerPsycholoog.Models.ViewModels.Afspraak;
using AfsprakenbeheerPsycholoog.Models.ViewModels.PatientPortaal;
using AfsprakenbeheerPsycholoog.Models.ViewModels.Planning;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace AfsprakenbeheerPsycholoog.Services
{
    public class PatientBookingService : IPatientBookingService
    {
        private readonly IAfspraakRepository _afspraakRepo;
        private readonly IAfspraakTypeRepository _typeRepo;
        private readonly IPatientRepository _patientRepo;
        private readonly IMapper _mapper;
        private readonly IGoogleCalendarService _calendarService;
        private readonly IEmailService _emailService;
        private readonly ApplicationDbContext _dbContext;
        private readonly CalendarSyncQueue _syncQueue;

        public PatientBookingService(
            IAfspraakRepository afspraakRepo,
            IAfspraakTypeRepository typeRepo,
            IPatientRepository patientRepo,
            IMapper mapper,
            IGoogleCalendarService calendarService,
            IEmailService emailService,
            ApplicationDbContext dbContext,
            CalendarSyncQueue syncQueue)
        {
            _afspraakRepo = afspraakRepo;
            _typeRepo = typeRepo;
            _patientRepo = patientRepo;
            _mapper = mapper;
            _calendarService = calendarService;
            _emailService = emailService;
            _dbContext = dbContext;
            _syncQueue = syncQueue;
        }

        public PatientBoekAfspraakViewModel GetBoekViewModel(DateTime datum)
        {
            return new PatientBoekAfspraakViewModel
            {
                Datum = datum
            };
        }

        public async Task<DagOverzichtViewModel> GetDagOverzichtVoorPatientAsync(DateTime datum)
        {
            var instelling = _dbContext.PraktijkInstellingen.FirstOrDefault(i => i.Id == 1) 
                ?? new PraktijkInstelling();

            var tijdsloten = await BuildTijdslotenVoorDatumAsync(datum, instelling);

            var vm = new DagOverzichtViewModel
            {
                Datum = datum.Date,
                Tijdsloten = tijdsloten
            };

            // Zoek de eerstvolgende beschikbare afspraak voor de patient zonder recursie
            try
            {
                var vandaag = DateTime.Today.AddDays(1);
                for (int i = 0; i < 30; i++)
                {
                    var testDatum = vandaag.AddDays(i);
                    List<TijdslotViewModel> testSloten;
                    if (testDatum.Date == datum.Date)
                    {
                        testSloten = tijdsloten;
                    }
                    else
                    {
                        testSloten = await BuildTijdslotenVoorDatumAsync(testDatum, instelling);
                    }

                    var vrijSlot = testSloten?.FirstOrDefault(s => !s.IsBezet);
                    if (vrijSlot != null)
                    {
                        vm.EerstVolgendeVrijeSlotTijd = vrijSlot.Tijd;
                        vm.EerstVolgendeVrijeSlotDatumStr = testDatum.ToString("yyyy-MM-dd");
                        break;
                    }
                }
            }
            catch { }

            return vm;
        }

        private async Task<List<TijdslotViewModel>> BuildTijdslotenVoorDatumAsync(DateTime datum, PraktijkInstelling instelling)
        {
            TimeZoneInfo tz;
            try
            {
                tz = TimeZoneInfo.FindSystemTimeZoneById("Europe/Amsterdam");
            }
            catch
            {
                tz = TimeZoneInfo.Local;
            }
            var dayOfWeek = datum.DayOfWeek;
            
            bool isActive1 = false;
            string startStr1 = "09:00";
            string endStr1 = "12:00";

            bool isActive2 = false;
            string startStr2 = "13:00";
            string endStr2 = "17:00";

            switch (dayOfWeek)
            {
                case DayOfWeek.Monday:
                    isActive1 = instelling.MaandagActief;
                    startStr1 = instelling.MaandagStart;
                    endStr1 = instelling.MaandagEinde;
                    isActive2 = instelling.Maandag2Actief;
                    startStr2 = instelling.MaandagStart2;
                    endStr2 = instelling.MaandagEinde2;
                    break;
                case DayOfWeek.Tuesday:
                    isActive1 = instelling.DinsdagActief;
                    startStr1 = instelling.DinsdagStart;
                    endStr1 = instelling.DinsdagEinde;
                    isActive2 = instelling.Dinsdag2Actief;
                    startStr2 = instelling.DinsdagStart2;
                    endStr2 = instelling.DinsdagEinde2;
                    break;
                case DayOfWeek.Wednesday:
                    isActive1 = instelling.WoensdagActief;
                    startStr1 = instelling.WoensdagStart;
                    endStr1 = instelling.WoensdagEinde;
                    isActive2 = instelling.Woensdag2Actief;
                    startStr2 = instelling.WoensdagStart2;
                    endStr2 = instelling.WoensdagEinde2;
                    break;
                case DayOfWeek.Thursday:
                    isActive1 = instelling.DonderdagActief;
                    startStr1 = instelling.DonderdagStart;
                    endStr1 = instelling.DonderdagEinde;
                    isActive2 = instelling.Donderdag2Actief;
                    startStr2 = instelling.DonderdagStart2;
                    endStr2 = instelling.DonderdagEinde2;
                    break;
                case DayOfWeek.Friday:
                    isActive1 = instelling.VrijdagActief;
                    startStr1 = instelling.VrijdagStart;
                    endStr1 = instelling.VrijdagEinde;
                    isActive2 = instelling.Vrijdag2Actief;
                    startStr2 = instelling.VrijdagStart2;
                    endStr2 = instelling.VrijdagEinde2;
                    break;
                case DayOfWeek.Saturday:
                    isActive1 = instelling.ZaterdagActief;
                    startStr1 = instelling.ZaterdagStart;
                    endStr1 = instelling.ZaterdagEinde;
                    isActive2 = instelling.Zaterdag2Actief;
                    startStr2 = instelling.ZaterdagStart2;
                    endStr2 = instelling.ZaterdagEinde2;
                    break;
                case DayOfWeek.Sunday:
                    isActive1 = instelling.ZondagActief;
                    startStr1 = instelling.ZondagStart;
                    endStr1 = instelling.ZondagEinde;
                    isActive2 = instelling.Zondag2Actief;
                    startStr2 = instelling.ZondagStart2;
                    endStr2 = instelling.ZondagEinde2;
                    break;
            }

            var tijdsloten = new List<TijdslotViewModel>();
            if (!isActive1 && !isActive2) return tijdsloten;

            var busySlots = new List<(DateTime Start, DateTime End)>();
            var dayStartLocal = datum.Date;
            var dayEndLocal = datum.Date.AddDays(1).AddTicks(-1);
            var dayStartUtc = TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(dayStartLocal, DateTimeKind.Unspecified), tz);
            var dayEndUtc = TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(dayEndLocal, DateTimeKind.Unspecified), tz);

            busySlots = await _calendarService.GetBusySlotsAsync(dayStartUtc, dayEndUtc);

            var slotDuur = instelling.SlotDuurMinuten;
            var buffer = instelling.BufferMinuten;

            if (isActive1)
            {
                var startParts = startStr1.Split(':');
                var endParts = endStr1.Split(':');
                var startTime = new TimeSpan(int.Parse(startParts[0]), int.Parse(startParts[1]), 0);
                var endTime = new TimeSpan(int.Parse(endParts[0]), int.Parse(endParts[1]), 0);

                var startLocal = datum.Date + startTime;
                var endLocal = datum.Date + endTime;
                var currentLocal = startLocal;

                while (currentLocal.AddMinutes(slotDuur) <= endLocal)
                {
                    var slotEndLocal = currentLocal.AddMinutes(slotDuur);
                    var slotStartUtc = TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(currentLocal, DateTimeKind.Unspecified), tz);
                    var slotEndUtc = TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(slotEndLocal, DateTimeKind.Unspecified), tz);

                    bool isBusy = busySlots.Any(b => b.Start < slotEndUtc && b.End > slotStartUtc);

                    tijdsloten.Add(new TijdslotViewModel
                    {
                        Tijd = currentLocal,
                        Starttijd = currentLocal.TimeOfDay,
                        Eindtijd = slotEndLocal.TimeOfDay,
                        IsBezet = isBusy,
                        Afspraak = null
                    });

                    currentLocal = currentLocal.AddMinutes(slotDuur + buffer);
                }
            }

            if (isActive2)
            {
                var startParts = startStr2.Split(':');
                var endParts = endStr2.Split(':');
                var startTime = new TimeSpan(int.Parse(startParts[0]), int.Parse(startParts[1]), 0);
                var endTime = new TimeSpan(int.Parse(endParts[0]), int.Parse(endParts[1]), 0);

                var startLocal = datum.Date + startTime;
                var endLocal = datum.Date + endTime;
                var currentLocal = startLocal;

                while (currentLocal.AddMinutes(slotDuur) <= endLocal)
                {
                    var slotEndLocal = currentLocal.AddMinutes(slotDuur);
                    var slotStartUtc = TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(currentLocal, DateTimeKind.Unspecified), tz);
                    var slotEndUtc = TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(slotEndLocal, DateTimeKind.Unspecified), tz);

                    bool isBusy = busySlots.Any(b => b.Start < slotEndUtc && b.End > slotStartUtc);

                    tijdsloten.Add(new TijdslotViewModel
                    {
                        Tijd = currentLocal,
                        Starttijd = currentLocal.TimeOfDay,
                        Eindtijd = slotEndLocal.TimeOfDay,
                        IsBezet = isBusy,
                        Afspraak = null
                    });

                    currentLocal = currentLocal.AddMinutes(slotDuur + buffer);
                }
            }

            return tijdsloten;
        }

        public async Task<bool> CreatePatientAfspraakAsync(PatientBoekAfspraakViewModel vm, int patientId)
        {
            if (!vm.GekozeTijdslot.HasValue) return false;

            var starttijd = vm.GekozeTijdslot.Value;
            
            var instelling = _dbContext.PraktijkInstellingen.FirstOrDefault(i => i.Id == 1) 
                ?? new PraktijkInstelling();

            // Planning window controleren
            var nuLocal = DateTime.Now;
            if (starttijd < nuLocal.AddHours(instelling.MinimaalVoorafUren)) return false;
            if (starttijd > nuLocal.Date.AddDays(instelling.MaximaleToekomstDagen)) return false;

            var standaardType = GetStandaardTypeVoorPatientBoeking();
            if (standaardType == null) return false;

            var slotDuur = instelling.SlotDuurMinuten;
            var eindtijd = starttijd.AddMinutes(slotDuur);

            // Timezone conversie
            TimeZoneInfo tz;
            try
            {
                tz = TimeZoneInfo.FindSystemTimeZoneById("Europe/Amsterdam");
            }
            catch
            {
                tz = TimeZoneInfo.Local;
            }

            // Valideren of het gekozen tijdstip binnen de geconfigureerde werkuren valt
            var dayOfWeek = starttijd.DayOfWeek;
            bool isActive1 = false;
            string startStr1 = "09:00";
            string endStr1 = "12:00";

            bool isActive2 = false;
            string startStr2 = "13:00";
            string endStr2 = "17:00";

            switch (dayOfWeek)
            {
                case DayOfWeek.Monday:
                    isActive1 = instelling.MaandagActief;
                    startStr1 = instelling.MaandagStart;
                    endStr1 = instelling.MaandagEinde;
                    isActive2 = instelling.Maandag2Actief;
                    startStr2 = instelling.MaandagStart2;
                    endStr2 = instelling.MaandagEinde2;
                    break;
                case DayOfWeek.Tuesday:
                    isActive1 = instelling.DinsdagActief;
                    startStr1 = instelling.DinsdagStart;
                    endStr1 = instelling.DinsdagEinde;
                    isActive2 = instelling.Dinsdag2Actief;
                    startStr2 = instelling.DinsdagStart2;
                    endStr2 = instelling.DinsdagEinde2;
                    break;
                case DayOfWeek.Wednesday:
                    isActive1 = instelling.WoensdagActief;
                    startStr1 = instelling.WoensdagStart;
                    endStr1 = instelling.WoensdagEinde;
                    isActive2 = instelling.Woensdag2Actief;
                    startStr2 = instelling.WoensdagStart2;
                    endStr2 = instelling.WoensdagEinde2;
                    break;
                case DayOfWeek.Thursday:
                    isActive1 = instelling.DonderdagActief;
                    startStr1 = instelling.DonderdagStart;
                    endStr1 = instelling.DonderdagEinde;
                    isActive2 = instelling.Donderdag2Actief;
                    startStr2 = instelling.DonderdagStart2;
                    endStr2 = instelling.DonderdagEinde2;
                    break;
                case DayOfWeek.Friday:
                    isActive1 = instelling.VrijdagActief;
                    startStr1 = instelling.VrijdagStart;
                    endStr1 = instelling.VrijdagEinde;
                    isActive2 = instelling.Vrijdag2Actief;
                    startStr2 = instelling.VrijdagStart2;
                    endStr2 = instelling.VrijdagEinde2;
                    break;
                case DayOfWeek.Saturday:
                    isActive1 = instelling.ZaterdagActief;
                    startStr1 = instelling.ZaterdagStart;
                    endStr1 = instelling.ZaterdagEinde;
                    isActive2 = instelling.Zaterdag2Actief;
                    startStr2 = instelling.ZaterdagStart2;
                    endStr2 = instelling.ZaterdagEinde2;
                    break;
                case DayOfWeek.Sunday:
                    isActive1 = instelling.ZondagActief;
                    startStr1 = instelling.ZondagStart;
                    endStr1 = instelling.ZondagEinde;
                    isActive2 = instelling.Zondag2Actief;
                    startStr2 = instelling.ZondagStart2;
                    endStr2 = instelling.ZondagEinde2;
                    break;
            }

            bool isBinnenInterval1 = false;
            if (isActive1)
            {
                var startParts = startStr1.Split(':');
                var endParts = endStr1.Split(':');
                var startTime = new TimeSpan(int.Parse(startParts[0]), int.Parse(startParts[1]), 0);
                var endTime = new TimeSpan(int.Parse(endParts[0]), int.Parse(endParts[1]), 0);

                var startLocal1 = starttijd.Date + startTime;
                var endLocal1 = starttijd.Date + endTime;

                if (starttijd >= startLocal1 && starttijd.AddMinutes(slotDuur) <= endLocal1)
                {
                    isBinnenInterval1 = true;
                }
            }

            bool isBinnenInterval2 = false;
            if (isActive2)
            {
                var startParts = startStr2.Split(':');
                var endParts = endStr2.Split(':');
                var startTime = new TimeSpan(int.Parse(startParts[0]), int.Parse(startParts[1]), 0);
                var endTime = new TimeSpan(int.Parse(endParts[0]), int.Parse(endParts[1]), 0);

                var startLocal2 = starttijd.Date + startTime;
                var endLocal2 = starttijd.Date + endTime;

                if (starttijd >= startLocal2 && starttijd.AddMinutes(slotDuur) <= endLocal2)
                {
                    isBinnenInterval2 = true;
                }
            }

            if (!isBinnenInterval1 && !isBinnenInterval2)
            {
                return false; // Buiten de geconfigureerde uren/tijdens pauze
            }

            var startUtc = TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(starttijd, DateTimeKind.Unspecified), tz);
            var endUtc = TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(eindtijd, DateTimeKind.Unspecified), tz);

            using var transaction = await _dbContext.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);
            try
            {
                // Double check local database availability first (within the transaction)
                if (_afspraakRepo.HeeftConflict(startUtc, endUtc))
                {
                    await transaction.RollbackAsync();
                    return false; // Conflict!
                }

                // Double check Google Calendar availability
                var busySlots = await _calendarService.GetBusySlotsAsync(startUtc, endUtc);
                if (busySlots.Any(b => b.Start < endUtc && b.End > startUtc))
                {
                    await transaction.RollbackAsync();
                    return false; // Conflict!
                }

                // Google Meet and Location details
                bool createMeetLink = string.Equals(vm.LocatieType, "GoogleMeet", StringComparison.OrdinalIgnoreCase);
                string locationText = vm.LocatieType switch
                {
                    "Praktijk" => "Op de praktijk (in-person)",
                    "GoogleMeet" => "Online via Google Meet",
                    "Telefoon" => "Telefonisch consult",
                    _ => "Niet gespecificeerd"
                };

                var opmerkingenMetLocatie = $"Gekozen Locatie: {locationText}\n" + (vm.Opmerkingen ?? "");

                var afspraak = new Afspraak
                {
                    PatientId = patientId,
                    TypeId = standaardType.Id,
                    Starttijd = startUtc,
                    Eindtijd = endUtc,
                    Status = AfspraakStatus.Gepland,
                    Opmerkingen = opmerkingenMetLocatie
                };

                _afspraakRepo.Add(afspraak);
                _afspraakRepo.SaveChanges();

                await transaction.CommitAsync();

                // Queue the sync & notification task to run asynchronously in the background
                await _syncQueue.QueueBackgroundWorkItemAsync(new CalendarSyncTask
                {
                    AfspraakId = afspraak.Id,
                    PatientId = patientId,
                    Action = SyncAction.Create,
                    CreateMeetLink = createMeetLink,
                    LocationText = locationText
                });

                return true;
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<bool> AnnuleerPatientAfspraakAsync(int afspraakId, int patientId)
        {
            var afspraak = _afspraakRepo.GetByIdEnPatient(afspraakId, patientId);
            if (afspraak == null || afspraak.Status != AfspraakStatus.Gepland) return false;

            // Annuleringstermijn controleren (bijv. 24 uur)
            if (afspraak.Starttijd < DateTime.UtcNow.AddHours(24)) return false;

            afspraak.Status = AfspraakStatus.Geannuleerd;
            _afspraakRepo.Update(afspraak);
            _afspraakRepo.SaveChanges();

            // Queue cancellation and notification to background worker
            await _syncQueue.QueueBackgroundWorkItemAsync(new CalendarSyncTask
            {
                AfspraakId = afspraak.Id,
                PatientId = patientId,
                Action = SyncAction.Cancel
            });

            return true;
        }

        private AfspraakType? GetStandaardTypeVoorPatientBoeking()
        {
            var types = _typeRepo.GetAll();
            return types.FirstOrDefault(t => string.Equals(t.Naam.Trim(), "Therapie", StringComparison.OrdinalIgnoreCase)) 
                   ?? types.FirstOrDefault(t => t.Naam.Contains("Therapie", StringComparison.OrdinalIgnoreCase) && !t.Naam.Contains("Praktijkhuis", StringComparison.OrdinalIgnoreCase))
                   ?? types.FirstOrDefault(t => t.VereistPatient);
        }
    }
}
