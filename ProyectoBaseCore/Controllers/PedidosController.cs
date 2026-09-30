using DTO;
using DTO.SAPB1;
using Helpers;
using INTERFACES.Services.SAPB1;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ProyectoBaseCore.Controllers
{
    public class PedidosController : ControllerBase
    {
        private readonly IPedidosSAPService _pedidosSAPService;

        public PedidosController(ClaimHelper claimHelper, IPedidosSAPService pedidosSAPService) : base(claimHelper)
        {
            _pedidosSAPService = pedidosSAPService;
        }
        public IActionResult Listado()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ListarPaginado([FromBody] ListarPaginadoPeticion peticion)
        {
            var resultado = await _pedidosSAPService.ListarPaginadoAsync(peticion, ActualDB);
            return ProcesarResultado(resultado);
        }
        [HttpPost]
        public async Task<IActionResult> ListarDetallesPaginado([FromBody] PedidoDetallesListarPaginadosReqDTO peticion)
        {
            var resultado = await _pedidosSAPService.ListarDetallesPaginadosAsync(peticion);
            return ProcesarResultado(resultado);
        }


        [HttpGet]
        [Authorize]
        public async Task<IActionResult> ObtenerDetalle(int id)
        {
            var resultado = await _pedidosSAPService.ObtenerDetalleAsync(id, ActualDB);
            return ProcesarResultado(resultado);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> FacturarMultiple([FromBody] FacturarMultipleDTO req)
        {
            var resultado = await _pedidosSAPService.FacturarPedidosAsync(req);
            return ProcesarResultado(resultado);
        }
    }
}
