using DTO;
using System;
using System.Collections.Generic;
using System.Text;

namespace INTERFACES.Repositories
{
    public interface IChequeTestRepository
    {
        Task<int> UpdateInsertCheque(ChequeTestDTO oChequeTestDTO, int IdUsuario, string baseDatos);
        Task<List<ChequeTestDTO>> ObtenerCheques(string baseDatos);
        Task<List<ChequeTestDTO>> ObtenerChequesParaAprobacion(int IdUsuario, string baseDatos);
    }
}
