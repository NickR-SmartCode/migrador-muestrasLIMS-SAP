using Helpers;
using INTERFACES;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ProyectoBaseCore.Controllers
{
    public class OCRDSAPController : ControllerBase
    {
        private readonly IOCRDSAPService _oCRDSAPService;

        public OCRDSAPController(ClaimHelper claimHelper, IOCRDSAPService oCRDSAPService) : base(claimHelper)
        {
            _oCRDSAPService = oCRDSAPService;
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Listar(string busqueda)
        {
            return ProcesarResultado(await _oCRDSAPService.ListarAsync(busqueda));
        }
    }
}