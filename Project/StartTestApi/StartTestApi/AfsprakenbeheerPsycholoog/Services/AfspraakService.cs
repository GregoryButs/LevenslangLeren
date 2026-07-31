using AfsprakenbeheerPsycholoog.Data.Entities;
using AfsprakenbeheerPsycholoog.Data.Repositories;
using AfsprakenbeheerPsycholoog.Helpers;
using AfsprakenbeheerPsycholoog.Models.ViewModels.Afspraak;
using AfsprakenbeheerPsycholoog.Models.ViewModels.Planning;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AfsprakenbeheerPsycholoog.Services
{
    /// <summary>
    /// Service voor het beheren van afspraken, inclusief aanmaken, bewerken, verwijderen en ophalen van afspraken op basis van verschillende criteria. 
    /// Bevat ook logica voor het verwerken van herhalingen en het genereren van dagoverzichten.
    /// </summary>
    public class AfspraakService : IAfspraakService
    {
        private readonly IAfspraakRepository _afspraakRepo;
        private readonly IPatientRepository _patientRepo;
        private readonly IAfspraakTypeRepository _typeRepo;
        private readonly IMapper _mapper;
        private readonly IGoogleCalendarService _calendarService;
        private readonly IEmailService _emailService;

        public AfspraakService(
            IAfspraakRepository afspraakRepo,
            IPatientRepository patientRepo,
            IAfspraakTypeRepository typeRepo,
            IMapper mapper,
            IGoogleCalendarService calendarService,
            IEmailService emailService)
        {
            _afspraakRepo = afspraakRepo;
            _patientRepo = patientRepo;
            _typeRepo = typeRepo;
            _mapper = mapper;
            _calendarService = calendarService;
            _emailService = emailService;
        }

        public IEnumerable<AfspraakListViewModel> GetAlleAfspraken()
        {
            var afspraken = _afspraakRepo
                .GetAllMetDetails();

            return _mapper.Map<IEnumerable<AfspraakListViewModel>>(afspraken)
                .OrderByDescending(a => a.Starttijd);
        }

        public IEnumerable<AfspraakListViewModel> GetAfsprakenVanPatient(int patientId)
        {
            var afspraken = _afspraakRepo.GetByPatientId(patientId);
            return _mapper.Map<IEnumerable<AfspraakListViewModel>>(afspraken)
                .OrderByDescending(a => a.Starttijd);
        }

        public IEnumerable<AfspraakListViewModel> GetAfsprakenOpStatus(AfspraakStatus status)
        {
            var afspraken = _afspraakRepo.GetByStatus(status);
            return _mapper.Map<IEnumerable<AfspraakListViewModel>>(afspraken)
                .OrderBy(a => a.Starttijd);
        }

        public AfspraakDetailViewModel? GetAfspraakDetail(int id)
        {
            var afspraak = _afspraakRepo.GetByIdMetDetails(id);
            if (afspraak == null) return null;

            return _mapper.Map<AfspraakDetailViewModel>(afspraak);
        }

        public CreateAfspraakViewModel GetCreateViewModel()
        {
            return new CreateAfspraakViewModel
            {
                Starttijd = DateTime.UtcNow.Date.AddDays(1).AddHours(9)
            };
        }

        public async Task<bool> CreateAfspraakAsync(CreateAfspraakViewModel vm)
        {
            var type = vm.TypeId.HasValue ? _typeRepo.GetById(vm.TypeId.Value) : null;
            if (type == null && vm.PatientId.HasValue)
            {
                type = _typeRepo.GetById(2) ?? _typeRepo.GetAll().FirstOrDefault();
            }
            if (type != null && type.VereistPatient && !vm.PatientId.HasValue) return false;

            var startmomenten = HerhalingHelper.BouwStartmomenten(vm.Starttijd, vm.Herhaling, vm.HerhaalTot);
            var aangemaakt = 0;
            Guid? reeksId = vm.Herhaling == HerhaalPatroon.Geen ? (Guid?)null : Guid.NewGuid();
            var isBlokkering = !vm.PatientId.HasValue;

            var toegevoegdeAfspraken = new List<Afspraak>();

            TimeZoneInfo tz;
            try
            {
                tz = TimeZoneInfo.FindSystemTimeZoneById("Europe/Amsterdam");
            }
            catch
            {
                tz = TimeZoneInfo.Local;
            }

            int duurMinuten = (vm.CustomDuurMinuten.HasValue && vm.CustomDuurMinuten.Value > 0)
                ? vm.CustomDuurMinuten.Value
                : (type?.StandaardDuurMinuten ?? 60);

            foreach (var start in startmomenten)
            {
                var eind = start.AddMinutes(duurMinuten);

                var startUtc = TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(start, DateTimeKind.Unspecified), tz);
                var eindUtc = TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(eind, DateTimeKind.Unspecified), tz);

                if (_afspraakRepo.HeeftConflict(startUtc, eindUtc, isBlokkering))
                {
                    continue;
                }

                var afspraak = _mapper.Map<Afspraak>(vm);
                afspraak.Starttijd = startUtc;
                afspraak.Eindtijd = eindUtc;
                afspraak.ReeksId = reeksId;

                _afspraakRepo.Add(afspraak);
                toegevoegdeAfspraken.Add(afspraak);
                aangemaakt++;
            }

            if (aangemaakt == 0) return false;

            _afspraakRepo.SaveChanges(); // Opslaan om IDs te genereren

            // Google Calendar Sync & Bevestigingsmails
            foreach (var afspraak in toegevoegdeAfspraken)
            {
                try
                {
                    var pNaam = afspraak.Patient != null ? $"{afspraak.Patient.Voornaam} {afspraak.Patient.Achternaam}".Trim() : "";
                    var googleEventId = await _calendarService.CreateEventAsync(afspraak.Starttijd, afspraak.Eindtijd, afspraak.Id, false, "", pNaam);
                    afspraak.GoogleEventId = googleEventId;
                    _afspraakRepo.Update(afspraak);
                }
                catch (Exception) { }

                if (afspraak.PatientId.HasValue)
                {
                    try
                    {
                        var patient = _patientRepo.GetById(afspraak.PatientId.Value);
                        if (patient != null && !string.IsNullOrEmpty(patient.Email))
                        {
                            await _emailService.SendConfirmationEmailAsync(
                                patient.Email,
                                patient.VolledigeNaam,
                                afspraak.Starttijd,
                                afspraak.Eindtijd,
                                type?.Naam ?? "Sessie",
                                afspraak.Id,
                                afspraak.Opmerkingen
                            );
                        }
                    }
                    catch (Exception) { }
                }
            }

            _afspraakRepo.SaveChanges(); // GoogleEventIds opslaan
            return true;
        }

        public EditAfspraakViewModel? GetEditViewModel(int id)
        {
            var afspraak = _afspraakRepo.GetById(id);
            if (afspraak == null) return null;

            return _mapper.Map<EditAfspraakViewModel>(afspraak);
        }

        public async Task<bool> EditAfspraakAsync(EditAfspraakViewModel vm)
        {
            var afspraakInDb = _afspraakRepo.GetById(vm.Id);
            if (afspraakInDb == null) return false;

            Data.Entities.AfspraakType? type = null;
            if (!vm.PatientId.HasValue)
            {
                vm.TypeId = null;
                if (string.IsNullOrWhiteSpace(vm.Opmerkingen) || vm.Opmerkingen == "Blokkering")
                {
                    if (afspraakInDb.Patient != null)
                    {
                        vm.Opmerkingen = $"{afspraakInDb.Patient.Voornaam} {afspraakInDb.Patient.Achternaam}".Trim();
                    }
                }
            }
            else
            {
                type = vm.TypeId.HasValue ? _typeRepo.GetById(vm.TypeId.Value) : null;
                if (type == null)
                {
                    type = _typeRepo.GetById(2) ?? _typeRepo.GetAll().FirstOrDefault();
                    vm.TypeId = type?.Id;
                }
            }

            var tz = TimeZoneHelper.DutchTimeZone;

            int duurMinuten = type?.StandaardDuurMinuten ?? 60;
            var startUtc = TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(vm.Starttijd, DateTimeKind.Unspecified), tz);
            var eindtijdUtc = TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(vm.Starttijd.AddMinutes(duurMinuten), DateTimeKind.Unspecified), tz);
            var isBlokkering = !vm.PatientId.HasValue;

            if (_afspraakRepo.HeeftConflict(startUtc, eindtijdUtc, isBlokkering, vm.Id))
            {
                return false;
            }

            var startTijdGewijzigd = afspraakInDb.Starttijd != startUtc;

            _mapper.Map(vm, afspraakInDb);
            afspraakInDb.Starttijd = startUtc;
            afspraakInDb.Eindtijd = eindtijdUtc;

            _afspraakRepo.Update(afspraakInDb);
            _afspraakRepo.SaveChanges();

            // Google Calendar Sync
            if (!string.IsNullOrEmpty(afspraakInDb.GoogleEventId))
            {
                try
                {
                    await _calendarService.UpdateEventAsync(afspraakInDb.GoogleEventId, afspraakInDb.Starttijd, afspraakInDb.Eindtijd, afspraakInDb.Id);
                }
                catch (Exception) { }
            }
            else
            {
                try
                {
                    var pNaam = afspraakInDb.Patient != null ? $"{afspraakInDb.Patient.Voornaam} {afspraakInDb.Patient.Achternaam}".Trim() : "";
                    var googleEventId = await _calendarService.CreateEventAsync(afspraakInDb.Starttijd, afspraakInDb.Eindtijd, afspraakInDb.Id, false, "", pNaam);
                    afspraakInDb.GoogleEventId = googleEventId;
                    _afspraakRepo.Update(afspraakInDb);
                    _afspraakRepo.SaveChanges();
                }
                catch (Exception) { }
            }

            // Stuur verzet-mail
            if (startTijdGewijzigd && afspraakInDb.PatientId.HasValue)
            {
                try
                {
                    var patient = _patientRepo.GetById(afspraakInDb.PatientId.Value);
                    if (patient != null && !string.IsNullOrEmpty(patient.Email))
                    {
                        await _emailService.SendRescheduleEmailAsync(
                            patient.Email,
                            patient.VolledigeNaam,
                            afspraakInDb.Starttijd,
                            type?.Naam ?? "Sessie"
                        );
                    }
                }
                catch (Exception) { }
            }

            return true;
        }

        public async Task DeleteAfspraakAsync(int id)
        {
            var afspraak = _afspraakRepo.GetById(id);
            if (afspraak == null) return;

            // Google Calendar sync
            if (!string.IsNullOrEmpty(afspraak.GoogleEventId))
            {
                try
                {
                    await _calendarService.DeleteEventAsync(afspraak.GoogleEventId);
                }
                catch (Exception) { }
            }

            // Mail sturen voor annulering
            if (afspraak.PatientId.HasValue && afspraak.Status == AfspraakStatus.Gepland)
            {
                try
                {
                    var patient = _patientRepo.GetById(afspraak.PatientId.Value);
                    var typeName = afspraak.Type?.Naam ?? "Sessie";
                    if (patient != null && !string.IsNullOrEmpty(patient.Email))
                    {
                        await _emailService.SendCancellationEmailAsync(
                            patient.Email,
                            patient.VolledigeNaam,
                            afspraak.Starttijd,
                            typeName
                        );
                    }
                }
                catch (Exception) { }
            }

            if (!string.IsNullOrEmpty(afspraak.GoogleEventId))
            {
                afspraak.Status = AfspraakStatus.Geannuleerd;
                _afspraakRepo.Update(afspraak);
                _afspraakRepo.SaveChanges();
            }
            else
            {
                _afspraakRepo.Delete(afspraak);
                _afspraakRepo.SaveChanges();
            }
        }

        public DagOverzichtViewModel GetDagOverzicht(DateTime datum)
        {
            var afsprakenDag = _afspraakRepo.GetByDatum(datum);
            var afsprakenVms = _mapper.Map<IEnumerable<AfspraakListViewModel>>(afsprakenDag);

            return new DagOverzichtViewModel
            {
                Datum = datum.Date,
                Tijdsloten = TijdslotHelper.BouwTijdsloten(datum, afsprakenVms, PraktijkInstellingen.SlotDuurMinuten)
            };
        }

        public async Task DeleteReeksAsync(Guid reeksId)
        {
            var afspraken = _afspraakRepo
                .GetAllByCondition(a => a.ReeksId == reeksId)
                .ToList();

            foreach (var afspraak in afspraken)
            {
                if (!string.IsNullOrEmpty(afspraak.GoogleEventId))
                {
                    try
                    {
                        await _calendarService.DeleteEventAsync(afspraak.GoogleEventId);
                    }
                    catch (Exception) { }
                }

                _afspraakRepo.Delete(afspraak);
            }

            _afspraakRepo.SaveChanges();
        }
    }
}
