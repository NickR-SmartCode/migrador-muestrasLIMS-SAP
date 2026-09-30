using DTO;
using INTERFACES.Repositories;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Transactions;

namespace DAO
{
    public class UsuarioDAO : IUsuarioRepository
    {
        private readonly Conexion _conexion;
        public UsuarioDAO(Conexion conexion)
        {
            _conexion = conexion;
        }

        public async Task<UsuarioDTO> ValidarUsuario(string Usuario, string Password, string baseDatos)
        {
            UsuarioDTO oUsuarioDTO = new UsuarioDTO();

            try
            {
                await using SqlConnection cn = _conexion.CrearConexion(baseDatos);
                await cn.OpenAsync();

                await using SqlCommand cmd = new("SMC_ValidaUsuario", cn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Usuario", Usuario);
                cmd.Parameters.AddWithValue("@Password", Password);

                await using SqlDataReader drd = await cmd.ExecuteReaderAsync();

                while (await drd.ReadAsync())
                {
                    oUsuarioDTO = new UsuarioDTO();
                    oUsuarioDTO.IdUsuario = int.Parse(drd["IdUsuario"].ToString());
                    oUsuarioDTO.Usuario = drd["Usuario"].ToString();
                    oUsuarioDTO.NombreUsuario = drd["NombreUsuario"].ToString();
                    oUsuarioDTO.Correo = (drd["Correo"].ToString());
                    oUsuarioDTO.IdPerfil = int.Parse(drd["IdPerfil"].ToString());
                    oUsuarioDTO.Estado = bool.Parse(drd["Estado"].ToString());
                    oUsuarioDTO.MenuInicio = (drd["MenuInicio"].ToString());
                }
                return oUsuarioDTO;
            }
            catch
            {
                throw;
            }
        }

        public async Task<int> GuardarRefreshToken(int IdUsuario, string Token, string TokenAnterior, string baseDatos)
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

            await using SqlConnection cn = _conexion.CrearConexion(baseDatos);
            await cn.OpenAsync();


            try
            {
                await using SqlCommand cmd = new("SMC_GuardarRefreshToken", cn);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@IdUsuario", IdUsuario);
                cmd.Parameters.AddWithValue("@Token", Token);
                cmd.Parameters.AddWithValue("@TokenAnterior", TokenAnterior);

                object? result = await cmd.ExecuteNonQueryAsync();

                int rpta = Convert.ToInt32(result);

                if (rpta <= 0)
                    throw new Exception("Error desconocido al grabar Acceso");

               
                scope.Complete();

                return rpta;
            }
            catch
            {
                throw;
            }
        }

        public async Task<List<UsuarioDTO>> ObtenerUsuarios(bool mostrarInactivos, string baseDatos)
        {
            List<UsuarioDTO> lstUsuarioDTO = new();


            await using SqlConnection cn = _conexion.CrearConexion(baseDatos);
            await cn.OpenAsync();

            await using SqlCommand cmd = new("SMC_ListarUsuarios", cn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@MostrarInactivos", mostrarInactivos);

            await using SqlDataReader drd = await cmd.ExecuteReaderAsync();

            while (await drd.ReadAsync())
            {
                UsuarioDTO oUsuarioDTO = new()
                {
                    IdUsuario = drd.GetInt32(drd.GetOrdinal("IdUsuario")),
                    Usuario = drd["Usuario"] as string,
                    NombreUsuario = drd["NombreUsuario"] as string,
                    IdPerfil = drd.GetInt32(drd.GetOrdinal("IdPerfil")),
                    NombrePerfil = drd["NombrePerfil"] as string,
                    Estado = drd.GetBoolean(drd.GetOrdinal("Estado")),
                    Correo = drd["Correo"] as string
                };

                lstUsuarioDTO.Add(oUsuarioDTO);
            }
            return lstUsuarioDTO;

        }


        public async Task<int> UpdateInsertUsuario(UsuarioDTO usuarioDTO, int idUsuario, string baseDatos)
        {
            var transactionOptions = new TransactionOptions
            {
                IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted,
                Timeout = TimeSpan.FromSeconds(60)
            };

            await using SqlConnection cn = _conexion.CrearConexion(baseDatos);
            await cn.OpenAsync();

            using var scope = new TransactionScope(
                TransactionScopeOption.Required,
                transactionOptions,
                TransactionScopeAsyncFlowOption.Enabled);

            try
            {
                await using SqlCommand cmd = new("SMC_UpdateInsertUsuarios", cn);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@IdUsuario", usuarioDTO.IdUsuario);
                cmd.Parameters.AddWithValue("@NombreUsuario", usuarioDTO.NombreUsuario?.ToUpper());
                cmd.Parameters.AddWithValue("@Usuario", usuarioDTO.Usuario?.ToUpper());
                cmd.Parameters.AddWithValue("@Contraseña", usuarioDTO.Password);
                cmd.Parameters.AddWithValue("@IdPerfil", usuarioDTO.IdPerfil);
                cmd.Parameters.AddWithValue("@Correo", usuarioDTO.Correo?.ToUpper());
                cmd.Parameters.AddWithValue("@Estado", usuarioDTO.Estado);
                cmd.Parameters.AddWithValue("@IdUsuarioRegistro", idUsuario);

                object? result = await cmd.ExecuteScalarAsync();

                int rpta = Convert.ToInt32(result);

                if (rpta <= 0)
                    throw new Exception("Error desconocido al crear Usuario");

                scope.Complete();

                return rpta;
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("UQ_Usuario_Usuario"))
                    throw new Exception("El usuario ingresado ya existe.");

                if (ex.Message.Contains("UQ_Usuario_Correo"))
                    throw new Exception("El correo ingresado ya está registrado.");

                throw;
            }
        }


