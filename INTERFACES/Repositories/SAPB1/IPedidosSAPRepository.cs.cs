using DTO;
using DTO.SAPB1;

namespace INTERFACES.Repositories.SAPB1
{
    public interface IPedidosSAPRepository
    {
        Task<ResultadoPaginado<PedidoSapDTO>> ListarPaginadoAsync(ListarPaginadoPeticion peticion, string bd, string orderBy);

        Task<List<PedidoDetalleSapDTO>> ObtenerDetalleAsync(int docEntry);

        Task<PedidoSapDTO> ObtenerPorIdAsync(int docEntry);

        Task<SapQuotationDTO> ObtenerUltimaCotizacionHanaAsync(string cardCode);

        Task<List<SapQuotationDTO>> ListarCotizacionesVigentesAnioActualAsync(string cardCode, string bd);
        Task<ResultadoPaginado<PedidoDetalleConCabeceraSapDTO>> ListarDetallesPaginadosAsync
            (PedidoDetallesListarPaginadosReqDTO peticion);
    }
}