using DTO;
using System;
using System.Collections.Generic;
using System.Text;

namespace INTERFACES.Services
{
    public interface IConfiguracionService
    {
        Task<ConfiguracionDTO> ObtenerConfiguracion(string baseDatos);
        Task<int> UpdateConfiguracion(ConfiguracionDTO oConfiguracionDTO, int IdUsuario, string baseDatos);
        Task<bool> EnviarEmailPrueba(string baseDatos);
    }
}
