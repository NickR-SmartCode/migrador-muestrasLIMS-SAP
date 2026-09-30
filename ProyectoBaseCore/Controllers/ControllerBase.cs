using DTO;
using Helpers;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ProyectoBaseCore.Controllers
{
    public class ControllerBase : Controller
    {
        private string? _actualDb;
        private int? _actualUserId;
        //private int? _actualSociedadId;
        private ClaimHelper _claimHelper;

        public ControllerBase(ClaimHelper claimHelper)
        {
            _claimHelper = claimHelper;
        }
        private ClaimDTO ObtenerSesion()
        {
            var identity = User.Identity as ClaimsIdentity
                ?? throw new UnauthorizedAccessException("Usuario no autenticado");
            return _claimHelper.ObtenerClaims(identity);
        }

        protected string ActualDB
        {
            get
            {
                if (string.IsNullOrEmpty(_actualDb))
                {
                    _actualDb = ObtenerSesion().BaseDatos;
                }
                return _actualDb ?? "";
            }
        }

        protected int ActualIdUsuario
        {
            get
            {
                if (!_actualUserId.HasValue)
                {

                    _actualUserId = ObtenerSesion().IdUsuario;
                }
                return _actualUserId.Value;
            }
        }

        //protected int ActualSociedadId
        //{
        //    get
        //    {
        //        if (!_actualSociedadId.HasValue)
        //        {
        //            var val = ((ClaimsIdentity)User.Identity).FindFirst("IdSociedad")?.Value;
        //            _actualSociedadId = int.Parse(val ?? "0");
        //        }
        //        return _actualSociedadId.Value;
        //    }
        //}
 

    protected IActionResult ProcesarResultado<T>(ResultOp<T> resultado)
        {
            if (resultado.Exito) return Ok(resultado);

            return resultado.Error?.Tipo switch
            {
                ResultOpErrores.NOT_FOUND => NotFound(resultado),
                ResultOpErrores.UNAUTHORIZED => Unauthorized(resultado),
                ResultOpErrores.VALIDATION_ERROR => BadRequest(resultado),
                _ => StatusCode(500, resultado)
            };
        }
    }
}