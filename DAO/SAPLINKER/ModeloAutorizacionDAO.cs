using DTO;
using INTERFACES.Repositories;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace DAO.SAPLINKER
{
    public class ModeloAutorizacionDAO : IModeloAutorizacionRepository
    {
        private readonly Conexion _conexion;
        public ModeloAutorizacionDAO(Conexion conexion)
        {
            _conexion = conexion;
        }
        public async Task<List<ModuloDTO>> ObtenerModulos(string baseDatos)
        {

            List<ModuloDTO> lstModuloDTO = new List<ModuloDTO>();
            await using SqlConnection cn = _conexion.CrearConexion(baseDatos);
            await cn.OpenAsync();

            await using SqlCommand cmd = new("SMC_ListarModulos", cn);
            cmd.CommandType = CommandType.StoredProcedure;

            await using SqlDataReader drd = await cmd.ExecuteReaderAsync();

            while (await drd.ReadAsync())
            {
                ModuloDTO oModuloDTO = new ModuloDTO();
                oModuloDTO.Id = int.Parse(drd["Id"].ToString());
                oModuloDTO.Descripcion = drd["Descripcion"].ToString();
                oModuloDTO.DebeExistirModelo = bool.Parse(drd["DebeExistirModelo"].ToString());
                lstModuloDTO.Add(oModuloDTO);
            }
            return lstModuloDTO;
        }

        public async Task<int> UpdateInsertModeloAutorizacion(ModeloAutorizacionDTO oModeloAutorizacionDTO, int IdUsuario, string baseDatos)
        {
            await using SqlConnection cn = _conexion.CrearConexion(baseDatos);
            await cn.OpenAsync();

            await using SqlCommand cmd = new("SMC_UpdateInsertModeloAutorizacion", cn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@IdModeloAutorizacion", oModeloAutorizacionDTO.IdModeloAutorizacion);
            cmd.Parameters.AddWithValue("@NombreModelo", oModeloAutorizacionDTO.NombreModelo);
            cmd.Parameters.AddWithValue("@DescripcionModelo", oModeloAutorizacionDTO.DescripcionModelo);
            cmd.Parameters.AddWithValue("@IncluirTodosUsuarios", oModeloAutorizacionDTO.IncluirTodosUsuarios);
            cmd.Parameters.AddWithValue("@Estado", oModeloAutorizacionDTO.Estado);
            cmd.Parameters.AddWithValue("@IdUsuario", IdUsuario);

            var result = await cmd.ExecuteScalarAsync();
            return Convert.ToInt32(result);
        }

        public async Task<int> InsertModeloAutorizacionAutor(ModeloAutorizacionAutorDTO oModeloAutorizacionAutorDTO, string baseDatos)
        {
            await using SqlConnection cn = _conexion.CrearConexion(baseDatos);
            await cn.OpenAsync();

            await using SqlCommand cmd = new("SMC_InsertModeloAutorizacionAutor", cn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@IdModeloAutorizacion", oModeloAutorizacionAutorDTO.IdModeloAutorizacion);
            cmd.Parameters.AddWithValue("@IdAutor", oModeloAutorizacionAutorDTO.IdAutor);

            return await cmd.ExecuteNonQueryAsync();
        }

        public async Task<int> InsertModeloAutorizacionEtapa(ModeloAutorizacionEtapaDTO oModeloAutorizacionEtapaDTO, string baseDatos)
        {
            await using SqlConnection cn = _conexion.CrearConexion(baseDatos);
            await cn.OpenAsync();

            await using SqlCommand cmd = new("SMC_InsertModeloAutorizacionEtapa", cn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@IdModeloAutorizacion", oModeloAutorizacionEtapaDTO.IdModeloAutorizacion);
            cmd.Parameters.AddWithValue("@IdEtapa", oModeloAutorizacionEtapaDTO.IdEtapa);

            return await cmd.ExecuteNonQueryAsync();
        }

        public async Task<int> InsertModeloAutorizacionModulo(ModeloAutorizacionModuloDTO oModeloAutorizacionModuloDTO, string baseDatos)
        {
            await using SqlConnection cn = _conexion.CrearConexion(baseDatos);
            await cn.OpenAsync();

            await using SqlCommand cmd = new("SMC_InsertModeloAutorizacionModulo", cn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@IdModeloAutorizacion", oModeloAutorizacionModuloDTO.IdModeloAutorizacion);
            cmd.Parameters.AddWithValue("@IdModulo", oModeloAutorizacionModuloDTO.IdModulo);

            return await cmd.ExecuteNonQueryAsync();
        }

        public async Task<int> InsertModeloAutorizacionCondicion(ModeloAutorizacionCondicionDTO oModeloAutorizacionCondicionDTO, string baseDatos)
        {
            await using SqlConnection cn = _conexion.CrearConexion(baseDatos);
            await cn.OpenAsync();

            await using SqlCommand cmd = new("SMC_InsertModeloAutorizacionCondicion", cn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@IdModeloAutorizacion", oModeloAutorizacionCondicionDTO.IdModeloAutorizacion);
            cmd.Parameters.AddWithValue("@Condicion", oModeloAutorizacionCondicionDTO.Condicion);

            return await cmd.ExecuteNonQueryAsync();
        }

        public async Task<List<ModeloAutorizacionDTO>> ListarModeloAutorizacion(bool MostrarInactivos, string baseDatos)
        {

            List<ModeloAutorizacionDTO> lstModeloAutorizacionDTO = new List<ModeloAutorizacionDTO>();
            await using SqlConnection cn = _conexion.CrearConexion(baseDatos);
            await cn.OpenAsync();

            await using SqlCommand cmd = new("SMC_ListarModeloAutorizacion", cn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@MostrarInactivos", MostrarInactivos);

            await using SqlDataReader drd = await cmd.ExecuteReaderAsync();

            while (await drd.ReadAsync())
            {
                ModeloAutorizacionDTO oModeloAutorizacionDTO = new ModeloAutorizacionDTO();
                oModeloAutorizacionDTO.IdModeloAutorizacion = int.Parse(drd["Id"].ToString());
                oModeloAutorizacionDTO.NombreModelo = drd["NombreModelo"].ToString();
                oModeloAutorizacionDTO.DescripcionModelo = drd["DescripcionModelo"].ToString();
                oModeloAutorizacionDTO.IncluirTodosUsuarios = bool.Parse(drd["IncluirTodosUsuarios"].ToString());
                oModeloAutorizacionDTO.Estado = bool.Parse(drd["Estado"].ToString());
                lstModeloAutorizacionDTO.Add(oModeloAutorizacionDTO);
            }
            return lstModeloAutorizacionDTO;
        }

        public async Task<ModeloAutorizacionDTO> ObtenerModeloAutorizacion(int IdModeloAutorizacion, string baseDatos)
        {

            ModeloAutorizacionDTO oModeloAutorizacionDTO = new ModeloAutorizacionDTO();
            await using SqlConnection cn = _conexion.CrearConexion(baseDatos);
            await cn.OpenAsync();

            await using SqlCommand cmd = new("SMC_ListarModeloAutorizacionxID", cn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@IdModeloAutorizacion", IdModeloAutorizacion);

            await using SqlDataReader drd = await cmd.ExecuteReaderAsync();

            while (await drd.ReadAsync())
            {
                oModeloAutorizacionDTO.IdModeloAutorizacion = int.Parse(drd["Id"].ToString());
                oModeloAutorizacionDTO.NombreModelo = drd["NombreModelo"].ToString();
                oModeloAutorizacionDTO.DescripcionModelo = drd["DescripcionModelo"].ToString();
                oModeloAutorizacionDTO.IncluirTodosUsuarios = bool.Parse(drd["IncluirTodosUsuarios"].ToString());
                oModeloAutorizacionDTO.Estado = bool.Parse(drd["Estado"].ToString());
            }
            return oModeloAutorizacionDTO;
        }
        public async Task<List<ModeloAutorizacionAutorDTO>> ObtenerAutorModeloAutorizacion(int IdModeloAutorizacion, string baseDatos)
        {

            List<ModeloAutorizacionAutorDTO> lstModeloAutorizacionAutorDTO = new List<ModeloAutorizacionAutorDTO>();
            await using SqlConnection cn = _conexion.CrearConexion(baseDatos);
            await cn.OpenAsync();

            await using SqlCommand cmd = new("SMC_ListarEtapaAutorizacionAutorxID", cn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@IdModeloAutorizacion", IdModeloAutorizacion);

            await using SqlDataReader drd = await cmd.ExecuteReaderAsync();

            while (await drd.ReadAsync())
            {
                ModeloAutorizacionAutorDTO oModeloAutorizacionAutorDTO = new ModeloAutorizacionAutorDTO();
                oModeloAutorizacionAutorDTO.IdModeloAutorizacion = int.Parse(drd["IdModeloAutorizacion"].ToString());
                oModeloAutorizacionAutorDTO.IdAutor = int.Parse(drd["IdUsuario"].ToString());
                lstModeloAutorizacionAutorDTO.Add(oModeloAutorizacionAutorDTO);
            }
            return lstModeloAutorizacionAutorDTO;
        }

        public async Task<List<ModeloAutorizacionModuloDTO>> ObtenerModuloModeloAutorizacion(int IdModeloAutorizacion, string baseDatos)
        {

            List<ModeloAutorizacionModuloDTO> lstModeloAutorizacionModuloDTO = new List<ModeloAutorizacionModuloDTO>();
            await using SqlConnection cn = _conexion.CrearConexion(baseDatos);
            await cn.OpenAsync();

            await using SqlCommand cmd = new("SMC_ListarEtapaAutorizacionModuloxID", cn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@IdModeloAutorizacion", IdModeloAutorizacion);

            await using SqlDataReader drd = await cmd.ExecuteReaderAsync();

            while (await drd.ReadAsync())
            {
                ModeloAutorizacionModuloDTO oModeloAutorizacionModuloDTO = new ModeloAutorizacionModuloDTO();
                oModeloAutorizacionModuloDTO.IdModeloAutorizacion = int.Parse(drd["IdModeloAutorizacion"].ToString());
                oModeloAutorizacionModuloDTO.IdModulo = int.Parse(drd["IdModulo"].ToString());
                lstModeloAutorizacionModuloDTO.Add(oModeloAutorizacionModuloDTO);
            }
            return lstModeloAutorizacionModuloDTO;
        }

        public async Task<List<ModeloAutorizacionEtapaDTO>> ObtenerEtapaModeloAutorizacion(int IdModeloAutorizacion, string baseDatos)
        {

            List<ModeloAutorizacionEtapaDTO> lstModeloAutorizacionEtapaDTO = new List<ModeloAutorizacionEtapaDTO>();
            await using SqlConnection cn = _conexion.CrearConexion(baseDatos);
            await cn.OpenAsync();

            await using SqlCommand cmd = new("SMC_ListarEtapaAutorizacionEtapaxID", cn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@IdModeloAutorizacion", IdModeloAutorizacion);

            await using SqlDataReader drd = await cmd.ExecuteReaderAsync();

            while (await drd.ReadAsync())
            {
                ModeloAutorizacionEtapaDTO oModeloAutorizacionEtapaDTO = new ModeloAutorizacionEtapaDTO();
                oModeloAutorizacionEtapaDTO.IdModeloAutorizacion = int.Parse(drd["IdModeloAutorizacion"].ToString());
                oModeloAutorizacionEtapaDTO.IdEtapa = int.Parse(drd["IdEtapa"].ToString());
                lstModeloAutorizacionEtapaDTO.Add(oModeloAutorizacionEtapaDTO);
            }
            return lstModeloAutorizacionEtapaDTO;
        }

        public async Task<List<ModeloAutorizacionCondicionDTO>> ObtenerCondicionModeloAutorizacion(int IdModeloAutorizacion, string baseDatos)
        {

            List<ModeloAutorizacionCondicionDTO> lstModeloAutorizacionCondicionDTO = new List<ModeloAutorizacionCondicionDTO>();
            await using SqlConnection cn = _conexion.CrearConexion(baseDatos);
            await cn.OpenAsync();

            await using SqlCommand cmd = new("SMC_ListarEtapaAutorizacionCondicionxID", cn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@IdModeloAutorizacion", IdModeloAutorizacion);

            await using SqlDataReader drd = await cmd.ExecuteReaderAsync();

            while (await drd.ReadAsync())
            {
                ModeloAutorizacionCondicionDTO oModeloAutorizacionCondicionDTO = new ModeloAutorizacionCondicionDTO();
                oModeloAutorizacionCondicionDTO.IdModeloAutorizacion = int.Parse(drd["IdModeloAutorizacion"].ToString());
                oModeloAutorizacionCondicionDTO.Condicion = (drd["Condicion"].ToString());
                lstModeloAutorizacionCondicionDTO.Add(oModeloAutorizacionCondicionDTO);
            }
            return lstModeloAutorizacionCondicionDTO;
        }
    }
}
