using DTO;
using System;
using System.Collections.Generic;
using System.Text;

namespace INTERFACES.Repositories
{
    public interface IMenuRepository
    {
        Task<List<MenuDTO>> ObtenerMenuxPerfil(int IdPerfil, string baseDatos);
        Task<List<ItemMenu>> ObtenerSubMenus(int IdMenu, int IdPerfil, string baseDatos);
        Task<List<ItemMenu>> ObtenerMenus(string baseDatos);
        Task<List<ItemMenu>> ObtenerMenuCompleto(string baseDatos);
        Task<List<MenuDTO>> obtenerMenuPerfil(int IdPerfil, string baseDatos);
        Task<int> LimpiarAccesos(int idPerfil, int idUsuario, string baseDatos);
        Task<int> InsertarAcceso(int idPerfil, int idUsuario, int idMenu, string baseDatos);
    }
}
