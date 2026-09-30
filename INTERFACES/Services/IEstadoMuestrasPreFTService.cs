using DTO;
using DTO.SAPB1;

namespace INTERFACES.Services
{
    public interface IEstadoMuestrasPreFTService
    {
        Task<ResultOp<ResultadoPaginado<EstadoMuestrasPreFT>>> ListarPaginadoAsync(EstadoMuestrasPreFTListarPaginado peticion, string bd,
        CancellationToken ct);

        Task<ResultOp<FacturarPreFTEnSAPResDTO>> FacturarPreFTEnSAP(FacturarPreFTEnSAPReqDTO req, string bd, int idUsuario, bool desdeAutomatico);
        Task<decimal> ObtenerTasaCambioDiaAsync(string moneda);
        Task<ResultOp<ResultadoPaginado<LogsAutomatizacionPF>>> ListarLogsPaginadosAsync(ListarPaginadoPeticion peticion,
        string bd);

        Task EjecutarProcesamientoAutomaticoAsync(string bd, int idUsuario, CancellationToken ct);
    }
}