using DAO;
using DTO;
using Helpers;
using INTERFACES.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using ProyectoBaseCore.Models;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace ProyectoBaseCore.Controllers
{
    public class HomeController : Controller
    {
        private readonly Encrypter _encrypter;
        private readonly string _DefaultBD;
        private readonly string _projectHash;
        private readonly IUsuarioService _usuarioService;

        public HomeController(IOptions<GlobalParameters> options, Encrypter encrypter, IUsuarioService usuarioService)
        {
            _DefaultBD = options.Value.DefaultDB;
            _projectHash = options.Value.ProjectHash;
            _encrypter = encrypter;
            _usuarioService = usuarioService;
        }
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpPost]
        public async Task<ActionResult> login([FromBody] LoginRequestDTO request)
        {
                string BaseDatos = _DefaultBD;
                string oldRefreshToken = Request.Cookies["RefreshToken"] == null ? "" : Request.Cookies["RefreshToken"];

                
                UsuarioDTO oUsuarioDTO = await _usuarioService.ValidarUsuario(request.Usuario, request.Password, BaseDatos);

                if (oUsuarioDTO.Estado == true)
                {

                    string jwt = CrearToken(oUsuarioDTO, BaseDatos);
                    string refreshToken = "";
                    if (request.Mantener)
                    {
                        refreshToken = GenerarRefreshToken();
                        await _usuarioService.GuardarRefreshToken(oUsuarioDTO.IdUsuario, refreshToken, oldRefreshToken, BaseDatos);
                    }
                    SetAuthCookies(Response, jwt, refreshToken, _encrypter.Encrypt(BaseDatos));
                    return Ok(new { data = oUsuarioDTO, token = jwt, RefreshToken = refreshToken, DataBaseInfo = _encrypter.Encrypt(BaseDatos) });
                }
                else
                {
                    return BadRequest(new { message = "El usuario está inactivo" });
                }

        }

        [Authorize]
        public async Task<IActionResult> CerrarSesion()
        {
            Response.Cookies.Delete("RefreshToken");
            Response.Cookies.Delete("JWTToken");
            Response.Cookies.Delete("DataBaseInfo");
            return RedirectToAction("Index");
        }
        public string CrearToken(UsuarioDTO usuario, string baseDatos)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_projectHash);

            var claims = new[]
            {
            new Claim("IdUsuario", usuario.IdUsuario.ToString()),
            new Claim("IdPerfil", usuario.IdPerfil.ToString()),
            new Claim("NombreUsuario", usuario.NombreUsuario),
            new Claim("HashedData", _encrypter.Encrypt(baseDatos))
        };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(30),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature
                )
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        private string GenerarRefreshToken()
        {
            byte[] bytes = RandomNumberGenerator.GetBytes(64);
            return Convert.ToBase64String(bytes);
        }

        public void SetAuthCookies(HttpResponse response,string jwtToken,string refreshToken,string dataBaseInfo)
        {
            CrearCookie(response, "JWTToken", jwtToken, DateTime.UtcNow.AddMinutes(30));
            CrearCookie(response, "RefreshToken", refreshToken, DateTime.UtcNow.AddDays(30));
            CrearCookie(response, "DataBaseInfo", dataBaseInfo, DateTime.UtcNow.AddDays(30));
        }

        public void CrearCookie( HttpResponse response,string nombre,string valor, DateTime expires)
        {
            var options = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                Expires = expires,
                SameSite = SameSiteMode.None // recomendado
            };

            response.Cookies.Append(nombre, valor, options);
        }

        [HttpPost]
        public async Task<IActionResult> PostRefreshToken()
        {
                string refreshToken = Request.Cookies["RefreshToken"];
                string baseDatos = _encrypter.Decrypt(Request.Cookies["DataBaseInfo"]);

                var result = await ValidarRefreshToken(refreshToken, baseDatos);

                if (!result.Success)
                {
                    return Unauthorized(new { message = result.Message });
                }

                SetAuthCookies(Response, result.JwtToken, result.RefreshToken, _encrypter.Encrypt(baseDatos));
                return Ok(new
                {
                    data = result.Usuario,
                    token = result.JwtToken
                });
        }

        public async Task<RefreshTokenResultDTO> ValidarRefreshToken(string refreshToken, string BaseDatos)
        {
            try
            {
                if (string.IsNullOrEmpty(refreshToken))
                {
                    return new RefreshTokenResultDTO
                    {
                        Success = false,
                        Message = "Refresh token no encontrado"
                    };
                }

                if (string.IsNullOrEmpty(BaseDatos))
                {
                    return new RefreshTokenResultDTO
                    {
                        Success = false,
                        Message = "DataBaseInfo no encontrado"
                    };
                }

                UsuarioDTO usuario = await _usuarioService.ValidarRefreshToken(refreshToken, BaseDatos);

                if (usuario == null)
                {
                    return new RefreshTokenResultDTO
                    {
                        Success = false,
                        Message = "Refresh token inválido"
                    };
                }

                // Generar nuevo JWT
                string jwt = CrearToken(usuario, BaseDatos);
                //TODO DESACTIVAR SOLO EL REFRESHTOKEN ANTERIOR
                string NewRefreshToken = GenerarRefreshToken();


                await _usuarioService.GuardarRefreshToken(usuario.IdUsuario, NewRefreshToken, refreshToken, BaseDatos);

                return new RefreshTokenResultDTO
                {
                    Success = true,
                    Usuario = usuario,
                    JwtToken = jwt,
                    RefreshToken = NewRefreshToken,
                    DataBaseInfo = _encrypter.Encrypt(BaseDatos)
                };

            }
            catch
            {
                throw;
            }
        }


    }
}
