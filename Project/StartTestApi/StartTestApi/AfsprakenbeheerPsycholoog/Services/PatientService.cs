using AfsprakenbeheerPsycholoog.Data.Entities;
using AfsprakenbeheerPsycholoog.Data.Repositories;
using AfsprakenbeheerPsycholoog.Models.ViewModels.Patient;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace AfsprakenbeheerPsycholoog.Services
{
    public class PatientService : IPatientService
    {
        private readonly IPatientRepository _repo;
        private readonly IMapper _mapper;

        public PatientService(IPatientRepository repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        public IEnumerable<PatientListViewModel> GetAllePatienten()
        {
            var patienten = _repo.GetAll();
            var gekoppeldeIds = _repo.GetGekoppeldePatientIds().ToHashSet();

            return _mapper.Map<IEnumerable<PatientListViewModel>>(patienten)
                .OrderBy(p => p.VolledigeNaam);
        }

        public PatientDetailViewModel? GetPatientDetail(int id)
        {
            // We halen de patiënt op INCLUSIEF zijn/haar afspraken & types
            var patient = _repo.GetByIdMetAfspraken(id);
            if (patient == null) return null;

            var vm = _mapper.Map<PatientDetailViewModel>(patient);

            // Sorteer afspraken chronologisch in het detail-overzicht
            vm.Afspraken = vm.Afspraken.OrderByDescending(a => a.Starttijd);

            return vm;
        }

        public EditPatientViewModel GetPatientEditViewModel(int id)
        {
            var patient = _repo.GetById(id);
            if (patient == null) return null;
            return _mapper.Map<EditPatientViewModel>(patient);
        }

        public void CreatePatient(CreatePatientViewModel vm)
        {
            var patient = _mapper.Map<Patient>(vm);
            _repo.Add(patient);
            _repo.SaveChanges();
        }

        public EditPatientViewModel? GetPatientForEdit(int id)
        {
            var patient = _repo.GetById(id);
            if (patient == null) return null;
            return _mapper.Map<EditPatientViewModel>(patient);
        }


        public void EditPatient(EditPatientViewModel vm)
        {
            var patientInDb = _repo.GetById(vm.Id);
            if (patientInDb != null)
            {
                _mapper.Map(vm, patientInDb);
                _repo.Update(patientInDb);
                _repo.SaveChanges();
            }
        }

        public void DeletePatient(int id)
        {
            var patientInDb = _repo.GetById(id);
            if (patientInDb != null)
            {
                _repo.Delete(patientInDb);
                _repo.SaveChanges();
            }
        }

        public bool KoppelPatientAanUser(int patientId, string userEmail)
        {
            var user = _repo.GetUserByEmail(userEmail);
            if (user == null) return false;
            if (user.PatientId.HasValue && user.PatientId != patientId) return false;

            user.PatientId = patientId;
            _repo.SaveChanges();
            return true;
        }

        public bool OntkoppelPatientVanUser(int patientId)
        {
            var user = _repo.GetUserByPatientId(patientId);
            if (user == null) return false;

            user.PatientId = null;
            _repo.SaveChanges();
            return true;
        }
    }
}

