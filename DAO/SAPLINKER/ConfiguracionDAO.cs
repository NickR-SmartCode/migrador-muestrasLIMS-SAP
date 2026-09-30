using DTO;
using INTERFACES.Repositories;
using Microsoft.Data.SqlClient;

using System.Data;

namespace DAO.SAPLINKER
{
    public class ConfiguracionDAO : IConfiguracionRepository
    {
        private readonly Conexion _conexion;
        public ConfiguracionDAO(Conexion conexion)
        {
            _conexion = conexion;
        }

        public async Task<ConfiguracionDTO> ObtenerConfiguracion(string BaseDatos)
        {
            ConfiguracionDTO oConfiguracionDTO = new ConfiguracionDTO();
            await using SqlConnection cn = _conexion.CrearConexion(BaseDatos);
            await cn.OpenAsync();

            await using Microsoft.Data.SqlClient.SqlCommand cmd = new("SMC_ObtenerConfiguracion", cn);
            cmd.CommandType = CommandType.StoredProcedure;

            await using SqlDataReader drd = await cmd.ExecuteReaderAsync();

            while (await drd.ReadAsync())
            {
                oConfiguracionDTO.RUC = (drd["RUC"].ToString());
                oConfiguracionDTO.RazonSocial = drd["RazonSocial"].ToString();
                oConfiguracionDTO.ServidorSMTP = drd["ServidorSMTP"].ToString();
                oConfiguracionDTO.PuertoSMTP = int.Parse(drd["PuertoSMTP"].ToString());
                oConfiguracionDTO.SSLSMTP = bool.Parse(drd["SSLSMTP"].ToString());
                oConfiguracionDTO.EmailSMTP = (drd["EmailSMTP"].ToString());
                oConfiguracionDTO.PasswordSMTP = (drd["PasswordSMTP"].ToString());
            }
            return oConfiguracionDTO;

        }

        public async Task<int> UpdateConfiguracion(ConfiguracionDTO oConfiguracionDTO, int IdUsuario, string baseDatos)
        {
            await using SqlConnection cn = _conexion.CrearConexion(baseDatos);
            await cn.OpenAsync();

            await using Microsoft.Data.SqlClient.SqlCommand cmd = new("SMC_UpdateConfiguracion", cn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@RUC", oConfiguracionDTO.RUC);
            cmd.Parameters.AddWithValue("@RazonSocial", oConfiguracionDTO.RazonSocial.ToUpper());
            cmd.Parameters.AddWithValue("@ServidorSMTP", oConfiguracionDTO.ServidorSMTP);
            cmd.Parameters.AddWithValue("@PuertoSMTP", oConfiguracionDTO.PuertoSMTP);
            cmd.Parameters.AddWithValue("@SSLSMTP", oConfiguracionDTO.SSLSMTP);
            cmd.Parameters.AddWithValue("@EmailSMTP", oConfiguracionDTO.EmailSMTP);
            cmd.Parameters.AddWithValue("@PasswordSMTP", oConfiguracionDTO.PasswordSMTP);
            cmd.Parameters.AddWithValue("@IdUsuario", IdUsuario);

            return await cmd.ExecuteNonQueryAsync();
        }
    }
}
