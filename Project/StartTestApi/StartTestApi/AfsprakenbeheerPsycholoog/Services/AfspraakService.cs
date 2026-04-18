using AfsprakenbeheerPsycholoog.Data.Entities;
using AfsprakenbeheerPsycholoog.Data.Repositories;
using AfsprakenbeheerPsycholoog.Helpers;
using AfsprakenbeheerPsycholoog.Models.ViewModels.Afspraak;
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

        public IEnumerable<AfspraakListViewModel> GetAlleAfspraken()
        {
            var afspraken = _afspraakRepo
                .GetAllMetDetails()
                .Where(a => a.PatientId.HasValue);

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
                Starttijd = DateTime.Now.Date.AddDays(1).AddHours(9),
                PatientenLijst = SelectListHelper.Patienten(_patientRepo.GetAll()),
                TypenLijst = SelectListHelper.Types(_typeRepo.GetAll())
            };
        }

        public bool CreateAfspraak(CreateAfspraakViewModel vm)
        {
            var type = _typeRepo.GetById(vm.TypeId);
            if (type == null) return false;
            if (type.VereistPatient && !vm.PatientId.HasValue) return false;

            var startmomenten = HerhalingHelper.BouwStartmomenten(vm.Starttijd, vm.Herhaling, vm.HerhaalTot);
            var aangemaakt = 0;
            Guid? reeksId = vm.Herhaling == HerhaalPatroon.Geen ? (Guid?)null : Guid.NewGuid();
            var isBlokkering = !vm.PatientId.HasValue;

            foreach (var start in startmomenten)
            {
                var eind = start.AddMinutes(type.StandaardDuurMinuten);

                if (!TryVoegAfspraakToe(start, eind, isBlokkering, () =>
                {
                    var afspraak = _mapper.Map<Afspraak>(vm);
                    afspraak.Starttijd = start;
                    afspraak.Eindtijd = eind;
                    afspraak.ReeksId = reeksId;
                    return afspraak;
                }))
                {
                    continue;
                }

                aangemaakt++;
            }

            if (aangemaakt == 0) return false;

            _afspraakRepo.SaveChanges();
            return true;
        }

        public EditAfspraakViewModel? GetEditViewModel(int id)
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
            if (type.VereistPatient && !vm.PatientId.HasValue) return false;

            var eindtijd = vm.Starttijd.AddMinutes(type.StandaardDuurMinuten);
            var isBlokkering = !vm.PatientId.HasValue;

            if (_afspraakRepo.HeeftConflict(vm.Starttijd, eindtijd, isBlokkering, vm.Id))
            {
                return false;
            }

            _mapper.Map(vm, afspraakInDb);
            afspraakInDb.Eindtijd = eindtijd;

            _afspraakRepo.Update(afspraakInDb);
            _afspraakRepo.SaveChanges();

            return true;
        }

        public void DeleteAfspraak(int id)
        {
            var afspraak = _afspraakRepo.GetById(id);
            if (afspraak == null) return;

            _afspraakRepo.Delete(afspraak);
            _afspraakRepo.SaveChanges();
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

        public void DeleteReeks(Guid reeksId)
        {
            var afspraken = _afspraakRepo
                .GetAllByCondition(a => a.ReeksId == reeksId)
                .ToList();

            foreach (var afspraak in afspraken)
            {
                _afspraakRepo.Delete(afspraak);
            }

            _afspraakRepo.SaveChanges();
        }

        private bool TryVoegAfspraakToe(DateTime start, DateTime eind, bool isBlokkering, Func<Afspraak> afspraakFactory)
        {
            if (_afspraakRepo.HeeftConflict(start, eind, isBlokkering))
            {
                return false;
            }

            _afspraakRepo.Add(afspraakFactory());
            return true;
        }
    }
}
