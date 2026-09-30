using DTO;
using Helpers;
using INTERFACES.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ProyectoBaseCore.Controllers
{
    public class PerfilController : BaseController
    {
        private readonly IPerfilService _perfilService;

        public PerfilController(ClaimHelper claimHelper,IPerfilService perfilService) : base(claimHelper)
        {
            _perfilService = perfilService;
        }

        public IActionResult Listado()
        {
            return View();
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> ObtenerPerfiles(bool MostrarInactivos)
        {
            ClaimDTO sesion = ObtenerSesion();
            List<PerfilDTO> datos = await _perfilService.ObtenerPerfiles(MostrarInactivos, sesion.BaseDatos);
            return Ok(datos);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> ObtenerDatosxID(int IdPerfil)
        {
            ClaimDTO sesion = ObtenerSesion();
            PerfilDTO perfil = await _perfilService.ObtenerDatosxID(IdPerfil, sesion.BaseDatos);
            return Ok(perfil);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> UpdateInsertPerfil([FromBody]PerfilDTO perfilDTO)
        {
            ClaimDTO sesion = ObtenerSesion();
            await _perfilService.UpdateInsertPerfil(perfilDTO, sesion.IdUsuario, sesion.BaseDatos);
            return Ok();
        }
    }
}
