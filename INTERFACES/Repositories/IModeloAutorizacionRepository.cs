using DTO;
using System;
using System.Collections.Generic;
using System.Text;

namespace INTERFACES.Repositories
{
    public interface IModeloAutorizacionRepository
    {
        Task<List<ModuloDTO>> ObtenerModulos(string baseDatos);
        Task<int> UpdateInsertModeloAutorizacion(ModeloAutorizacionDTO oModeloAutorizacionDTO, int IdUsuario, string baseDatos);
        Task<int> InsertModeloAutorizacionAutor(ModeloAutorizacionAutorDTO oModeloAutorizacionAutorDTO, string baseDatos);
        Task<int> InsertModeloAutorizacionEtapa(ModeloAutorizacionEtapaDTO oModeloAutorizacionEtapaDTO, string baseDatos);
        Task<int> InsertModeloAutorizacionModulo(ModeloAutorizacionModuloDTO oModeloAutorizacionModuloDTO, string baseDatos);
        Task<int> InsertModeloAutorizacionCondicion(ModeloAutorizacionCondicionDTO oModeloAutorizacionCondicionDTO, string baseDatos);
        Task<List<ModeloAutorizacionDTO>> ListarModeloAutorizacion(bool MostrarInactivos, string baseDatos);
        Task<ModeloAutorizacionDTO> ObtenerModeloAutorizacion(int IdModeloAutorizacion, string baseDatos);
        Task<List<ModeloAutorizacionAutorDTO>> ObtenerAutorModeloAutorizacion(int IdModeloAutorizacion, string baseDatos);
        Task<List<ModeloAutorizacionModuloDTO>> ObtenerModuloModeloAutorizacion(int IdModeloAutorizacion, string baseDatos);
        Task<List<ModeloAutorizacionEtapaDTO>> ObtenerEtapaModeloAutorizacion(int IdModeloAutorizacion, string baseDatos);
        Task<List<ModeloAutorizacionCondicionDTO>> ObtenerCondicionModeloAutorizacion(int IdModeloAutorizacion, string baseDatos);
    }
}
