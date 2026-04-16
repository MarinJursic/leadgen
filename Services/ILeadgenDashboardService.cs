using leadgen.ViewModels.Home;

namespace leadgen.Services;

public interface ILeadgenDashboardService
{
    HomeDashboardViewModel BuildDashboard();
}
