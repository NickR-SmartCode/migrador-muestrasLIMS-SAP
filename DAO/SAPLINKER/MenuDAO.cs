using DTO;
using INTERFACES.Repositories;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Transactions;

namespace DAO.SAPLINKER
{
    public class MenuDAO : IMenuRepository
    {
        private readonly Conexion _conexion;
        public MenuDAO(Conexion conexion)
        {
            _conexion = conexion;
        }

        public async Task<List<MenuDTO>> ObtenerMenuxPerfil(int IdPerfil, string baseDatos)
        {
            List<MenuDTO> lstMenuDTO = new List<MenuDTO>();

            try
            {
                await using SqlConnection cn = _conexion.CrearConexion(baseDatos);
                await cn.OpenAsync();

                await using SqlCommand cmd = new("SMC_ObtenerMenuxPerfil", cn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@IdPerfil", IdPerfil);

                await using SqlDataReader drd = await cmd.ExecuteReaderAsync();

                while (await drd.ReadAsync())
                {
                    MenuDTO oMenuDTO = new MenuDTO();
                    oMenuDTO.IdMenu = Convert.ToInt32(drd["IdMenu"]);
                    oMenuDTO.IdMenuPadre = Convert.ToInt32(drd["IdMenuPadre"]);
                    oMenuDTO.Descripcion = drd["Descripcion"].ToString();
                    oMenuDTO.Controller = drd["Controller"].ToString();
                    oMenuDTO.Estado = Convert.ToBoolean(drd["Estado"]);
                    oMenuDTO.IdPerfil = Convert.ToInt32(drd["IdPerfil"]);
                    lstMenuDTO.Add(oMenuDTO);
                }
                return lstMenuDTO;
            }
            catch
            {
                throw;
            }

        }

        public async Task<List<ItemMenu>> ObtenerSubMenus(int IdMenu, int IdPerfil, string baseDatos)
        {
            List<ItemMenu> lstItemMenu = new List<ItemMenu>();

            try
            {
                await using SqlConnection cn = _conexion.CrearConexion(baseDatos);
                await cn.OpenAsync();

                await using SqlCommand cmd = new("SMC_ObtenerSubMenuxIdMenu", cn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@IdMenu", IdMenu);
                cmd.Parameters.AddWithValue("@IdPerfil", IdPerfil);

                await using SqlDataReader drd = await cmd.ExecuteReaderAsync();

                while (await drd.ReadAsync())
                {
                    ItemMenu oItemMenu = new ItemMenu
                    {
                        IdMenu = Convert.ToInt32(drd["IdMenu"]),
                        IdMenuPadre = Convert.ToInt32(drd["IdMenuPadre"]),
                        Descripcion = drd["Descripcion"].ToString(),
                        Controller = drd["Controller"].ToString(),
                        Estado = Convert.ToBoolean(drd["Estado"]),
                        IdPerfil = Convert.ToInt32(drd["IdPerfil"]),
                        EsSubMenu = Convert.ToBoolean(drd["EsSubMenu"]),
                        Action = drd["Action"].ToString()
                    };

                    lstItemMenu.Add(oItemMenu);
                }
                return lstItemMenu;
            }
            catch
            {
                throw;
            }
        }
        public async Task<List<MenuDTO>> obtenerMenuPerfil(int IdPerfil, string baseDatos)
        {
            List<MenuDTO> listaAcceso = new List<MenuDTO>();

            try
            {
                await using SqlConnection cn = _conexion.CrearConexion(baseDatos);
                await cn.OpenAsync();

                await using SqlCommand cmd = new("SMC_AccesoXPerfil", cn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@IdPerfil", IdPerfil);

                await using SqlDataReader drd = await cmd.ExecuteReaderAsync();

                while (await drd.ReadAsync())
                {
                    MenuDTO oSeg_Rol = new MenuDTO();
                    oSeg_Rol.IdMenu = int.Parse(drd["idMenu"].ToString());
                    oSeg_Rol.Descripcion = drd["Descripcion"].ToString();
                    oSeg_Rol.Estado = bool.Parse(drd["Estado"].ToString());
                    listaAcceso.Add(oSeg_Rol);
                }
                return listaAcceso;
            }
            catch
            {
                throw;
            }        
        }

        public async Task<int> LimpiarAccesos(int idPerfil, int idUsuario, string baseDatos)
        {
            await using SqlConnection cn = _conexion.CrearConexion(baseDatos);
            await cn.OpenAsync();

            await using SqlCommand cmd = new("SMC_LimpiarAccesos", cn);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@IdPerfil", idPerfil);
            cmd.Parameters.AddWithValue("@IdUsuario", idUsuario);

            return await cmd.ExecuteNonQueryAsync();
        }

        public async Task<int> InsertarAcceso(int idPerfil, int idUsuario, int idMenu, string baseDatos)
        {
            await using SqlConnection cn = _conexion.CrearConexion(baseDatos);
            await cn.OpenAsync();

            await using SqlCommand cmd = new("SMC_GrabarAcceso", cn);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@IdPerfil", idPerfil);
            cmd.Parameters.AddWithValue("@IdUsuario", idUsuario);
            cmd.Parameters.AddWithValue("@IdMenu", idMenu);

            return await cmd.ExecuteNonQueryAsync();
        }

        public async Task<List<ItemMenu>> ObtenerMenus(string baseDatos)
        {
            List<ItemMenu> lstItemMenu = new List<ItemMenu>();

            await using SqlConnection cn = _conexion.CrearConexion(baseDatos);
            await cn.OpenAsync();

            await using SqlCommand cmd = new("SMC_ListarMenus", cn);
            cmd.CommandType = CommandType.StoredProcedure;

            await using SqlDataReader drd = await cmd.ExecuteReaderAsync();

            while (await drd.ReadAsync())
            {
                ItemMenu oItemMenu = new ItemMenu();

                oItemMenu.IdMenu = int.Parse(drd["IdMenu"].ToString());
                oItemMenu.IdMenuPadre = int.Parse(drd["IdMenuPadre"].ToString());
                oItemMenu.Descripcion = drd["Descripcion"].ToString();
                oItemMenu.Controller = drd["Controller"].ToString();
                oItemMenu.Action = drd["Action"].ToString();
                oItemMenu.MenuPadre = drd["MenuPadre"].ToString();

                lstItemMenu.Add(oItemMenu);
            }
            return lstItemMenu;
        }
        public async Task<List<ItemMenu>> ObtenerMenuCompleto(string baseDatos)
        {
            List<ItemMenu> lstItemMenu = new List<ItemMenu>();

            try
            {
                await using SqlConnection cn = _conexion.CrearConexion(baseDatos);
                await cn.OpenAsync();

                await using SqlCommand cmd = new("SMC_ListarMenuCompleto", cn);
                cmd.CommandType = CommandType.StoredProcedure;

                await using SqlDataReader drd = await cmd.ExecuteReaderAsync();

                while (await drd.ReadAsync())
                {
                    ItemMenu oItemMenu = new ItemMenu();

                    oItemMenu.IdMenu = int.Parse(drd["IdMenu"].ToString());
                    oItemMenu.IdMenuPadre = int.Parse(drd["IdMenuPadre"].ToString());
                    oItemMenu.Descripcion = drd["Descripcion"].ToString();

                    lstItemMenu.Add(oItemMenu);
                }
                return lstItemMenu;
            }
            catch
            {
                throw;
            }
        }
    }
}
