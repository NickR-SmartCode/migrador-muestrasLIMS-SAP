using DTO;
using Helpers;
using INTERFACES.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SERVICES;
using System.Security.Claims;

namespace ProyectoBaseCore.Controllers
{
    public class ModeloAutorizacionController : BaseController
    {
        private readonly IModeloAutorizacionService _modeloAutorizacionService; 

        public ModeloAutorizacionController(ClaimHelper claimHelper, IModeloAutorizacionService modeloAutorizacionService) :base(claimHelper)
        {
            _modeloAutorizacionService = modeloAutorizacionService;
        }

        public IActionResult Listado()
        {
            return View();
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> ObtenerModulos()
        {
            ClaimDTO sesion = ObtenerSesion();
            List<ModuloDTO> datos = await _modeloAutorizacionService.ObtenerModulos(sesion.BaseDatos);
            return Ok(datos);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> UpdateInsertModeloAutorizacion([FromBody] ModeloAutorizacionDTO modeloAutorizacionDTO)
        {
            ClaimDTO sesion = ObtenerSesion();
            await _modeloAutorizacionService.UpdateInsertModeloAutorizacion(modeloAutorizacionDTO, sesion.IdUsuario, sesion.BaseDatos);
            return Ok();
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> ListarModeloAutorizacion(bool MostrarInactivos)
        {
            ClaimDTO sesion = ObtenerSesion();
            List<ModeloAutorizacionDTO> datos = await _modeloAutorizacionService.ListarModeloAutorizacion(MostrarInactivos,sesion.BaseDatos);
            return Ok(datos);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> ObtenerModeloAutorizacion(int IdModeloAutorizacion)
        {
            ClaimDTO sesion = ObtenerSesion();
            ModeloAutorizacionDTO datos = await _modeloAutorizacionService.ObtenerModeloAutorizacion(IdModeloAutorizacion, sesion.BaseDatos);
            return Ok(datos);
        }
    }
}
