using AfsprakenbeheerPsycholoog.Data.Entities;
using AfsprakenbeheerPsycholoog.Data.Repositories;
using AfsprakenbeheerPsycholoog.Helpers;
using AfsprakenbeheerPsycholoog.Models.ViewModels.Afspraak;
using AfsprakenbeheerPsycholoog.Models.ViewModels.Dashboard;
using AfsprakenbeheerPsycholoog.Models.ViewModels.PatientPortaal;
using AfsprakenbeheerPsycholoog.Models.ViewModels.Planning;
using AutoMapper;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AfsprakenbeheerPsycholoog.Services
{
    public class AfspraakService : IAfspraakService
    {
        private readonly IAfspraakRepository _afspraakRepo;
        private readonly IPatientRepository _patientRepo;
        private readonly IAfspraakTypeRepository _typeRepo;
        private readonly IMapper _mapper;

        public AfspraakService(
            IAfspraakRepository afspraakRepo,
            IPatientRepository patientRepo,
            IAfspraakTypeRepository typeRepo,
            IMapper mapper)
        {
            _afspraakRepo = afspraakRepo;
            _patientRepo = patientRepo;
            _typeRepo = typeRepo;
            _mapper = mapper;
        }

        // ==================== LEZEN ====================

        public IEnumerable<AfspraakListViewModel> GetAlleAfspraken()
        {
            var afspraken = _afspraakRepo.GetAllMetDetails();
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

        // --- Create Logica ---

        public CreateAfspraakViewModel GetCreateViewModel()
        {
            var vm = new CreateAfspraakViewModel
            {
                Starttijd = DateTime.Now.Date.AddDays(1).AddHours(9),
                PatientenLijst = SelectListHelper.Patienten(_patientRepo.GetAll()),
                TypenLijst = SelectListHelper.Types(_typeRepo.GetAll())
            };
            return vm;
        }

        public bool CreateAfspraak(CreateAfspraakViewModel vm)
        {
            var type = _typeRepo.GetById(vm.TypeId);
            if (type == null) return false;

            var eindtijd = vm.Starttijd.AddMinutes(type.StandaardDuurMinuten);

            // Controleer op overlapping!
            if (_afspraakRepo.HeeftConflict(vm.Starttijd, eindtijd))
            {
                return false; // Er is een conflict, breek de creatie af
            }

            var afspraak = _mapper.Map<Afspraak>(vm);
            afspraak.Eindtijd = eindtijd;

            _afspraakRepo.Add(afspraak);
            _afspraakRepo.SaveChanges();

            return true;
        }

        // --- Edit Logica ---

        public EditAfspraakViewModel GetEditViewModel(int id)
        {
            var afspraak = _afspraakRepo.GetById(id);
            if (afspraak == null) return null;

            var vm = _mapper.Map<EditAfspraakViewModel>(afspraak);
            vm.PatientenLijst = SelectListHelper.Patienten(_patientRepo.GetAll(), vm.PatientId);
            vm.TypenLijst = SelectListHelper.Types(_typeRepo.GetAll(), vm.TypeId);
            vm.StatusLijst = SelectListHelper.Statussen(vm.Status);
            return vm;
        }

        public bool EditAfspraak(EditAfspraakViewModel vm)
        {
            var afspraakInDb = _afspraakRepo.GetById(vm.Id);
            if (afspraakInDb == null) return false;

            var type = _typeRepo.GetById(vm.TypeId);
            if (type == null) return false;

            var eindtijd = vm.Starttijd.AddMinutes(type.StandaardDuurMinuten);

            // Controleer op overlapping en stuur het huidige vm.Id mee!
            if (_afspraakRepo.HeeftConflict(vm.Starttijd, eindtijd, vm.Id))
            {
                return false; // Er is een overlap, breek het bewerken af
            }

            _mapper.Map(vm, afspraakInDb);
            afspraakInDb.Eindtijd = eindtijd;

            _afspraakRepo.Update(afspraakInDb);
            _afspraakRepo.SaveChanges();

            return true;
        }
        

        public void DeleteAfspraak(int id)
        {
            Afspraak? afspraak = _afspraakRepo.GetById(id);
            if (afspraak != null)
            {
                _afspraakRepo.Delete(afspraak);
                _afspraakRepo.SaveChanges();
            }
        }

        // ==================== DagPlanning ====================
         public DagOverzichtViewModel GetDagOverzicht(DateTime datum)
        {
            var afsprakenDag = _afspraakRepo.GetByDatum(datum);
            return new DagOverzichtViewModel
            {
                Datum = datum.Date,
                Tijdsloten = TijdslotHelper.BouwTijdsloten(datum, afsprakenDag, _mapper)
            };
        }

        // ==================== Patient Portaal ====================
        public PatientBoekAfspraakViewModel GetBoekViewModel(DateTime datum)
        {
            return new PatientBoekAfspraakViewModel
            {
                Datum = datum,
                TypenLijst = SelectListHelper.Types(_typeRepo.GetAll())
            };
        }


        public bool CreatePatientAfspraak(PatientBoekAfspraakViewModel vm, int patientId)
        {
            var type = _typeRepo.GetById(vm.TypeId);
            if (type == null) return false;

            if (!PraktijkInstellingen.MagBoeken(vm.GekozeTijdslot)) return false;

            var eindtijd = vm.GekozeTijdslot.AddMinutes(type.StandaardDuurMinuten);
            if (_afspraakRepo.HeeftConflict(vm.GekozeTijdslot, eindtijd)) return false;

            var afspraak = _mapper.Map<Afspraak>(vm);  // Starttijd + Status + TypeId + Opmerkingen
            afspraak.PatientId = patientId;            // manueel
            afspraak.Eindtijd = eindtijd;             // manueel

            _afspraakRepo.Add(afspraak);
            _afspraakRepo.SaveChanges();
            return true;
        }

        public bool AnnuleerPatientAfspraak(int afspraakId, int patientId)
        {
            var afspraak = _afspraakRepo.GetByIdEnPatient(afspraakId, patientId);
            if (afspraak == null) return false;
            if (afspraak.Status != AfspraakStatus.Gepland) return false;
            if (afspraak.Starttijd <= DateTime.Now) return false;

            afspraak.Status = AfspraakStatus.Geannuleerd;
            _afspraakRepo.Update(afspraak);
            _afspraakRepo.SaveChanges();
            return true;
        }

        // ==================== DASHBOARD ====================

        public DashboardViewModel GetDashboard(string psycholoogNaam)
        {
            var vandaag = DateTime.Today;
            var (startWeek, eindeWeek) = WeekHelper.GetHuidigeWeek(vandaag);

            var afsprakenVandaag = _afspraakRepo.GetByDatum(vandaag)
                .Where(a => a.Status == AfspraakStatus.Gepland)
                .OrderBy(a => a.Starttijd)
                .ToList();

            return new DashboardViewModel
            {
                PsycholoogNaam = psycholoogNaam,
                AantalAfsprakenVandaag = afsprakenVandaag.Count,
                AantalAfsprakenDezeWeek = _afspraakRepo.CountByWeek(startWeek, eindeWeek),
                AantalPatienten = _afspraakRepo.CountPatienten(),
                AfsprakenVandaag = _mapper.Map<List<AfspraakListViewModel>>(afsprakenVandaag),
                VolgendeAfspraak = _afspraakRepo.GetVolgende() is Afspraak v
                                            ? _mapper.Map<AfspraakListViewModel>(v) : null
            };
        }
    }
}
