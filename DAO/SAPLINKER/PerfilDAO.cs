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
    public class PerfilDAO : IPerfilRepository
    {
        private readonly Conexion _conexion;

        public PerfilDAO(Conexion conexion)
        {
            _conexion = conexion;
        }

        public async Task<List<PerfilDTO>> ObtenerPerfiles(bool MostrarInactivos, string baseDatos)
        {

            List<PerfilDTO> lstPerfilDTO = new List<PerfilDTO>();
            await using SqlConnection cn = _conexion.CrearConexion(baseDatos);
            await cn.OpenAsync();

            await using SqlCommand cmd = new("SMC_ListarPerfiles", cn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@MostrarInactivos", MostrarInactivos);

            await using SqlDataReader drd = await cmd.ExecuteReaderAsync();

            while (await drd.ReadAsync())
            {
                PerfilDTO oPerfilDTO = new PerfilDTO();
                oPerfilDTO.IdPerfil = int.Parse(drd["IdPerfil"].ToString());
                oPerfilDTO.Descripcion = drd["Descripcion"].ToString();
                oPerfilDTO.Estado = bool.Parse(drd["Estado"].ToString());
                lstPerfilDTO.Add(oPerfilDTO);
            }
            return lstPerfilDTO;
        }

        public async Task<PerfilDTO> ObtenerDatosxID(int IdPerfil, string baseDatos)
        {

            PerfilDTO oPerfilDTO = new PerfilDTO();


            await using SqlConnection cn = _conexion.CrearConexion(baseDatos);
            await cn.OpenAsync();

            await using SqlCommand cmd = new("SMC_ListarPerfilesxID", cn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@IdPerfil", IdPerfil);

            await using SqlDataReader drd = await cmd.ExecuteReaderAsync();

            while (await drd.ReadAsync())
            {

                oPerfilDTO.IdPerfil = int.Parse(drd["IdPerfil"].ToString());
                oPerfilDTO.Descripcion = drd["Descripcion"].ToString();
                oPerfilDTO.Estado = bool.Parse(drd["Estado"].ToString());
                oPerfilDTO.IdMenuInicial = int.Parse(drd["IdMenuInicial"].ToString());
            }
            return oPerfilDTO;

        }

        public async Task<int> UpdateInsertPerfil(PerfilDTO oPerfilDTO, int idUsuario, string baseDatos)
        {
            await using SqlConnection cn = _conexion.CrearConexion(baseDatos);
            await cn.OpenAsync();

            await using SqlCommand cmd = new("SMC_UpdateInsertPerfiles", cn);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@IdPerfil", oPerfilDTO.IdPerfil);
            cmd.Parameters.AddWithValue("@Descripcion", oPerfilDTO.Descripcion?.ToUpper());
            cmd.Parameters.AddWithValue("@Estado", oPerfilDTO.Estado);
            cmd.Parameters.AddWithValue("@IdMenuInicial", oPerfilDTO.IdMenuInicial);
            cmd.Parameters.AddWithValue("@IdUsuario", idUsuario);

            return await cmd.ExecuteNonQueryAsync();
        }

    }
}
