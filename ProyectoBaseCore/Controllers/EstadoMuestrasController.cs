using DTO;
using DTO.SAPB1;
using DTO.SAPB1ServiceLayer;
using Helpers;
using INTERFACES.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SERVICES;


namespace ProyectoBaseCore.Controllers
{
    public class EstadoMuestrasController : ControllerBase
    {
        private readonly IEstadoMuestrasService _estadoMuestrasService;
        private readonly AdminAutomatizadorPedidoService _adminAuto;

        public EstadoMuestrasController(ClaimHelper claimHelper,
            IEstadoMuestrasService estadoMuestrasService, AdminAutomatizadorPedidoService adminAuto) : base(claimHelper)
        {
            _estadoMuestrasService = estadoMuestrasService;
            _adminAuto = adminAuto;
        }


        public IActionResult Listado()
        {

            return View();
        }

        public IActionResult GestionCreacionDePedidos()
        {

            return View();
        }

        [Authorize]
        [HttpPost]
        public  async Task<IActionResult> ListarPaginado([FromBody] ListarPaginadoPeticion paginadoPeticion, CancellationToken ct)
        {
            var resultado = await _estadoMuestrasService.ListarPaginadoAsync(paginadoPeticion, ActualDB, ct);
            return ProcesarResultado(resultado);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> ProcesarMuestrasASap([FromBody] ProcesarMuestraSAPDTO req, CancellationToken ct)
        {
            var resultado = await _estadoMuestrasService.ProcesarMuestrasASap(req, ActualDB, ActualIdUsuario, ct);
            return ProcesarResultado(resultado);
        }

        #region Automatizacion de creacion de Pedidos SAP
        [HttpPost]
        [Authorize]
        public IActionResult IniciarAutomatizacion()
        {
            bool iniciado = _adminAuto.Start(ActualDB, (ct) =>
                _estadoMuestrasService.EjecutarProcesamientoAutomaticoAsync(ActualDB, ActualIdUsuario, ct)
            );

            if (!iniciado) return BadRequest(ResultOp<string>.Fallo("Ya existe un proceso activo."));
            return ProcesarResultado(ResultOp<string>.Ok("Proceso iniciado."));
        }

        [HttpPost]
        public IActionResult IniciarPruebaCarga()
        {
            bool iniciado = _adminAuto.Start(ActualDB, (ct) =>
                _estadoMuestrasService.EjecutarMockAutomatizacionAsync(ActualDB, ActualIdUsuario, ct)
            );

            if (!iniciado) return BadRequest(ResultOp<string>.Fallo("Ya hay un proceso (real o de prueba) en ejecución."));

            return Ok(ResultOp<string>.Ok("Simulación iniciada."));
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
            var res = await _estadoMuestrasService.ListarLogsPaginadosAsync(peticion, ActualDB);
            return ProcesarResultado(res);
        }
        #endregion

    }
}
