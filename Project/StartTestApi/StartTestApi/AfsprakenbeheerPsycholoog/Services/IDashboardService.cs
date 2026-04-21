using AfsprakenbeheerPsycholoog.Models.ViewModels.Dashboard;
using AfsprakenbeheerPsycholoog.Models.ViewModels.Planning;

namespace AfsprakenbeheerPsycholoog.Services
{
    /// <summary>
    /// Interface voor de service die verantwoordelijk is voor het beheren van het dashboard en weekoverzicht.
    /// </summary>
    public interface IDashboardService
    {
        DashboardViewModel GetDashboard(string psycholoogNaam, DateTime? weekDatum = null);
        WeekOverzichtViewModel GetWeekOverzicht(DateTime datum, int? patientId, bool isPsycholoog);
    }
}