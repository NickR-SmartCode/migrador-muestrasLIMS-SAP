using DTO;
using System;
using System.Collections.Generic;
using System.Text;

namespace INTERFACES.Services
{
    public interface IMenuService
    {
        Task<List<MenuDTO>> ObtenerMenuxPerfil(int IdPerfil, string baseDatos);
        Task<List<ItemMenu>> ObtenerMenus(string baseDatos);
        Task<List<ItemMenu>> ObtenerMenuCompleto(string baseDatos);
        Task<List<MenuDTO>> obtenerMenuPerfil(int IdPerfil, string baseDatos);
        Task<bool> GrabarAccesos(List<int> accesos, int idPerfil, int idUsuario, string baseDatos);
    }
}
