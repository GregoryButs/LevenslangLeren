using AfsprakenbeheerPsycholoog.Authentication;
using AfsprakenbeheerPsycholoog.Data.Entities;
using AfsprakenbeheerPsycholoog.Data.Repositories;
using AfsprakenbeheerPsycholoog.Models.ViewModels.Patient;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AfsprakenbeheerPsycholoog.Services
{
    /// <summary>
    /// Service voor het beheren van patiënten, inclusief koppeling aan gebruikersaccounts en verwerking van nieuwe aanmeldingen.    
    /// </summary>
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
            var patienten = _repo.GetAlleActievePatientenWithAfspraken();
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
            // 1. Controleer of er al een actieve patiënt bestaat met dit e-mailadres
            if (!string.IsNullOrWhiteSpace(vm.Email))
            {
                var cleanEmail = vm.Email.Trim().ToLower();

                bool emailBestaatAl = _repo.GetAllByCondition(p => p.IsActief && p.Email.ToLower() == cleanEmail).Any();

                if (emailBestaatAl)
                {
                    throw new InvalidOperationException($"Er bestaat al een actieve patiënt met het e-mailadres '{vm.Email.Trim()}'.");
                }
            }

            // 2. Map de ViewModel naar de Entity
            var patient = _mapper.Map<Patient>(vm);

            // 3. Voorkom NULL waarden op niet-optionele databasetekstvelden
            patient.Telefoonnummer ??= "";
            patient.SecundairEmail ??= "";
            patient.DossierNummer ??= "";
            patient.Rijksregisternummer ??= "";
            patient.IsActief = true;

            _repo.Add(patient);

            try
            {
                _repo.SaveChanges();
            }
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException("Kan patiënt niet opslaan. Mogelijks bestaat het e-mailadres of dossiernummer al in de database.", ex);
            }

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
                vm.Telefoonnummer ??= "";
                vm.SecundairEmail ??= "";
                vm.DossierNummer ??= "";
                vm.Rijksregisternummer ??= "";

                _mapper.Map(vm, patientInDb);

                patientInDb.Telefoonnummer ??= "";
                patientInDb.SecundairEmail ??= "";
                patientInDb.DossierNummer ??= "";
                patientInDb.Rijksregisternummer ??= "";

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

            // Verwijder het gekoppelde Identity gebruikersaccount zodat het e-mailadres vrijkomt voor nieuwe registraties
            var user = _repo.GetUserByPatientId(id);
            if (user != null)
            {
                _userManager.DeleteAsync(user).GetAwaiter().GetResult();
            }

            _repo.Update(patientInDb);
            _repo.SaveChanges();
        }

        public bool KoppelPatientAanUser(int patientId, string userEmail, bool setAsPrimary = true)
        {
            var (success, _) = KoppelPatientAanUserDetailed(patientId, userEmail, setAsPrimary);
            return success;
        }

        public (bool Success, string ErrorMessage) KoppelPatientAanUserDetailed(int patientId, string userEmail, bool setAsPrimary = true)
        {
            if (string.IsNullOrWhiteSpace(userEmail))
                return (false, "Er is geen e-mailadres opgegeven.");

            var cleanEmail = userEmail.Trim();
            var user = _repo.GetUserByEmail(cleanEmail);
            if (user == null)
            {
                return (false, $"Geen geregistreerd account gevonden voor '{cleanEmail}'. De patiënt moet zich eerst registreren via de registratiepagina.");
            }

            if (user.PatientId.HasValue && user.PatientId.Value != patientId)
            {
                return (false, $"Het account '{cleanEmail}' is al gekoppeld aan een ander patiëntendossier (ID: {user.PatientId.Value}).");
            }

            user.PatientId = patientId;

            var patient = _repo.GetById(patientId);
            if (patient != null)
            {
                if (setAsPrimary)
                {
                    if (!string.Equals(patient.Email, cleanEmail, StringComparison.OrdinalIgnoreCase))
                    {
                        patient.SecundairEmail = patient.Email;
                        patient.Email = cleanEmail;
                    }
                }
                else
                {
                    if (!string.Equals(patient.Email, cleanEmail, StringComparison.OrdinalIgnoreCase))
                    {
                        patient.SecundairEmail = cleanEmail;
                    }
                }
                if (user.Geboortedatum.HasValue)
                {
                    patient.Geboortedatum = user.Geboortedatum.Value;
                }
            }

            _repo.SaveChanges();
            return (true, string.Empty);
        }

        public bool OntkoppelPatientVanUser(int patientId)
        {
            var user = _repo.GetUserByPatientId(patientId);
            if (user == null) return false;

            user.PatientId = null;
            _repo.SaveChanges();
            return true;
        }

        // --- NIEUWE AANMELDINGEN & WACHTLIJT BEHEREN ---

        public async Task<IEnumerable<ApplicationUser>> GetNieuweAanmeldingenAsync()
        {
            var psychologen = await _userManager.GetUsersForClaimAsync(new System.Security.Claims.Claim("IsPsycholoog", "true"));
            var psycholoogIds = psychologen.Select(p => p.Id).ToList();

            var nieuweGebruikers = await _userManager.Users
                .Where(u => u.PatientId == null && !u.IsOpWachtlijst && !psycholoogIds.Contains(u.Id))
                .ToListAsync();

            return nieuweGebruikers;
        }

        public async Task<IEnumerable<ApplicationUser>> GetWachtlijstAanmeldingenAsync()
        {
            var psychologen = await _userManager.GetUsersForClaimAsync(new System.Security.Claims.Claim("IsPsycholoog", "true"));
            var psycholoogIds = psychologen.Select(p => p.Id).ToList();

            var wachtlijstGebruikers = await _userManager.Users
                .Where(u => u.PatientId == null && u.IsOpWachtlijst && !psycholoogIds.Contains(u.Id))
                .OrderByDescending(u => u.WachtlijstDatum)
                .ToListAsync();

            return wachtlijstGebruikers;
        }

        public async Task<bool> PlaatsOpWachtlijstAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return false;

            user.IsOpWachtlijst = true;
            user.WachtlijstDatum = DateTime.UtcNow;
            var result = await _userManager.UpdateAsync(user);
            return result.Succeeded;
        }

        public async Task<bool> HerstelVanWachtlijstAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return false;

            user.IsOpWachtlijst = false;
            user.WachtlijstDatum = null;
            var result = await _userManager.UpdateAsync(user);
            return result.Succeeded;
        }

        public async Task<(bool succes, string naam)> MaakEnKoppelNieuwePatientAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return (false, string.Empty);

            var newPatientVm = _mapper.Map<CreatePatientViewModel>(user);
            var newPatientId = CreatePatient(newPatientVm);
            
            user.IsOpWachtlijst = false;
            user.WachtlijstDatum = null;
            await _userManager.UpdateAsync(user);

            KoppelPatientAanUser(newPatientId, user.Email);
            
            return (true, $"{newPatientVm.Voornaam} {newPatientVm.Achternaam}");
        }

        public (bool Success, string ErrorMessage) MergePatients(MergePatientViewModel model)
        {
            if (model.TargetPatientId <= 0 || model.SourcePatientId <= 0)
                return (false, "Ongeldige patiënt IDs opgegeven.");

            if (model.TargetPatientId == model.SourcePatientId)
                return (false, "Kan een patiënt niet met zichzelf samenvoegen.");

            DateOnly geboortedatum;
            if (!DateOnly.TryParse(model.Geboortedatum, out geboortedatum))
            {
                return (false, "Ongeldige geboortedatum indeling.");
            }

            var updatedData = new Patient
            {
                Id = model.TargetPatientId,
                Voornaam = model.Voornaam.Trim(),
                Achternaam = model.Achternaam.Trim(),
                Geboortedatum = geboortedatum,
                Email = model.Email.Trim(),
                SecundairEmail = string.IsNullOrWhiteSpace(model.SecundairEmail) ? null : model.SecundairEmail.Trim(),
                Telefoonnummer = string.IsNullOrWhiteSpace(model.Telefoonnummer) ? "" : model.Telefoonnummer.Trim(),
                DossierNummer = string.IsNullOrWhiteSpace(model.DossierNummer) ? null : model.DossierNummer.Trim(),
                Rijksregisternummer = string.IsNullOrWhiteSpace(model.Rijksregisternummer) ? null : model.Rijksregisternummer.Trim(),
                EmotioneleStabiliteit = model.EmotioneleStabiliteit
            };

            var success = _repo.MergePatients(model.TargetPatientId, model.SourcePatientId, updatedData);
            if (!success)
            {
                return (false, "Samenvoegen mislukt. Een of beide patiënten konden niet gevonden worden.");
            }

            return (true, string.Empty);
        }
    }
}

