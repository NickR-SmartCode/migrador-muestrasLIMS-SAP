using DTO;
using System;
using System.Collections.Generic;
using System.Text;

namespace INTERFACES.Repositories
{
    public interface IConfiguracionRepository
    {
        Task<ConfiguracionDTO> ObtenerConfiguracion(string baseDatos);
        Task<int> UpdateConfiguracion(ConfiguracionDTO oConfiguracionDTO, int IdUsuario, string baseDatos);
    }
}
