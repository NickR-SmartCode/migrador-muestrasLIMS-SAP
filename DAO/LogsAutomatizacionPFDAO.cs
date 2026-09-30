using DTO;
using DTO.SAPB1;
using INTERFACES;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Text;

namespace DAO
{
    public class LogsAutomatizacionPFDAO : DAOBase<LogsAutomatizacionPF> , ILogsAutomatizacionPFRepository
    {
        public LogsAutomatizacionPFDAO(Conexion conexion) : base(conexion, "SMC_Logs_AutomatizacionPF") { }
     
        protected override LogsAutomatizacionPF Mapear(SqlDataReader reader)
        {
            return new LogsAutomatizacionPF
            {
                Id = ObtenerValor<int>(reader, "Id"),
                Fecha_Ejecucion = ObtenerValor<DateTime>(reader, "Fecha_Ejecucion"),
                Project = ObtenerValor<long?>(reader, "Project"),
                Customer = ObtenerValor<long?>(reader, "Customer"),
                CardCodeSAP = ObtenerValor<string>(reader, "CardCodeSAP") ?? "",
                CardName = ObtenerValor<string>(reader, "CardName") ?? "",
                Estado = ObtenerValor<string>(reader, "Estado") ?? "",
                Mensaje = ObtenerValor<string>(reader, "Mensaje") ?? "",
                DocNumSAP = ObtenerValor<int?>(reader, "DocNumSAP"),
                SeriesSAP = ObtenerValor<int?>(reader, "SeriesSAP"),
                Detalle = ObtenerValor<string>(reader, "Detalle") ?? "",
                NroPF = ObtenerValor<string>(reader, "NroPF") ?? ""
            };
        }
        public async Task GuardarLogAutomatizacionAsync(EstadoMuestrasPreFT preFT, string estado, string mensaje, 
            int? docNum, int? series, int? docEntry, int idUsuario, string bd)
        {
            UsarConexionLIMS(false);
            using var conn = CrearConexion(bd);
            await conn.OpenAsync();
            string sql = @"INSERT INTO 
            SMC_Logs_AutomatizacionPF
            (Customer, CardCodeSAP,
            Estado, Mensaje, DocNumSAP, IdUsuario, SeriesSAP, DocEntrySAP, NroPF, Detalle, CardName)
            VALUES (@c, @cc, @est, @msg, @dn, @u, @sSap, @deSap, @nroPF, @detalle, @cardName)
                ";
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@c", preFT.Customer ?? string.Empty);
            cmd.Parameters.AddWithValue("@cc", preFT.RucLIMSParaSAP ?? string.Empty);
            cmd.Parameters.AddWithValue("@est", estado);
            cmd.Parameters.AddWithValue("@msg", mensaje);
            cmd.Parameters.AddWithValue("@dn", (object?)docNum ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@u", idUsuario);
            cmd.Parameters.AddWithValue("@sSap", (object?)series ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@deSap", (object?)docEntry ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@nroPF", (object?)preFT.NroPF ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@detalle", (object?)preFT.DetalleLinea ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@cardName", (object?)preFT.RazonSocialLIMSParaSAP ?? DBNull.Value);
            await cmd.ExecuteNonQueryAsync();
        }
    }
}
