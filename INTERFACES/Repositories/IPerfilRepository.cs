using DTO;
using System;
using System.Collections.Generic;
using System.Text;

namespace INTERFACES.Repositories
{
    public interface IPerfilRepository
    {
        Task<List<PerfilDTO>> ObtenerPerfiles(bool MostrarInactivos, string baseDatos);
        Task<PerfilDTO> ObtenerDatosxID(int IdPerfil, string baseDatos);
        Task<int> UpdateInsertPerfil(PerfilDTO oPerfilDTO, int idUsuario, string baseDatos);
    }
}
