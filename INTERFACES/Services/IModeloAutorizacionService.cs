using DTO;
using System;
using System.Collections.Generic;
using System.Text;

namespace INTERFACES.Services
{
    public interface IModeloAutorizacionService
    {
        Task<List<ModuloDTO>> ObtenerModulos(string baseDatos);
        Task<int> UpdateInsertModeloAutorizacion(ModeloAutorizacionDTO oModeloAutorizacionDTO, int IdUsuario, string baseDatos);
        Task<List<ModeloAutorizacionDTO>> ListarModeloAutorizacion(bool MostrarInactivos, string baseDatos);
        Task<ModeloAutorizacionDTO> ObtenerModeloAutorizacion(int IdModeloAutorizacion, string baseDatos);
    }
}
