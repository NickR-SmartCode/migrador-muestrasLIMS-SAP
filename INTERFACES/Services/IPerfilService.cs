using DTO;

namespace INTERFACES.Services
{
    public interface IPerfilService
    {
        Task<List<PerfilDTO>> ObtenerPerfiles(bool MostrarInactivos, string baseDatos);
        Task<PerfilDTO> ObtenerDatosxID(int IdPerfil, string baseDatos);
        Task<int> UpdateInsertPerfil(PerfilDTO oPerfilDTO, int idUsuario, string baseDatos);
    }
}
