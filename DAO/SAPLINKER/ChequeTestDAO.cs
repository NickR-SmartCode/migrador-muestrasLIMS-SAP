using DTO;
using INTERFACES.Repositories;
using INTERFACES.Services;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace DAO.SAPLINKER
{
    public class ChequeTestDAO : IChequeTestRepository
    {
        private readonly Conexion _conexion;

        public ChequeTestDAO(Conexion conexion)
        {
            _conexion = conexion;
        }
        public async Task<int> UpdateInsertCheque(ChequeTestDTO oChequeTestDTO, int IdUsuario, string baseDatos)
        {
            await using SqlConnection cn = _conexion.CrearConexion(baseDatos);
            await cn.OpenAsync();

            await using SqlCommand cmd = new("SMC_UpdateInsertCheque", cn);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@IdCheque", oChequeTestDTO.IdCheque);
            cmd.Parameters.AddWithValue("@Nombre", oChequeTestDTO.Nombre.ToUpper());
            cmd.Parameters.AddWithValue("@Fecha", oChequeTestDTO.Fecha);
            cmd.Parameters.AddWithValue("@Moneda", oChequeTestDTO.Moneda);
            cmd.Parameters.AddWithValue("@Monto", oChequeTestDTO.Monto);
            cmd.Parameters.AddWithValue("@IdUsuario", IdUsuario);

            var result = await cmd.ExecuteScalarAsync();
            return Convert.ToInt32(result);
        }

        public async Task<List<ChequeTestDTO>> ObtenerCheques(string baseDatos)
        {
            List<ChequeTestDTO> lstChequeTestDTO = new List<ChequeTestDTO>();
            await using SqlConnection cn = _conexion.CrearConexion(baseDatos);
            await cn.OpenAsync();

            await using SqlCommand cmd = new("SMC_ListarCheques", cn);
            cmd.CommandType = CommandType.StoredProcedure;

            await using SqlDataReader drd = await cmd.ExecuteReaderAsync();

            while (await drd.ReadAsync())
            {
                ChequeTestDTO oChequeTestDTO = new ChequeTestDTO();
                oChequeTestDTO.IdCheque = int.Parse(drd["IdCheque"].ToString());
                oChequeTestDTO.Nombre = drd["Nombre"].ToString();
                oChequeTestDTO.Fecha = DateTime.Parse(drd["Fecha"].ToString());
                oChequeTestDTO.Moneda = (drd["Moneda"].ToString());
                oChequeTestDTO.Monto = decimal.Parse(drd["Monto"].ToString());
                oChequeTestDTO.EstadoAprobacion = drd["EstadoAprobacion"] == DBNull.Value ? 0 : int.Parse(drd["EstadoAprobacion"].ToString());
                lstChequeTestDTO.Add(oChequeTestDTO);
            }
            return lstChequeTestDTO;
        }

        public async Task<List<ChequeTestDTO>> ObtenerChequesParaAprobacion(int IdUsuario, string baseDatos)
        {
            List<ChequeTestDTO> lstChequeTestDTO = new List<ChequeTestDTO>();
            await using SqlConnection cn = _conexion.CrearConexion(baseDatos);
            await cn.OpenAsync();

            await using SqlCommand cmd = new("SMC_ListarAprobacionesCheques", cn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@IdUsuario", IdUsuario);

            await using SqlDataReader drd = await cmd.ExecuteReaderAsync();

            while (await drd.ReadAsync())
            {
                ChequeTestDTO oChequeTestDTO = new ChequeTestDTO();
                oChequeTestDTO.IdCheque = int.Parse(drd["IdCheque"].ToString());
                oChequeTestDTO.Nombre = drd["Nombre"].ToString();
                oChequeTestDTO.Fecha = DateTime.Parse(drd["Fecha"].ToString());
                oChequeTestDTO.Moneda = (drd["Moneda"].ToString());
                oChequeTestDTO.Monto = decimal.Parse(drd["Monto"].ToString());
                oChequeTestDTO.IdModuloAprobacionModelo = int.Parse(drd["IdModuloAprobacionModelo"].ToString());
                lstChequeTestDTO.Add(oChequeTestDTO);
            }
            return lstChequeTestDTO;
        }

    }
}
