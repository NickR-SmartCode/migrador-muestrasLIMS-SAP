using DTO;
using Helpers;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ProyectoBaseCore.Controllers
{
    public class BaseController : Controller
    {
        protected readonly ClaimHelper _claimHelper;
        public BaseController(ClaimHelper claimHelper)
        {
            _claimHelper = claimHelper;
        }
        protected ClaimDTO ObtenerSesion()
        {
            var identity = User.Identity as ClaimsIdentity
                ?? throw new UnauthorizedAccessException("Usuario no autenticado");
            return _claimHelper.ObtenerClaims(identity);
        }
    }
}
