using DTO;
using INTERFACES.Repositories;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace DAO.SAPLINKER
{
    public class AprobacionesDAO : IAprobacionesRepository
    {
        private readonly Conexion _conexion;

        public AprobacionesDAO(Conexion conexion)
        {
            _conexion = conexion;
        }
        public async Task<ModuloDTO> ObtenerDatosModulo(int IdModulo, string baseDatos)
        {

            ModuloDTO oModuloDTO = new ModuloDTO();

            await using SqlConnection cn = _conexion.CrearConexion(baseDatos);
            await cn.OpenAsync();

            await using SqlCommand cmd = new("ObtenerDatosxIdModulo", cn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@IdModulo", IdModulo);

            await using SqlDataReader drd = await cmd.ExecuteReaderAsync();

            while (await drd.ReadAsync())
            {
                oModuloDTO.Id = int.Parse(drd["Id"].ToString());
                oModuloDTO.Descripcion = (drd["Descripcion"].ToString());
                oModuloDTO.DebeExistirModelo = bool.Parse(drd["DebeExistirModelo"].ToString());
            }
            return oModuloDTO;

        }
        public async Task<List<AprobacionesDTO>> ObtenerModelosPorAutorDocumento(int IdModulo, int IdAutor, string baseDatos)
        {

            List<AprobacionesDTO> lstAprobacionesDTO = new List<AprobacionesDTO>();
            await using SqlConnection cn = _conexion.CrearConexion(baseDatos);
            await cn.OpenAsync();

            await using SqlCommand cmd = new("SMC_ObtenerModelosPorAutor", cn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@IdModulo", IdModulo);
            cmd.Parameters.AddWithValue("@IdAutor", IdAutor);

            await using SqlDataReader drd = await cmd.ExecuteReaderAsync();

            while (await drd.ReadAsync())
            {
                AprobacionesDTO oAprobacionesDTO = new AprobacionesDTO();
                oAprobacionesDTO.IdModulo = int.Parse(drd["IdModulo"].ToString());
                oAprobacionesDTO.IdEtapa = int.Parse(drd["IdEtapa"].ToString());
                oAprobacionesDTO.AutorizacionesRequeridas = int.Parse(drd["AutorizacionesRequeridas"].ToString());
                oAprobacionesDTO.RechazosRequeridos = int.Parse(drd["RechazosRequeridos"].ToString());
                oAprobacionesDTO.IdModelo = int.Parse(drd["IdModelo"].ToString());
                oAprobacionesDTO.Condicion = (drd["Condicion"].ToString());
                lstAprobacionesDTO.Add(oAprobacionesDTO);
            }
            return lstAprobacionesDTO;
        }

        public async Task<int> ValidarCondicion(int Id, string condicion, string baseDatos)
        {
            await using SqlConnection cn = _conexion.CrearConexion(baseDatos);
            await cn.OpenAsync();

            await using SqlCommand cmd = new(condicion, cn);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@Id", Id);
            var result = await cmd.ExecuteScalarAsync();
            return result != null ? Convert.ToInt32(result) : 0;
        }

        public async Task<int> UpdateModuloAprobarDirecto(int IdModulo, int IdTablaOriginal, string baseDatos)
        {
            await using SqlConnection cn = _conexion.CrearConexion(baseDatos);
            await cn.OpenAsync();

            await using SqlCommand cmd = new("SMC_UpdateModuloAprobarDirecto", cn);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@IdModulo", IdModulo);
            cmd.Parameters.AddWithValue("@IdTablaOriginal", IdTablaOriginal);

            return await cmd.ExecuteNonQueryAsync();
        }

        public async Task<int> UpdateInsertDocumentoAprobacionEtapas(int IdEtapa, int IdModulo, int IdTablaOriginal, string baseDatos)
        {
            await using SqlConnection cn = _conexion.CrearConexion(baseDatos);
            await cn.OpenAsync();

            await using SqlCommand cmd = new("SMC_UpdateInsertModuloAprobacionEtapa", cn);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@IdEtapa", IdEtapa);
            cmd.Parameters.AddWithValue("@IdModulo", IdModulo);
            cmd.Parameters.AddWithValue("@IdTablaOriginal", IdTablaOriginal);

            var result = await cmd.ExecuteScalarAsync();
            return Convert.ToInt32(result);
        }

        public async Task<int> UpdateInsertModuloAprobacionModelo(int IdEtapa, int IdModuloAprobacionEtapa, string baseDatos)
        {
            await using SqlConnection cn = _conexion.CrearConexion(baseDatos);
            await cn.OpenAsync();

            await using SqlCommand cmd = new("SMC_UpdateInsertModuloAprobacionModelo", cn);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@IdEtapa", IdEtapa);
            cmd.Parameters.AddWithValue("@IdModuloAprobacionEtapa", IdModuloAprobacionEtapa);

            return await cmd.ExecuteNonQueryAsync();
        }

        public async Task<List<TurnoAprobarDTO>> ObtenerTurnoAprobadores(int IdModulo, int IdTablaOriginal, string baseDatos)
        {

            List<TurnoAprobarDTO> lstTurnoAprobarDTO = new List<TurnoAprobarDTO>();
            await using SqlConnection cn = _conexion.CrearConexion(baseDatos);
            await cn.OpenAsync();

            await using SqlCommand cmd = new("SMC_ObtenerTurnoAprobadores", cn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@IdModulo", IdModulo);
            cmd.Parameters.AddWithValue("@IdTablaOriginal", IdTablaOriginal);

            await using SqlDataReader drd = await cmd.ExecuteReaderAsync();

            while (await drd.ReadAsync())
            {
                TurnoAprobarDTO oTurnoAprobarDTO = new TurnoAprobarDTO();
                oTurnoAprobarDTO.IdModuloAprobacionModelo = int.Parse(drd["IdModuloAprobacionModelo"].ToString());
                oTurnoAprobarDTO.Usuario = (drd["usuario"].ToString());
                oTurnoAprobarDTO.Nombre = (drd["NombreUsuario"].ToString());
                oTurnoAprobarDTO.Email = (drd["Correo"].ToString());
                oTurnoAprobarDTO.correoEnviado = bool.Parse(drd["CorreoEnviado"].ToString());
                lstTurnoAprobarDTO.Add(oTurnoAprobarDTO);
            }
            return lstTurnoAprobarDTO;
        }

        public async Task<int> MarcarCorreoAprobadorEnviado(int IdModuloAprobacionModelo, int Enviado, string baseDatos)
        {
            await using SqlConnection cn = _conexion.CrearConexion(baseDatos);
            await cn.OpenAsync();

            await using SqlCommand cmd = new("SMC_MarcarCorreoAprobadorEnviado", cn);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@IdModuloAprobacionModelo", IdModuloAprobacionModelo);
            cmd.Parameters.AddWithValue("@Enviado", Enviado);

            return await cmd.ExecuteNonQueryAsync();
        }

    }
}
