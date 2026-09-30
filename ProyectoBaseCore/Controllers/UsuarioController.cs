using DAO;
using DTO;
using Helpers;
using INTERFACES.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ProyectoBaseCore.Controllers
{
    public class UsuarioController : BaseController
    {
        private readonly IUsuarioService _usuarioService;

        public UsuarioController(IUsuarioService usuarioService, ClaimHelper claimHelper):base(claimHelper)
        {
            _usuarioService = usuarioService;
        }

        public IActionResult Listado()
        {
            return View();
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> ObtenerUsuarios(bool MostrarInactivos)
        {
            ClaimDTO sesion = ObtenerSesion();
            List<UsuarioDTO> datos = await _usuarioService.ObtenerUsuarios(MostrarInactivos, sesion.BaseDatos);
            return Ok(datos);
        }
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> UpdateInsertUsuario([FromBody] UsuarioDTO usuarioDTO)
        {
            ClaimDTO sesion = ObtenerSesion();
            int resultado = await _usuarioService.UpdateInsertUsuario(usuarioDTO, sesion.IdUsuario, sesion.BaseDatos);
            return Ok();
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> ObtenerDatosxID(int IdUsuario)
        {
            ClaimDTO sesion = ObtenerSesion();
            UsuarioDTO oUsuarioDTO = await _usuarioService.ObtenerDatosxID(IdUsuario, sesion.BaseDatos);
            return Ok(oUsuarioDTO);
        }
    }
}
