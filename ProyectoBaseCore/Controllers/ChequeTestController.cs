using DAO.SAPLINKER;
using DTO;
using Helpers;
using INTERFACES.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SERVICES;
using System.Security.Claims;

namespace ProyectoBaseCore.Controllers
{
    public class ChequeTestController : BaseController
    {
        private readonly IChequeTestService _chequeTestService;

        public ChequeTestController(ClaimHelper claimHelper, IChequeTestService chequeTestService): base(claimHelper) 
        {
            _chequeTestService = chequeTestService;
        }

        public IActionResult Listado()
        {
            return View();
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> ObtenerCheques()
        {
            ClaimDTO sesion = ObtenerSesion();
            List<ChequeTestDTO> datos = await _chequeTestService.ObtenerCheques(sesion.BaseDatos);
            return Ok(datos);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> ObtenerChequesParaAprobacion()
        {
            ClaimDTO sesion = ObtenerSesion();
            List<ChequeTestDTO> datos = await _chequeTestService.ObtenerChequesParaAprobacion(sesion.IdUsuario,sesion.BaseDatos);
            return Ok(datos);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> UpdateInsertCheque([FromBody] ChequeTestDTO oChequeTestDTO)
        {
            ClaimDTO sesion = ObtenerSesion();
            string baseUrl = $"{Request.Scheme}://{Request.Host}";
            await _chequeTestService.UpdateInsertCheque(oChequeTestDTO, sesion.IdUsuario, baseUrl, sesion.BaseDatos);
            return Ok();
        }
    }
}
