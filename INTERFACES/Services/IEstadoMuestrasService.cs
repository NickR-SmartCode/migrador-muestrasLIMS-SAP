using DTO;
using DTO.SAPB1;
using System;
using System.Collections.Generic;
using System.Text;

namespace INTERFACES.Services
{
    public interface IEstadoMuestrasService
    {
        Task<ResultOp<ResultadoPaginado<EstadoMuestraDTO>>> ListarPaginadoAsync(ListarPaginadoPeticion peticion,
            string bd, CancellationToken ct);
        Task<ResultOp<ResultadoPaginado<SMC_LogAutomatizacionDTO>>> ListarLogsPaginadosAsync(ListarPaginadoPeticion peticion,
            string bd);

        Task<ResultOp<SapOrderCreationResponseDTO>> ProcesarMuestrasASap(ProcesarMuestraSAPDTO req, string bd, int idUsuario, CancellationToken ct);

        Task EjecutarProcesamientoAutomaticoAsync(string bd, int idUsuario, CancellationToken ct);

        Task EjecutarMockAutomatizacionAsync(string bd, int idUsuario, CancellationToken ct);


    }
}
