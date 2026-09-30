using DTO;
using System;
using System.Collections.Generic;
using System.Text;

namespace INTERFACES.Repositories
{
    public interface IDashboardRepository
    {
        Task<List<LimsRawDataDTO>> ObtenerMuestrasLimsRawAsync(int anio, string bd);
        Task<List<SapCustomerMasterDTO>> ObtenerClientesMaestrosSapAsync(string bd);
        Task<List<SapItemMasterDTO>> ObtenerArticulosMaestrosSapAsync(string bd);
        Task<List<SapIntegrationDataDTO>> ObtenerVincualcionesSapRawAsync(int anio, string bd);
        Task<List<SapQuotationPriceDTO>> ObtenerPreciosUltimasCotizacionesAsync(string bd);
    }
}
