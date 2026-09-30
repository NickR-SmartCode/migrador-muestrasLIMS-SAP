using DTO;
using DTO.SAPB1;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Text;

namespace INTERFACES.Repositories
{
    public interface IEstadoMuestrasPreFTRepository
    {
        Task<ResultadoPaginado<EstadoMuestrasPreFT>> ListarPaginadoAsync(ListarPaginadoPeticion peticion, string bd,
   Action<SqlCommand>? agregarParametrosExtra = null, CancellationToken? ct = null);

        Task<List<EstadoMuestrasPreFT>> ObtenerPrefacturasPlanasPorAnioAsync(int anio, string bd);
    }
}
