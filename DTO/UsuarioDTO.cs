using System;
using System.Collections.Generic;
using System.Text;

namespace DTO
{
    public class UsuarioDTO
    {
        public int IdUsuario { get; set; }
        public string? NombreUsuario { get; set; }
        public string? Usuario { get; set; }
        public string? Password { get; set; }
        public int IdPerfil { get; set; }
        public string? NombrePerfil { get; set; }
        public bool Estado { get; set; }
        public string? Correo { get; set; }
        public string? MenuInicio { get; set; }
    }
    public class RefreshTokenResultDTO
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public UsuarioDTO Usuario { get; set; }
        public string JwtToken { get; set; }
        public string RefreshToken { get; set; }
        public string DataBaseInfo { get; set; }
    }
}
