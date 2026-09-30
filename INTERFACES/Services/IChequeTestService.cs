using DTO;
using System;
using System.Collections.Generic;
using System.Text;

namespace INTERFACES.Services
{
    public interface IChequeTestService
    {
        Task<int> UpdateInsertCheque(ChequeTestDTO oChequeTestDTO, int IdUsuario,string BaseUrl, string baseDatos);
        Task<List<ChequeTestDTO>> ObtenerCheques(string baseDatos);
        Task<List<ChequeTestDTO>> ObtenerChequesParaAprobacion(int IdUsuario, string baseDatos);
    }
}
