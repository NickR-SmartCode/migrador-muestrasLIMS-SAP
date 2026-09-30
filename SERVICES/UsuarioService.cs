using DTO;
using Helpers;
using INTERFACES.Repositories;
using INTERFACES.Services;

namespace SERVICES
{
    public class UsuarioService : IUsuarioService
    {
        private readonly IUsuarioRepository _repository;
        private readonly Encrypter _encrypter;

        public UsuarioService(IUsuarioRepository repository, Encrypter encrypter)
        {
            _repository = repository;
            _encrypter = encrypter;
        }

        public async Task<List<UsuarioDTO>> ObtenerUsuarios(bool mostrarInactivos, string baseDatos) => await _repository.ObtenerUsuarios(mostrarInactivos, baseDatos);
        public async Task<UsuarioDTO> ObtenerDatosxID(int IdUsuario, string baseDatos) => await _repository.ObtenerDatosxID(IdUsuario, baseDatos);
        public async Task<int> GuardarRefreshToken(int IdUsuario, string Token, string TokenAnterior, string Base) => await _repository.GuardarRefreshToken(IdUsuario, Token, TokenAnterior, Base);
        public async Task<UsuarioDTO> ValidarRefreshToken(string refreshToken, string BaseDatos) => await _repository.ValidarRefreshToken(refreshToken, BaseDatos);
        public async Task<int> UpdateInsertUsuario(UsuarioDTO usuarioDTO, int idUsuario, string baseDatos)
        {
            if (!String.IsNullOrEmpty(usuarioDTO.Password))
            {
                usuarioDTO.Password = _encrypter.ObtenerSHA256String(usuarioDTO.Password);
            }
            int resultado = await _repository.UpdateInsertUsuario(usuarioDTO, idUsuario, baseDatos);

            return resultado;
        }
        public async Task<UsuarioDTO> ValidarUsuario(string Usuario, string Password, string BaseDatos)
        {

            Password = _encrypter.ObtenerSHA256String(Password);
            UsuarioDTO oUsuarioDTO = await _repository.ValidarUsuario(Usuario, Password, BaseDatos);

            if (String.IsNullOrEmpty(oUsuarioDTO.Usuario))
            {
                throw new Exception("Usuario y/o Contraseña Incorrectos");
            }
            return oUsuarioDTO;
        }


    }
}
