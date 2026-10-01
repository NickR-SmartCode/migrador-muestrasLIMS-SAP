using DTO;
using DTO.SAPB1;
using INTERFACES.Repositories;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Identity.Client;
using Sap.Data.Hana;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace DAO.SAPLINKER.SAPB1
{
    public class EstadoMuestrasPreFTDAO : DAOBase<EstadoMuestrasPreFT>, IEstadoMuestrasPreFTRepository
    {


        private readonly Conexion _conexion;
        private readonly IMemoryCache _cache;
        private readonly TimeSpan _cacheDuration = TimeSpan.FromMinutes(10);
        public EstadoMuestrasPreFTDAO(Conexion conexion, IMemoryCache cache) : base(conexion, "View_Muestras_Prefactura_LIMS_SAP")
        {
            _conexion = conexion;
            _cache = cache;
            UsarConexionLIMS(false);
        }

        protected override EstadoMuestrasPreFT Mapear(SqlDataReader reader)
        {
            return new EstadoMuestrasPreFT
            {
                Project = reader["PROJECT"]?.ToString()?.Replace("-", "") ?? string.Empty,
                Customer = reader["CUSTOMER"]?.ToString() ?? string.Empty,
                Cliente = reader["CLIENTE"]?.ToString() ?? string.Empty,
                FechaEmision = Convert.ToDateTime(reader["FECHA_EMISION"] != DBNull.Value ? reader["FECHA_EMISION"] : DateTime.Now),
                NroPF = reader["NRO_PF"]?.ToString() ?? string.Empty,
                SubTotalLineaPF = Convert.ToDecimal(reader["SUB_TOTAL_LINEA"]),
                ValorUnitarioAnaPF = Convert.ToDecimal(reader["VALOR_UNITARIO"]),
                Cantidad = Convert.ToInt32(reader["CANTIDAD"]),
                Description = reader["PRODUCTO"]?.ToString() ?? string.Empty,
                DetalleLinea = reader["DETALLE_PF"]?.ToString() ?? string.Empty,
                TipoServicio = reader["TIPO_SERVICIO"]?.ToString() ?? string.Empty,
                Moneda = reader["MONEDA"]?.ToString() ?? string.Empty,
                Orden = Convert.ToInt32(reader["ITEM_ORDEN"] != DBNull.Value ? reader["ITEM_ORDEN"] : 1)
            };
        }

        public async new Task<ResultadoPaginado<EstadoMuestrasPreFT>> ListarPaginadoAsync(ListarPaginadoPeticion peticion,
 string bd, Action<SqlCommand>? agregarParametrosExtra = null, CancellationToken? ct = null)
        {
            string totalSinFiltrarKey = $"TotalSinFiltrar_{_nombreTabla}_{bd}";
            string busquedaLimpia = peticion.Busqueda?.Trim().ToLower() ?? "";
            string estadoLimpio = peticion.Estado?.ToString() ?? "";

            using var tempCmd = new SqlCommand();
            agregarParametrosExtra?.Invoke(tempCmd);

            var extraParamsKey = new System.Text.StringBuilder();
            foreach (SqlParameter param in tempCmd.Parameters)
            {
                extraParamsKey.Append($"_{param.ParameterName}_{param.Value}");
            }

            string totalFiltradoKey = $"TotalFiltrado_{_nombreTabla}_{bd}_{busquedaLimpia}_{estadoLimpio}{extraParamsKey}";

            bool hasCachedTotalSinFiltrar = _cache.TryGetValue(totalSinFiltrarKey, out int cachedTotalSinFiltrar);
            bool hasCachedTotalFiltrado = _cache.TryGetValue(totalFiltradoKey, out int cachedTotalFiltrado);

            Action<SqlCommand> parametrosConCache = cmd =>
            {
                agregarParametrosExtra?.Invoke(cmd);

                cmd.Parameters.AddWithValue("@CachedTotalSinFiltrar", hasCachedTotalSinFiltrar ? cachedTotalSinFiltrar : -1);
                cmd.Parameters.AddWithValue("@CachedTotalFiltrado", hasCachedTotalFiltrado ? cachedTotalFiltrado : -1);
            };

            UsarConexionLIMS(false);
            var baseList = await base.ListarPaginadoAsync(peticion, bd, parametrosConCache, ct);

            if (!hasCachedTotalSinFiltrar && baseList.TotalSinFiltrar >= 0)
                _cache.Set(totalSinFiltrarKey, baseList.TotalSinFiltrar, TimeSpan.FromMinutes(15));
            else if (hasCachedTotalSinFiltrar)
                baseList.TotalSinFiltrar = cachedTotalSinFiltrar;

            if (!hasCachedTotalFiltrado && baseList.TotalFiltrado >= 0)
                _cache.Set(totalFiltradoKey, baseList.TotalFiltrado, TimeSpan.FromMinutes(5));
            else if (hasCachedTotalFiltrado)
                baseList.TotalFiltrado = cachedTotalFiltrado;


            var listaDeNoEncontrados = new List<(string, string, string)>();
            foreach (var pf in baseList.Data)
            {

                var key =
                 $"Match_{bd}_{pf.NroPF}_{pf.Cliente.Split("-")[0]}_{pf.DetalleLinea.Replace("\n", "")}";
                if (_cache.TryGetValue(key, out (int docNum, int series, bool isDraft) sapDetails))
                {
                    pf.DocNumSAP = sapDetails.docNum;
                    pf.SeriesSAP = sapDetails.series;
                    pf.IsDraftSAP = sapDetails.isDraft;
                }
                else
                {
                    if (!listaDeNoEncontrados.Any(c => c.Item1 == pf.NroPF))
                    {
                        listaDeNoEncontrados.Add((pf.NroPF, pf.Cliente.Split("-")[0], pf.DetalleLinea.Replace("\n", "")));
                    }
                }
            }

            if (listaDeNoEncontrados.Any())
            {

                var tasks = listaDeNoEncontrados.Select(m =>
                    CargarMatchesConSapParaPreFT(m.Item1, m.Item2, m.Item3, bd)
                );

                await Task.WhenAll(tasks);


                foreach (var pf in baseList.Data)
                {


                    var key = $"Match_{bd}_{pf.NroPF}_{pf.Cliente.Split("-")[0]}_{pf.DetalleLinea.Replace("\n", "")}";

                    if (_cache.TryGetValue(key, out (int docNum, int series, bool isDraft) sapDetails))
                    {
                        pf.DocNumSAP = sapDetails.docNum;
                        pf.SeriesSAP = sapDetails.series;
                        pf.IsDraftSAP = sapDetails.isDraft;
                    }
                }
            }
            return baseList;
        }

        #region GESTION DE MATCHES CON SAP

        public async Task CargarMatchesConSapParaPreFT(string nroPF, string clienteRuc, string detalleLinea, string bd)
        {
            string sql = $@"
WITH FirmData AS (
    SELECT 
        'Firm' AS ""Source"",
        I.""DocNum"", 
        I.""Series"", 
        I.""Comments"",
        L.""U_SMC_MUESTRAS_LIMS"",
        L.""U_SMC_RDLIMS1""
    FROM OINV I 
    INNER JOIN INV1 L ON I.""DocEntry"" = L.""DocEntry""
    WHERE I.""DocEntry"" = (
        SELECT TOP 1 T0.""DocEntry"" 
        FROM OINV T0 
        WHERE T0.""Comments"" = ? 
          AND T0.""LicTradNum"" = ?
        ORDER BY T0.""DocEntry"" DESC
    )
    AND L.""BaseType"" = '-1' 
),
DraftData AS (
    SELECT 
        'Draft' AS ""Source"",
          I.""DocNum"", 
        I.""Series"", 
        I.""Comments"",
        L.""U_SMC_MUESTRAS_LIMS"",
        L.""U_SMC_RDLIMS1""
    FROM ODRF I 
    INNER JOIN DRF1 L ON I.""DocEntry"" = L.""DocEntry""
    WHERE I.""DocEntry"" = (
        SELECT TOP 1 T0.""DocEntry"" 
        FROM ODRF T0 
        WHERE T0.""Comments"" = ? 
            AND T0.""LicTradNum"" = ?
          AND T0.""ObjType"" = '13' 
          AND T0.""DocStatus"" = 'O' 
        ORDER BY T0.""DocEntry"" DESC
    )
    AND L.""BaseType"" = '-1' 
)
/* Seleccionamos las columnas una a una para garantizar el nombre en el Reader */
SELECT 
    ""Source"", ""DocNum"", ""Series"", ""U_SMC_MUESTRAS_LIMS"", ""Comments"", ""U_SMC_RDLIMS1"" 
FROM FirmData
UNION ALL
SELECT 
    ""Source"", ""DocNum"", ""Series"", ""U_SMC_MUESTRAS_LIMS"", ""Comments"", ""U_SMC_RDLIMS1""
FROM DraftData
WHERE NOT EXISTS (SELECT 1 FROM FirmData)";

            using var conn = _conexion.CrearConexionHana("");
            await conn.OpenAsync();

            using var cmd = new HanaCommand(sql, conn);
            cmd.Parameters.Add(new HanaParameter("", $"{nroPF}"));
            cmd.Parameters.Add(new HanaParameter("", $"{clienteRuc}"));

            cmd.Parameters.Add(new HanaParameter("", $"{nroPF}"));
            cmd.Parameters.Add(new HanaParameter("", $"{clienteRuc}"));



            using var reader = (HanaDataReader)await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var m = new MatchConSAP
                {
                    Source = reader["Source"]?.ToString() ?? "",
                    DocNum = reader["DocNum"] != DBNull.Value ? Convert.ToInt32(reader["DocNum"]) : 0,
                    Series = reader["Series"] != DBNull.Value ? Convert.ToInt32(reader["Series"]) : 0,
                    U_SMC_MUESTRAS_LIMS = reader["U_SMC_MUESTRAS_LIMS"]?.ToString() ?? "",
                    Comments = reader["Comments"]?.ToString() ?? "",
                    U_SMC_RDLIMS1 = reader.GetValue(reader.GetOrdinal("U_SMC_RDLIMS1"))?.ToString() ?? ""
                };

                var key =
                 $"Match_{bd}_{m.Comments}_{clienteRuc}_{m.U_SMC_RDLIMS1.Replace("\n", "")}";
                _cache.Set(key, (m.DocNum, m.Series, m.Source == "Draft" ? true : false), TimeSpan.FromMinutes(1));
            }

        }

        #endregion



        public async Task<List<EstadoMuestrasPreFT>> ObtenerPrefacturasPlanasPorAnioAsync(int anio, string bd)
        {
            var listaLims = new List<EstadoMuestrasPreFT>();

            DateTime inicioAnio = new DateTime(anio, 1, 1);
            DateTime finAnio = new DateTime(anio, 12, 31);

            DateTime fechaCorteFusion = new DateTime(2026, 10, 1);

            DateTime inicioReal = inicioAnio < fechaCorteFusion ? fechaCorteFusion : inicioAnio;

            string fechaInicioStr = inicioReal.ToString("yyyy-MM-dd");
            string fechaFinStr = finAnio.ToString("yyyy-MM-dd");

            string sqlLims = @"
        SELECT 
            CLIENTE = CAST([RUC_CLI_PF] AS NVARCHAR(300)) + '-' + CAST([RAZ_SOC_CLI_PF] AS NVARCHAR(300)), 
            [CUSTOMER], 
            [PROJECT],
            '' AS PRODUCTO, 
            [NRO_PF],  
            [FEC_EMI_PF] AS FECHA_EMISION, 
            GETDATE() AS FECHA_ENV_SAP, 
            MONEDA_PF AS MONEDA,
            1 AS CANTIDAD, 
            '' AS TIPO_SERVICIO, 
            '' AS DETALLE, 
            [DETALLE_PF], 
            'SRV00001' AS CODIGO,
            [VALOR_UNITARIO_PF] AS VALOR_UNITARIO, 
            [PRECIO_UNITARIO_PF] AS SUB_TOTAL_LINEA,
            [IGV_PF], 
            [SUB_TOTAL_PF], 
            [TOTAL_PF], 
            [ITEM_ORDEN]
        FROM View_Muestras_Prefactura_LIMS_SAP
        WHERE TRY_CONVERT(DATE, FEC_EMI_PF, 103) BETWEEN @fechaInicio AND @fechaFin
        ORDER BY [ITEM_ORDEN]";

            UsarConexionLIMS(true);
            using (var connSql = CrearConexion(bd))
            {
                await connSql.OpenAsync();
                using var cmdLims = new SqlCommand(sqlLims, connSql);
                cmdLims.Parameters.AddWithValue("@fechaInicio", fechaInicioStr);
                cmdLims.Parameters.AddWithValue("@fechaFin", fechaFinStr);

                using var reader = await cmdLims.ExecuteReaderAsync();
                while (await reader.ReadAsync()) listaLims.Add(Mapear(reader));
            }

            if (!listaLims.Any()) return listaLims;

            var sapMatches = new Dictionary<string, List<MatchSAP>>();

            // Consulta HANA filtrada exactamente por el mismo rango de fechas
            string sqlHana = @"
        SELECT 'Firm' AS ""Source"", T0.""DocEntry"", T0.""DocNum"", T0.""Series"", T0.""DocDate"", T0.""DocTotal"", T0.""DocCur"", T0.""DocRate"", T0.""U_ProjectoLims"", T0.""CardCode"", T1.""LicTradNum""
        FROM OINV T0 
        INNER JOIN OCRD T1 ON T0.""CardCode"" = T1.""CardCode"" 
        WHERE T0.""U_ProjectoLims"" IS NOT NULL 
          AND T0.""DocDate"" BETWEEN ? AND ? 
          AND T0.""CANCELED"" = 'N'

        UNION ALL

        SELECT 'Draft' AS ""Source"", T0.""DocEntry"", T0.""DocNum"", T0.""Series"", T0.""DocDate"", T0.""DocTotal"", T0.""DocCur"", T0.""DocRate"", T0.""U_ProjectoLims"", T0.""CardCode"", T1.""LicTradNum""
        FROM ODRF T0 
        INNER JOIN OCRD T1 ON T0.""CardCode"" = T1.""CardCode"" 
        WHERE T0.""ObjType"" = '13' 
          AND T0.""DocStatus"" = 'O' 
          AND T0.""U_ProjectoLims"" IS NOT NULL 
          AND T0.""DocDate"" BETWEEN ? AND ?";

            using (var connHana = _conexion.CrearConexionHana(bd))
            {
                await connHana.OpenAsync();
                using var cmdHana = new HanaCommand(sqlHana, connHana);

                cmdHana.Parameters.Add(new HanaParameter("p1", HanaDbType.Date) { Value = inicioReal });
                cmdHana.Parameters.Add(new HanaParameter("p2", HanaDbType.Date) { Value = finAnio });
                cmdHana.Parameters.Add(new HanaParameter("p3", HanaDbType.Date) { Value = inicioReal });
                cmdHana.Parameters.Add(new HanaParameter("p4", HanaDbType.Date) { Value = finAnio });

                using var readerHana = (HanaDataReader)await cmdHana.ExecuteReaderAsync();
                while (await readerHana.ReadAsync())
                {
                    string projectLims = readerHana["U_ProjectoLims"]?.ToString() ?? "";
                    string cardCode = readerHana["LicTradNum"]?.ToString() ?? "";
                    string key = $"{projectLims}_{cardCode}";

                    var match = new MatchSAP
                    {
                        IsDraft = readerHana["Source"]?.ToString() == "Draft",
                        DocEntry = Convert.ToInt32(readerHana["DocEntry"]),
                        DocNum = Convert.ToInt32(readerHana["DocNum"]),
                        Series = Convert.ToInt32(readerHana["Series"]),
                        DocDate = Convert.ToDateTime(readerHana["DocDate"]),
                        DocTotal = Convert.ToDecimal(readerHana["DocTotal"]),
                        DocCur = readerHana["DocCur"]?.ToString() ?? "SOL",
                        DocRate = readerHana["DocRate"] != DBNull.Value ? Convert.ToDecimal(readerHana["DocRate"]) : 1m
                    };

                    if (!sapMatches.ContainsKey(key)) sapMatches[key] = new List<MatchSAP>();
                    sapMatches[key].Add(match);
                }
            }

            foreach (var pf in listaLims)
            {
                string rucCliente = pf.Cliente?.Split('-')[0] ?? "";
                string sapKey = $"{pf.NroPF}_{rucCliente}";

                if (sapMatches.TryGetValue(sapKey, out var matches))
                {
                    pf.DocumentosSAP = matches;
                }
            }

            return listaLims;
        }
    } 



    public class MatchConSAP
    {
        public string Source { get; set; } = string.Empty;
        public int DocNum { get; set; }
        public int Series { get; set; }
        public string U_SMC_MUESTRAS_LIMS { get; set; } = string.Empty;
        public string Comments { get; set; } = string.Empty;
        public string U_SMC_RDLIMS1 { get; set; } = string.Empty;
    }

}
