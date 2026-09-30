using System;
using System.Collections.Generic;
using System.Text;

namespace INTERFACES.Services
{
    public interface IAprobacionesService
    {
        Task<bool> RegistrarAprobacionDocumento(int IdTabla, int IdModulo, int IdAutor,string baseDatos);
        Task<bool> EnviarCorreoAprobadores(int IdModulo, int IdTablaOriginal, string baseUrl, string BaseDatos);
    }
}
