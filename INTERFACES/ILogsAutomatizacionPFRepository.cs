using DTO;
using DTO.SAPB1;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Text;

namespace INTERFACES
{
    public interface ILogsAutomatizacionPFRepository
    {
         Task<ResultadoPaginado<LogsAutomatizacionPF>> ListarPaginadoAsync(ListarPaginadoPeticion peticion,
            string bd, Action<SqlCommand>? agregarParametrosExtra,
        CancellationToken? ct);

        Task GuardarLogAutomatizacionAsync(EstadoMuestrasPreFT preFT, string estado, string mensaje,
            int? docNum, int? series, int? docEntry, int idUsuario, string bd);
    }
}
