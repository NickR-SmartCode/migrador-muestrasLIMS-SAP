using DTO;
using Helpers;
using INTERFACES.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using SERVICES;
using System.Security.Claims;

namespace ProyectoBaseCore.Controllers
{
    public class ConfiguracionController : BaseController
    {
        private readonly IConfiguracionService _configuracionService;

        public ConfiguracionController(ClaimHelper claimHelper, IConfiguracionService configuracionService):base(claimHelper)
        {
            _configuracionService= configuracionService;
        }

        public IActionResult Listado()
        {
            return View();
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> ObtenerConfiguracion()
        {
            ClaimDTO sesion = ObtenerSesion();
            ConfiguracionDTO ConfiguracionDTO = await _configuracionService.ObtenerConfiguracion(sesion.BaseDatos);
            return Ok(ConfiguracionDTO);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> UpdateConfiguracion([FromBody] ConfiguracionDTO oConfiguracionDTO)
        {
            ClaimDTO sesion = ObtenerSesion();
            int resultado = await _configuracionService.UpdateConfiguracion(oConfiguracionDTO, sesion.IdUsuario, sesion.BaseDatos);
            return Ok();
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> EnviarEmailPrueba()
        {
            ClaimDTO sesion = ObtenerSesion();
            bool resultado = await _configuracionService.EnviarEmailPrueba(sesion.BaseDatos);
            return Ok();
        }
    }
}
