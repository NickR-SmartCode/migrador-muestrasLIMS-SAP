using DTO;
using INTERFACES.Repositories;
using Microsoft.Data.SqlClient;

namespace DAO.SAPLINKER
{
    
        public class LogsAutomatizacionDAO : DAOBase<SMC_LogAutomatizacionDTO>, ILogsAutomatizcacionRepository
        {
            public LogsAutomatizacionDAO(Conexion conexion)
                : base(conexion, "SMC_Logs_Automatizacion") { }

            protected override SMC_LogAutomatizacionDTO Mapear(SqlDataReader reader)
            {
                return new SMC_LogAutomatizacionDTO
                {
                    Id = ObtenerValor<int>(reader, "Id"),
                    Fecha_Ejecucion = ObtenerValor<DateTime>(reader, "Fecha_Ejecucion"),
                    SampleNumber = ObtenerValor<long>(reader, "SampleNumber"),
                    Project = ObtenerValor<long?>(reader, "Project"),
                    Customer = ObtenerValor<long?>(reader, "Customer"),
                    Analysis = ObtenerValor<string>(reader, "Analysis") ?? "",
                    CostItemTl = ObtenerValor<long?>(reader, "CostItemTl"),
                    CardCodeSAP = ObtenerValor<string>(reader, "CardCodeSAP") ?? "",
                    Estado = ObtenerValor<string>(reader, "Estado") ?? "",
                    Mensaje = ObtenerValor<string>(reader, "Mensaje") ?? "",
                    DocNumSAP = ObtenerValor<int?>(reader, "DocNumSAP"),
                    SeriesSAP = ObtenerValor<int?>(reader, "SeriesSAP")
                };
            }
        }
    }