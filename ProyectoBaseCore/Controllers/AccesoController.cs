using DAO;
using DAO.SAPLINKER;
using DTO;
using Helpers;
using INTERFACES.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ProyectoBaseCore.Controllers
{
    public class AccesoController : BaseController
    {
        private readonly IMenuService _menuService;

        public AccesoController(ClaimHelper claimHelper, IMenuService menuService):base(claimHelper)
        {
            _menuService = menuService;
        }
        public IActionResult Listado()
        {
            return View();
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> ObtenerAccesos(int IdPerfil)
        {
            ClaimDTO sesion = ObtenerSesion();
            List<MenuDTO> lstAcceso = await _menuService.obtenerMenuPerfil(IdPerfil, sesion.BaseDatos);
            return Ok(lstAcceso);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> grabarAcceso(List<int> Accesos, int IdPerfil)
        {
            ClaimDTO sesion = ObtenerSesion();
            await _menuService.GrabarAccesos(Accesos, IdPerfil, sesion.IdUsuario, sesion.BaseDatos);
            return Ok();

        }
    }
}
