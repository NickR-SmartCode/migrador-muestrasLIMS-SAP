using DTO;
using DTO.SAPB1;
using System;
using System.Collections.Generic;
using System.Text;

namespace INTERFACES.Services.SAPB1
{
    public interface IPedidosSAPService
    {

        Task<ResultOp<ResultadoPaginado<PedidoSapDTO>>> ListarPaginadoAsync(ListarPaginadoPeticion peticion, string bd);

        Task<ResultOp<List<PedidoDetalleSapDTO>>> ObtenerDetalleAsync(int docEntry, string bd);
        Task<ResultOp<SapOrderCreationResponseDTO>> FacturarPedidosAsync(FacturarMultipleDTO facturarMultiple);

        Task<ResultOp<SapQuotationDTO>> ObtenerUltimaCotizacionHanaAsync(string cardCode);

        Task<ResultOp<List<SapQuotationDTO>>> ListarCotizacionesVigentesAnioActualAsync(string cardCode, string bd);
        Task<ResultOp<SapOrderCreationResponseDTO>> CrearOrdenAsync(object orden);
        Task<ResultOp<ResultadoPaginado<PedidoDetalleConCabeceraSapDTO>>> ListarDetallesPaginadosAsync(PedidoDetallesListarPaginadosReqDTO
            peticion);
    }
        
}
