using DTO;
using Helpers;
using INTERFACES.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SERVICES;
using System.Security.Claims;

namespace ProyectoBaseCore.Controllers
{
    public class EtapaAutorizacionController : BaseController
    {
        private readonly IEtapaAutorizacionService _etapaAutorizacionService;

        public EtapaAutorizacionController(ClaimHelper claimHelper, IEtapaAutorizacionService etapaAutorizacionService):base(claimHelper)
        {
            _etapaAutorizacionService = etapaAutorizacionService;
        }

        public IActionResult Listado()
        {
            return View();
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> ListarEtapaAutorizacion(bool MostrarInactivos)
        {
            ClaimDTO sesion = ObtenerSesion();
            List<EtapaAutorizacionDTO> datos = await _etapaAutorizacionService.ListarEtapaAutorizacion(MostrarInactivos, sesion.BaseDatos);
            return Ok(datos);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> UpdateInsertEtapaAutorizacion([FromBody] EtapaAutorizacionDTO etapaAutorizacionDTO)
        {
            ClaimDTO sesion = ObtenerSesion();
            await _etapaAutorizacionService.UpdateInsertEtapaAutorizacion(etapaAutorizacionDTO, sesion.IdUsuario, sesion.BaseDatos);
            return Ok();
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> ObtenerEtapaAutorizacion(int IdEtapaAutorizacion)
        {
            ClaimDTO sesion = ObtenerSesion();
            EtapaAutorizacionDTO datos = await _etapaAutorizacionService.ObtenerEtapaAutorizacion(IdEtapaAutorizacion, sesion.BaseDatos);
            return Ok(datos);
        }
    }
}
