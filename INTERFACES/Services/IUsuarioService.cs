using DTO;

namespace INTERFACES.Services
{
    public interface IUsuarioService
    {
        Task<List<UsuarioDTO>> ObtenerUsuarios(bool mostrarInactivos, string baseDatos);
        Task<UsuarioDTO> ObtenerDatosxID(int IdUsuario, string baseDatos);
        Task<int> UpdateInsertUsuario(UsuarioDTO usuarioDTO, int idUsuario, string baseDatos);
        Task<UsuarioDTO> ValidarUsuario(string Usuario, string Password, string baseDatos);
        Task<int> GuardarRefreshToken(int IdUsuario, string Token, string TokenAnterior, string baseDatos);
        Task<UsuarioDTO> ValidarRefreshToken(string refreshToken, string baseDatos);
    }
}
