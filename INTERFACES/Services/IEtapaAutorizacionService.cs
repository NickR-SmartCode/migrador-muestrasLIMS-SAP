using DTO;
using System;
using System.Collections.Generic;
using System.Text;

namespace INTERFACES.Services
{
    public interface IEtapaAutorizacionService
    {
        Task<List<EtapaAutorizacionDTO>> ListarEtapaAutorizacion(bool MostrarInactivos, string baseDatos);
        Task<int> UpdateInsertEtapaAutorizacion(EtapaAutorizacionDTO oEtapaAutorizacionDTO, int IdUsuario, string baseDatos);
        Task<EtapaAutorizacionDTO> ObtenerEtapaAutorizacion(int IdEtapaAutorizacion, string baseDatos);
    }
}
