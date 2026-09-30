using DTO;
using DTO.SAPB1;
using Microsoft.Data.SqlClient;
using System.Data;


namespace INTERFACES.Repositories
{
    public interface IEstadoMuestrasRepository
    {
        Task<ResultadoPaginado<EstadoMuestraDTO>> ListarPaginadoAsync(ListarPaginadoPeticion peticion, string bd,
              CancellationToken ct, Action<SqlCommand>? agregarParametrosExtra = null);

        Task GuardarRelacionMuestraSapAsync(MuestraIdentificadorDTO m, int docEntry, int series, int docNum,
            string bd, int idUsuario);

        Task<List<SMC_MuestraProcesadaDTO>> ObtenerMatchesProcesadosLocalAsync(List<long> sampleNumbers, string bd);

        Task GuardarLogAutomatizacionAsync(EstadoMuestraDTO m, string estado, string mensaje, int? docNum, int? series, int? docEntry, int idUsuario, string bd);
        Task<List<EstadoMuestraDTO>> ObtenerPendientesAutomatizacionAsync(string bd, CancellationToken ct);
        Task CargarMaestrosDesdeHanaAsync(List<long> customerIds, List<string> analysisCodes, string schema);
        Task<string?> ObtenerGroupNumClienteAsync(bool esSNExtranjero, string cardCode, bool tieneDet, string schema);
        Task<bool> ExisteProveedorEnSAP(string cardCode);
        Task<ClienteSAPDTO> BuscarCardCodeByRUC(string ruc);


    }
}
