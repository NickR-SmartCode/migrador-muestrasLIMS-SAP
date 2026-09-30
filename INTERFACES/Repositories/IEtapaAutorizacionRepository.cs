using DTO;
using System;
using System.Collections.Generic;
using System.Text;

namespace INTERFACES.Repositories
{
    public interface IEtapaAutorizacionRepository
    {
        Task<List<EtapaAutorizacionDTO>> ListarEtapaAutorizacion(bool MostrarInactivos, string baseDatos);
        Task<int> UpdateInsertEtapaAutorizacion(EtapaAutorizacionDTO oEtapaAutorizacionDTO, int IdUsuario, string baseDatos);
        Task<int> UpdateInsertEtapaAutorizacionDetalle(EtapaAutorizacionDetalleDTO oEtapaAutorizacionDetalleDTO, string baseDatos);
        Task<EtapaAutorizacionDTO> ObtenerEtapaAutorizacion(int IdEtapaAutorizacion, string baseDatos);
        Task<List<EtapaAutorizacionDetalleDTO>> ObtenerDetallaEtapaAutorizacion(int IdEtapaAutorizacion, string baseDatos);
    }
}
