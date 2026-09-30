using DTO;
using INTERFACES.Repositories;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace DAO.SAPLINKER
{
    public class EtapaAutorizacionDAO : IEtapaAutorizacionRepository
    {
        private readonly Conexion _conexion;

        public EtapaAutorizacionDAO(Conexion conexion)
        {
            _conexion = conexion;
        }
        public async Task<List<EtapaAutorizacionDTO>> ListarEtapaAutorizacion(bool MostrarInactivos, string baseDatos)
        {

            List<EtapaAutorizacionDTO> lstEtapaAutorizacionDTO = new List<EtapaAutorizacionDTO>();
            await using SqlConnection cn = _conexion.CrearConexion(baseDatos);
            await cn.OpenAsync();

            await using SqlCommand cmd = new("SMC_ListarEtapaAutorizacion", cn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@MostrarInactivos", MostrarInactivos);

            await using SqlDataReader drd = await cmd.ExecuteReaderAsync();

            while (await drd.ReadAsync())
            {
                EtapaAutorizacionDTO oEtapaAutorizacionDTO = new EtapaAutorizacionDTO();
                oEtapaAutorizacionDTO.IdEtapaAutorizacion = int.Parse(drd["Id"].ToString());
                oEtapaAutorizacionDTO.NombreEtapa = drd["NombreEtapa"].ToString();
                oEtapaAutorizacionDTO.DescripcionEtapa = drd["DescripcionEtapa"].ToString();
                oEtapaAutorizacionDTO.AutorizacionesRequeridas = int.Parse(drd["AutorizacionesRequeridas"].ToString());
                oEtapaAutorizacionDTO.RechazosRequeridos = int.Parse(drd["RechazosRequeridos"].ToString());
                oEtapaAutorizacionDTO.Estado = bool.Parse(drd["Estado"].ToString());
                lstEtapaAutorizacionDTO.Add(oEtapaAutorizacionDTO);
            }
            return lstEtapaAutorizacionDTO;
        }

        public async Task<int> UpdateInsertEtapaAutorizacion(EtapaAutorizacionDTO oEtapaAutorizacionDTO, int IdUsuario, string baseDatos)
        {
            await using SqlConnection cn = _conexion.CrearConexion(baseDatos);
            await cn.OpenAsync();

            await using SqlCommand cmd = new("SMC_UpdateInsertEtapaAutorizacion", cn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@IdEtapaAutorizacion", oEtapaAutorizacionDTO.IdEtapaAutorizacion);
            cmd.Parameters.AddWithValue("@NombreEtapa", oEtapaAutorizacionDTO.NombreEtapa.ToUpper());
            cmd.Parameters.AddWithValue("@DescripcionEtapa", oEtapaAutorizacionDTO.DescripcionEtapa.ToUpper());
            cmd.Parameters.AddWithValue("@AutorizacionesRequeridas", oEtapaAutorizacionDTO.AutorizacionesRequeridas);
            cmd.Parameters.AddWithValue("@RechazosRequeridos", oEtapaAutorizacionDTO.RechazosRequeridos);
            cmd.Parameters.AddWithValue("@Estado", oEtapaAutorizacionDTO.Estado);
            cmd.Parameters.AddWithValue("@IdUsuario", IdUsuario);

            var result = await cmd.ExecuteScalarAsync();
            return Convert.ToInt32(result);
        }

        public async Task<int> UpdateInsertEtapaAutorizacionDetalle(EtapaAutorizacionDetalleDTO oEtapaAutorizacionDetalleDTO, string baseDatos)
        {
            await using SqlConnection cn = _conexion.CrearConexion(baseDatos);
            await cn.OpenAsync();

            await using SqlCommand cmd = new("SMC_InsertEtapaAutorizacionDetalle", cn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@IdEtapaAutorizacion", oEtapaAutorizacionDetalleDTO.IdEtapaAutorizacion);
            cmd.Parameters.AddWithValue("@IdUsuario", oEtapaAutorizacionDetalleDTO.IdUsuario);
            return await cmd.ExecuteNonQueryAsync();
        }
        public async Task<EtapaAutorizacionDTO> ObtenerEtapaAutorizacion(int IdEtapaAutorizacion, string baseDatos)
        {

            EtapaAutorizacionDTO oEtapaAutorizacionDTO = new EtapaAutorizacionDTO();
            await using SqlConnection cn = _conexion.CrearConexion(baseDatos);
            await cn.OpenAsync();

            await using SqlCommand cmd = new("SMC_ListarEtapaAutorizacionxID", cn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@IdEtapaAutorizacion", IdEtapaAutorizacion);

            await using SqlDataReader drd = await cmd.ExecuteReaderAsync();

            while (await drd.ReadAsync())
            {
                oEtapaAutorizacionDTO.IdEtapaAutorizacion = int.Parse(drd["Id"].ToString());
                oEtapaAutorizacionDTO.NombreEtapa = drd["NombreEtapa"].ToString();
                oEtapaAutorizacionDTO.DescripcionEtapa = drd["DescripcionEtapa"].ToString();
                oEtapaAutorizacionDTO.AutorizacionesRequeridas = int.Parse(drd["AutorizacionesRequeridas"].ToString());
                oEtapaAutorizacionDTO.RechazosRequeridos = int.Parse(drd["RechazosRequeridos"].ToString());
                oEtapaAutorizacionDTO.Estado = bool.Parse(drd["Estado"].ToString());
            }
            return oEtapaAutorizacionDTO;
        }

        public async Task<List<EtapaAutorizacionDetalleDTO>> ObtenerDetallaEtapaAutorizacion(int IdEtapaAutorizacion, string baseDatos)
        {

            List<EtapaAutorizacionDetalleDTO> lstEtapaAutorizacionDetalleDTO = new List<EtapaAutorizacionDetalleDTO>();
            await using SqlConnection cn = _conexion.CrearConexion(baseDatos);
            await cn.OpenAsync();

            await using SqlCommand cmd = new("SMC_ListarEtapaAutorizacionDetallexID", cn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@IdEtapaAutorizacion", IdEtapaAutorizacion);

            await using SqlDataReader drd = await cmd.ExecuteReaderAsync();

            while (await drd.ReadAsync())
            {
                EtapaAutorizacionDetalleDTO oEtapaAutorizacionDetalleDTO = new EtapaAutorizacionDetalleDTO();
                oEtapaAutorizacionDetalleDTO.IdEtapaAutorizacion = int.Parse(drd["IdEtapaAutorizacion"].ToString());
                oEtapaAutorizacionDetalleDTO.IdUsuario = int.Parse(drd["IdUsuario"].ToString());
                lstEtapaAutorizacionDetalleDTO.Add(oEtapaAutorizacionDetalleDTO);
            }
            return lstEtapaAutorizacionDetalleDTO;
        }

    }
}
