using DAO;
using DTO;
using Helpers;
using INTERFACES.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ProyectoBaseCore.Controllers
{
    public class MenuController : BaseController
    {
        private readonly IMenuService _menuService;

        public MenuController(IMenuService menuService, ClaimHelper claimHelper):base(claimHelper)
        {
            _menuService = menuService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> ObtenerMenuxPerfil()
        {
            ClaimDTO sesion = ObtenerSesion();
            List<MenuDTO> lstMenuDTO = await _menuService.ObtenerMenuxPerfil(sesion.IdPerfil, sesion.BaseDatos);
            return Ok(lstMenuDTO);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> ObtenerMenus()
        {
            ClaimDTO sesion = ObtenerSesion();
            List<ItemMenu> lstAcceso = await _menuService.ObtenerMenus(sesion.BaseDatos);
            return Ok(lstAcceso);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> ObtenerMenuCompleto()
        {
            ClaimDTO sesion = ObtenerSesion();
            List<ItemMenu> lstAcceso = await _menuService.ObtenerMenuCompleto(sesion.BaseDatos);
            return Ok(lstAcceso);
        }
    }
}
