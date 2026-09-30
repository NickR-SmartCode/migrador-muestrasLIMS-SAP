using Helpers;
using INTERFACES.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ProyectoBaseCore.Controllers
{
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;

        public DashboardController(ClaimHelper helper, IDashboardService dashboardService) : base(helper)
        {
            _dashboardService = dashboardService;
        }

        public IActionResult Listado()
        {
            return View();
        }

        public IActionResult Prefacturas()
        {
            return View();
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> ObtenerDatos(int anio)
        {
            var resultado = await _dashboardService.ObtenerResumenDashboardAsync(anio, ActualDB);
            return ProcesarResultado(resultado);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> ObtenerDatosPrefacturas([FromQuery]int anio)
        {
            var resultado = await _dashboardService.ObtenerResumenDashboardPreFTAsync(anio, ActualDB);
            return ProcesarResultado(resultado);
        }

    }
}
