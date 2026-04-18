using AfsprakenbeheerPsycholoog.Data.Entities;
using AfsprakenbeheerPsycholoog.Data.Repositories;
using AfsprakenbeheerPsycholoog.Helpers;
using AfsprakenbeheerPsycholoog.Models.ViewModels.Afspraak;
using AfsprakenbeheerPsycholoog.Models.ViewModels.PatientPortaal;
using AfsprakenbeheerPsycholoog.Models.ViewModels.Planning;
using AutoMapper;

namespace AfsprakenbeheerPsycholoog.Services
{
    public class PatientBoekService : IPatientBoekService
    {
        private readonly IAfspraakRepository _afspraakRepo;
        private readonly IAfspraakTypeRepository _typeRepo;
        private readonly IMapper _mapper;

        public PatientBoekService(
            IAfspraakRepository afspraakRepo,
            IAfspraakTypeRepository typeRepo,
            IMapper mapper)
        {
            _afspraakRepo = afspraakRepo;
            _typeRepo = typeRepo;
            _mapper = mapper;
        }

        public PatientBoekAfspraakViewModel GetBoekViewModel(DateTime datum)
        {
            return new PatientBoekAfspraakViewModel
            {
                Datum = datum
            };
        }

        public DagOverzichtViewModel GetDagOverzichtVoorPatient(DateTime datum)
        {
            var afsprakenDag = _afspraakRepo.GetByDatum(datum);
            var afsprakenVms = _mapper.Map<IEnumerable<AfspraakListViewModel>>(afsprakenDag);

            return new DagOverzichtViewModel
            {
                Datum = datum.Date,
                Tijdsloten = TijdslotHelper.BouwTijdsloten(datum, afsprakenVms, 60)
            };
        }

        public bool CreatePatientAfspraak(PatientBoekAfspraakViewModel vm, int patientId)
        {
            if (!vm.GekozeTijdslot.HasValue) return false;

            var starttijd = vm.GekozeTijdslot.Value;
            if (!PraktijkInstellingen.MagBoeken(starttijd)) return false;

            var standaardType = GetStandaardTypeVoorPatientBoeking();
            if (standaardType == null) return false;

            var eindtijd = starttijd.AddMinutes(60);
            if (_afspraakRepo.HeeftConflict(starttijd, eindtijd)) return false;

            var afspraak = new Afspraak
            {
                PatientId = patientId,
                TypeId = standaardType.Id,
                Starttijd = starttijd,
                Eindtijd = eindtijd,
                Status = AfspraakStatus.Gepland,
                Opmerkingen = vm.Opmerkingen
            };

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

        private AfspraakType? GetStandaardTypeVoorPatientBoeking()
        {
            var types = _typeRepo.GetAll().OrderBy(t => t.Id).ToList();
            return types.FirstOrDefault(t => t.StandaardDuurMinuten == 60) ?? types.FirstOrDefault();
        }
    }
}