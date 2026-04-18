using AfsprakenbeheerPsycholoog.Authentication;
using AfsprakenbeheerPsycholoog.Data.Entities;
using AfsprakenbeheerPsycholoog.Data.Repositories;
using AfsprakenbeheerPsycholoog.Models.ViewModels.Patient;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AfsprakenbeheerPsycholoog.Services
{
    public class PatientService : IPatientService
    {
        private readonly IPatientRepository _repo;
        private readonly IMapper _mapper;
        private readonly UserManager<ApplicationUser> _userManager;

        public PatientService(IPatientRepository repo, IMapper mapper, UserManager<ApplicationUser> userManager)
        {
            _repo = repo;
            _mapper = mapper;
            _userManager = userManager;
        }

        public IEnumerable<PatientListViewModel> GetAllePatienten()
        {
            var patienten = _repo.GetAllByCondition(p => p.IsActief);
            var gekoppeldeIds = _repo.GetGekoppeldePatientIds().ToHashSet();

            // Map de patiënten naar de ViewModels
            var vmList = _mapper.Map<IEnumerable<PatientListViewModel>>(patienten).ToList();

            // Vul IsGekoppeld efficiënt in d.m.v. de HashSet
            foreach (var vm in vmList)
            {
                vm.IsGekoppeld = gekoppeldeIds.Contains(vm.Id);
            }

            // Retourneer de gesorteerde lijst
            return vmList.OrderBy(p => p.VolledigeNaam);
        }

        public PatientDetailViewModel? GetPatientDetail(int id)
        {
            // We halen de patiënt op INCLUSIEF zijn/haar afspraken & types
            var patient = _repo.GetByIdMetAfspraken(id);
            if (patient == null) return null;

            var vm = _mapper.Map<PatientDetailViewModel>(patient);

            // Bepaal of de patiënt is gekoppeld aan een gebruiker
            var gekoppeldeIds = _repo.GetGekoppeldePatientIds();
            vm.IsGekoppeld = gekoppeldeIds.Contains(id);

            // Sorteer afspraken chronologisch in het detail-overzicht
            vm.Afspraken = vm.Afspraken.OrderByDescending(a => a.Starttijd);

            return vm;
        }

        public EditPatientViewModel? GetPatientEditViewModel(int id)
        {
            var patient = _repo.GetById(id);
            if (patient == null) return null;
            return _mapper.Map<EditPatientViewModel>(patient);
        }

        public int CreatePatient(CreatePatientViewModel vm)
        {
            var patient = _mapper.Map<Patient>(vm);
            _repo.Add(patient);
            _repo.SaveChanges();
            
            return patient.Id;
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

        public IEnumerable<PatientListViewModel> GetInactievePatienten()
        {
            var patienten = _repo.GetInactievePatienten();
            var gekoppeldeIds = _repo.GetGekoppeldePatientIds().ToHashSet();

            var vmList = _mapper.Map<IEnumerable<PatientListViewModel>>(patienten).ToList();

            foreach (var vm in vmList)
            {
                vm.IsGekoppeld = gekoppeldeIds.Contains(vm.Id);
            }

            return vmList.OrderBy(p => p.VolledigeNaam);
        }

        public bool HeractiveerPatient(int id)
        {
            var patient = _repo.GetByIdInclusiefInactief(id);
            if (patient == null) return false;

            patient.IsActief = true;
            patient.VerwijderdOp = null;
            patient.VerwijderdReden = null;

            _repo.Update(patient);
            _repo.SaveChanges();
            return true;
        }

        public void DeletePatient(int id)
        {
            var patientInDb = _repo.GetById(id);
            if (patientInDb == null) return;

            patientInDb.IsActief = false;
            patientInDb.VerwijderdOp = DateTime.Now;
            patientInDb.VerwijderdReden = "Handmatig gedeactiveerd";

            // optioneel: account ontkoppelen
            var user = _repo.GetUserByPatientId(id);
            if (user != null) user.PatientId = null;

            _repo.Update(patientInDb);
            _repo.SaveChanges();
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

        // --- NIEUWE AANMELDINGEN BEHEREN ---

        public async Task<IEnumerable<ApplicationUser>> GetNieuweAanmeldingenAsync()
        {
            var psychologen = await _userManager.GetUsersForClaimAsync(new System.Security.Claims.Claim("IsPsycholoog", "true"));
            var psycholoogIds = psychologen.Select(p => p.Id).ToList();

            var nieuweGebruikers = await _userManager.Users
                .Where(u => u.PatientId == null && !psycholoogIds.Contains(u.Id))
                .ToListAsync();

            return nieuweGebruikers;
        }

        public async Task<(bool succes, string naam)> MaakEnKoppelNieuwePatientAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return (false, string.Empty);

            var newPatientVm = _mapper.Map<CreatePatientViewModel>(user);
            var newPatientId = CreatePatient(newPatientVm);
            
            KoppelPatientAanUser(newPatientId, user.Email);
            
            return (true, $"{newPatientVm.Voornaam} {newPatientVm.Achternaam}");
        }
    }
}

