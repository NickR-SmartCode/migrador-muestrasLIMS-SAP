using DTO;
using DTO.SAPB1;
using Helpers;
using INTERFACES.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SERVICES;

namespace ProyectoBaseCore.Controllers
{
    public class EstadoPrefacturasController : ControllerBase
    {
        private readonly IEstadoMuestrasPreFTService _estadoMuestrasPreFTService;
        private readonly AdminAutomatPFService _adminAuto;

        public EstadoPrefacturasController(ClaimHelper claimHelper, IEstadoMuestrasPreFTService
         estadoMuestrasPreFTService, AdminAutomatPFService adminAuto) : base(claimHelper)
        {
            _estadoMuestrasPreFTService = estadoMuestrasPreFTService;
           _adminAuto = adminAuto;
        }
        public IActionResult Listado()
        {
            return View();
        }

        public IActionResult EstadoIntegracion()
        {
            return View();
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> ListarPaginado([FromBody] EstadoMuestrasPreFTListarPaginado
            peticion, CancellationToken ct)
        {
            var resultado = await _estadoMuestrasPreFTService.ListarPaginadoAsync(peticion, ActualDB, ct);
            return ProcesarResultado(resultado);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> ProcesarMuestrasASap([FromBody] FacturarPreFTEnSAPReqDTO preFts, CancellationToken ct)
        {
            var resultado = await _estadoMuestrasPreFTService.FacturarPreFTEnSAP(preFts, ActualDB, ActualIdUsuario, false);
            return ProcesarResultado(resultado);
        }

        [HttpPost]
        [Authorize]
        public IActionResult IniciarAutomatizacion()
        {
            bool iniciado = _adminAuto.Start(ActualDB);
           

            if (!iniciado) return BadRequest(ResultOp<string>.Fallo("Ya existe un proceso activo."));
            return ProcesarResultado(ResultOp<string>.Ok("Proceso iniciado."));
        }

      
        [HttpPost]
        [Authorize]
        public IActionResult DetenerAutomatizacion()
        {
            _adminAuto.Stop(ActualDB);
            return ProcesarResultado(ResultOp<string>.Ok("Se ha solicitado la detención del proceso."));
        }

        [HttpGet]
        [Authorize]
        public IActionResult ObtenerEstadoAutomatizacion()
        {
            return ProcesarResultado(ResultOp<AutomatizacionPedidoProgDTO>.Ok(_adminAuto.GetProgress(ActualDB)));
        }

        [HttpPost]
        [Authorize]
        public IActionResult TogglePausaAutomatizacion(CancellationToken ct)
        {
            _adminAuto.TogglePause(ActualDB);
            return ProcesarResultado(ResultOp<string>.Ok("Proceso pausado."));
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> ListarLogs([FromBody] ListarPaginadoPeticion peticion)
        {
            var res = await _estadoMuestrasPreFTService.ListarLogsPaginadosAsync(peticion, ActualDB);
            return ProcesarResultado(res);
        }

    }
}
