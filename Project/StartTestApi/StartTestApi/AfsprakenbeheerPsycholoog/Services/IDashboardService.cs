using AfsprakenbeheerPsycholoog.Models.ViewModels.Dashboard;
using AfsprakenbeheerPsycholoog.Models.ViewModels.Planning;

namespace AfsprakenbeheerPsycholoog.Services
{
    public interface IDashboardService
    {
        DashboardViewModel GetDashboard(string psycholoogNaam, DateTime? weekDatum = null);
        WeekOverzichtViewModel GetWeekOverzicht(DateTime datum, int? patientId, bool isPsycholoog);
    }
}