        public async Task<UsuarioDTO> ObtenerDatosxID(int IdUsuario, string baseDatos)
        {
            UsuarioDTO oUsuarioDTO = new UsuarioDTO();

            try
            {
                await using SqlConnection cn = _conexion.CrearConexion(baseDatos);
                await cn.OpenAsync();

                await using SqlCommand cmd = new("SMC_ListarUsuariosxID", cn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@IdUsuario", IdUsuario);

                await using SqlDataReader drd = await cmd.ExecuteReaderAsync();

                while (await drd.ReadAsync())
                {
                    oUsuarioDTO.IdUsuario = drd.GetInt32(drd.GetOrdinal("IdUsuario"));
                    oUsuarioDTO.Usuario = drd["Usuario"].ToString();
                    oUsuarioDTO.NombreUsuario = drd["NombreUsuario"].ToString();
                    oUsuarioDTO.IdPerfil = drd.GetInt32(drd.GetOrdinal("IdPerfil"));
                    oUsuarioDTO.NombrePerfil = drd["NombrePerfil"].ToString();
                    oUsuarioDTO.Estado = drd.GetBoolean(drd.GetOrdinal("Estado"));
                    oUsuarioDTO.Correo = drd["Correo"].ToString();
                }
                return oUsuarioDTO;
            }
            catch
            {
                throw;
            }
        }
        public async Task<UsuarioDTO> ValidarRefreshToken(string refreshToken, string baseDatos)
        {
            UsuarioDTO oUsuarioDTO = new UsuarioDTO();

            try
            {
                await using SqlConnection cn = _conexion.CrearConexion(baseDatos);
                await cn.OpenAsync();

                await using SqlCommand cmd = new("SMC_ValidarRefreshToken", cn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Token", refreshToken);

                await using SqlDataReader drd = await cmd.ExecuteReaderAsync();

                while (await drd.ReadAsync())
                {
                    oUsuarioDTO = new UsuarioDTO();
                    oUsuarioDTO.IdUsuario = int.Parse(drd["IdUsuario"].ToString());
                    oUsuarioDTO.Usuario = drd["Usuario"].ToString();
                    oUsuarioDTO.NombreUsuario = drd["NombreUsuario"].ToString();
                    oUsuarioDTO.Correo = (drd["Correo"].ToString());
                    oUsuarioDTO.IdPerfil = int.Parse(drd["IdPerfil"].ToString());
                    oUsuarioDTO.Estado = bool.Parse(drd["Estado"].ToString());
                    oUsuarioDTO.MenuInicio = (drd["MenuInicio"].ToString());
                }
                return oUsuarioDTO;
            }
            catch
            {
                throw;
            }
        }

      
    }
}
