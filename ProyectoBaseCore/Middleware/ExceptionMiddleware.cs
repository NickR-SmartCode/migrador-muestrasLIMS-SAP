using DTO;
using Microsoft.Data.SqlClient;
using System.Net;
using System.Net.NetworkInformation;
using System.Text.Json;

namespace ProyectoBaseCore.Middleware
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "No autorizado");
                await HandleExceptionAsync(context, HttpStatusCode.Unauthorized, ex.Message);
            }
            catch (SqlException sqlEx) when (sqlEx.Number == 0)
            {
                context.Response.ContentType = "application/json";
                context.Response.StatusCode = (int)444;

                var response = ResultOp<object>.Fallo("Operacion cancelada por el usuario", ResultOpErrores.CANCELLED);

                var json = JsonSerializer.Serialize(response);
                await context.Response.WriteAsync(json);
            }
            catch(TimeoutException)
            {

                context.Response.ContentType = "application/json";
                context.Response.StatusCode = (int)445;

                var response = ResultOp<object>.Fallo("Tiempo de espera agotado, por favor, intente nuevamente.", ResultOpErrores.TIMEOUT);

                var json = JsonSerializer.Serialize(response);
                await context.Response.WriteAsync(json);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado");
                await HandleExceptionAsync(context, HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, HttpStatusCode status, string message)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)status;

            var response = ResultOp<bool>.Fallo("Error critico en el servidor", ResultOpErrores.INTERNAL_SERVER_ERROR);
            var json = JsonSerializer.Serialize(response);
            await context.Response.WriteAsync(json);
        }
    }
}
