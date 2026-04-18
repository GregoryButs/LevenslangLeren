using AfsprakenbeheerPsycholoog.Data.Entities;
using AfsprakenbeheerPsycholoog.Data.Repositories;
using AfsprakenbeheerPsycholoog.Helpers;
using AfsprakenbeheerPsycholoog.Models.ViewModels.Afspraak;
using AfsprakenbeheerPsycholoog.Models.ViewModels.Dashboard;
using AfsprakenbeheerPsycholoog.Models.ViewModels.Planning;
using AutoMapper;

namespace AfsprakenbeheerPsycholoog.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly IAfspraakRepository _afspraakRepo;
        private readonly IMapper _mapper;

        public DashboardService(IAfspraakRepository afspraakRepo, IMapper mapper)
        {
            _afspraakRepo = afspraakRepo;
            _mapper = mapper;
        }

        public DashboardViewModel GetDashboard(string psycholoogNaam, DateTime? weekDatum = null)
        {
            var vandaag = DateTime.Today;
            var peilDatum = weekDatum ?? vandaag;
            var (startWeek, eindeWeek) = WeekHelper.GetHuidigeWeek(vandaag);

            var afsprakenVandaag = _afspraakRepo.GetByDatum(vandaag)
                .Where(a => a.Status == AfspraakStatus.Gepland && a.PatientId.HasValue)
                .OrderBy(a => a.Starttijd)
                .ToList();

            var volgendeAfspraak = _afspraakRepo.GetVolgende(zonderBlokkeringen: true);

            return new DashboardViewModel
            {
                PsycholoogNaam = psycholoogNaam,
                AantalAfsprakenVandaag = afsprakenVandaag.Count,
                AantalAfsprakenDezeWeek = _afspraakRepo.CountByWeek(startWeek, eindeWeek, zonderBlokkeringen: true),
                AantalPatienten = _afspraakRepo.CountPatienten(),
                AfsprakenVandaag = _mapper.Map<List<AfspraakListViewModel>>(afsprakenVandaag),
                VolgendeAfspraak = volgendeAfspraak != null
                    ? _mapper.Map<AfspraakListViewModel>(volgendeAfspraak)
                    : null,
                WeekOverzicht = GetWeekOverzicht(peilDatum, null, true)
            };
        }

        public WeekOverzichtViewModel GetWeekOverzicht(DateTime datum, int? patientId, bool isPsycholoog)
        {
            var (startWeek, eindeWeek) = WeekHelper.GetHuidigeWeek(datum);

            if (!isPsycholoog && !patientId.HasValue)
            {
                return WeekHelper.BouwWeekOverzicht(datum, new List<AfspraakListViewModel>(), false);
            }

            var weekAfspraken = _afspraakRepo.GetInPeriodeMetDetails(
                startWeek,
                eindeWeek,
                !isPsycholoog ? patientId : null);

            var vmList = _mapper.Map<IEnumerable<AfspraakListViewModel>>(weekAfspraken).ToList();
            return WeekHelper.BouwWeekOverzicht(datum, vmList, isPsycholoog);
        }
    }
}