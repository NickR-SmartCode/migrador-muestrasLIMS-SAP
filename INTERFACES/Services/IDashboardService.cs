using DTO;

namespace INTERFACES.Services
{
    public interface IDashboardService
    {

        Task<ResultOp<Dictionary<string, DashboardMetricsDTO>>> 
            ObtenerResumenDashboardAsync(int anio, string bd);

        Task<ResultOp<Dictionary<string, DashboardMetricsDTO>>> ObtenerResumenDashboardPreFTAsync(int anio, string bd);
    }
}
