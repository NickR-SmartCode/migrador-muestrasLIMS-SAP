using DTO;
using INTERFACES.Repositories;
using INTERFACES.Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Transactions;
using static System.Collections.Specialized.BitVector32;

namespace SERVICES
{
    public class MenuService : IMenuService
    {
        private readonly IMenuRepository _repository;

        public MenuService(IMenuRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<MenuDTO>> ObtenerMenuxPerfil(int IdPerfil, string BaseDatos)
        {
            List<MenuDTO> menus = await _repository.ObtenerMenuxPerfil(IdPerfil, BaseDatos);
            foreach (var menu in menus)
            {
                menu.Items = await ObtenerSubMenuRecursivo(menu.IdMenu, IdPerfil, BaseDatos);
            }
            return menus;
        }
        private async Task<IList<ItemMenu>> ObtenerSubMenuRecursivo(int idMenu, int IdPerfil, string BaseDatos)
        {
            var subMenus = await _repository.ObtenerSubMenus(idMenu, IdPerfil, BaseDatos);

            foreach (var sub in subMenus)
            {
                if (sub.EsSubMenu)
                {
                    sub.ItemsSubMenu = await ObtenerSubMenuRecursivo(sub.IdMenu, IdPerfil, BaseDatos);
                }
            }
            return subMenus;
        }
        public async Task<List<ItemMenu>> ObtenerMenus(string BaseDatos) => await _repository.ObtenerMenus(BaseDatos);
        public async Task<List<ItemMenu>> ObtenerMenuCompleto(string BaseDatos) => await _repository.ObtenerMenuCompleto(BaseDatos);
        public async Task<List<MenuDTO>> obtenerMenuPerfil(int IdPerfil, string BaseDatos) => await _repository.obtenerMenuPerfil(IdPerfil, BaseDatos);

        public async Task<bool> GrabarAccesos(List<int> accesos, int idPerfil, int idUsuario, string baseDatos)
        {
            var transactionOptions = new TransactionOptions
            {
                IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted,
                Timeout = TimeSpan.FromSeconds(60)
            };

            using var scope = new TransactionScope(
                TransactionScopeOption.Required,
                transactionOptions,
                TransactionScopeAsyncFlowOption.Enabled);

            int rpta = await _repository.LimpiarAccesos(idPerfil, idUsuario, baseDatos);

            if (rpta <= 0)
                throw new Exception("Error al limpiar accesos");

            foreach (var idMenu in accesos)
            {
                int rpta2 = await _repository.InsertarAcceso(idPerfil, idUsuario, idMenu, baseDatos);

                if (rpta2 <= 0)
                    throw new Exception("Error al grabar acceso");
            }

            scope.Complete();
            return true;
        }
    }

}
