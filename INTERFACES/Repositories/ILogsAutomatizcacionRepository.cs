using DTO;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Text;

namespace INTERFACES.Repositories
{
    public interface ILogsAutomatizcacionRepository
    {
        Task<ResultadoPaginado<SMC_LogAutomatizacionDTO>> ListarPaginadoAsync(ListarPaginadoPeticion peticion, string bd,
            Action<SqlCommand>? agregarParametrosExtra, CancellationToken? ct);

    }
}
