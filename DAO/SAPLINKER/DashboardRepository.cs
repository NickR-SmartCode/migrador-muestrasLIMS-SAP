using DTO;
using INTERFACES.Repositories;
using Microsoft.Data.SqlClient;
using Sap.Data.Hana;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Text;

namespace DAO.SAPLINKER
{
    public class DashboardRepository : IDashboardRepository
    {
        private readonly Conexion _conexion;

        public DashboardRepository(Conexion conexion)
        {
            _conexion = conexion;
        }

    

        protected TValue? ObtenerValorSql<TValue>(SqlDataReader reader, string columnName)
        {
            int ordinal;
            try { ordinal = reader.GetOrdinal(columnName); } catch { return default; }
            if (reader.IsDBNull(ordinal)) return default;
            try { return (TValue)Convert.ChangeType(reader.GetValue(ordinal), typeof(TValue)); } catch { return default; }
        }

        protected TValue? ObtenerValorHana<TValue>(HanaDataReader reader, string columnName)
        {
            int ordinal;
            try { ordinal = reader.GetOrdinal(columnName); } catch { return default; }
            if (reader.IsDBNull(ordinal)) return default;
            try { return (TValue)Convert.ChangeType(reader.GetValue(ordinal), typeof(TValue)); } catch { return default; }
        }



        #region SQL Server - LIMS

        public async Task<List<LimsRawDataDTO>> ObtenerMuestrasLimsRawAsync(int anio, string bd)
        {
            var lista = new List<LimsRawDataDTO>(80000);

            string sql = @"
        SELECT SAMPLE_NUMBER, ANALYSIS, COST_ITEM_TL, CUSTOMER, REPLACE(PROJECT, '-', '') AS 'PROJECT', CHANGED_ON 
        FROM View_Muestras_LIMS_SAP
        WHERE CHANGED_ON >= @fechaIni AND CHANGED_ON <= @fechaFin";

            DateTime fechaIni = new DateTime(anio, 1, 1);
            DateTime fechaFin = new DateTime(anio, 12, 31, 23, 59, 59);

            using (var conn = _conexion.CrearCnLims(bd))
            {
                await conn.OpenAsync();
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@fechaIni", fechaIni);
                    cmd.Parameters.AddWithValue("@fechaFin", fechaFin);

                    using (var reader = await cmd.ExecuteReaderAsync(CommandBehavior.SequentialAccess))
                    {
                        int ordSample = reader.GetOrdinal("SAMPLE_NUMBER");
                        int ordAnalysis = reader.GetOrdinal("ANALYSIS");
                        int ordCI = reader.GetOrdinal("COST_ITEM_TL");
                        int ordCust = reader.GetOrdinal("CUSTOMER");
                        int ordProj = reader.GetOrdinal("PROJECT");
                        int ordDate = reader.GetOrdinal("CHANGED_ON");

                        while (await reader.ReadAsync())
                        {
                            lista.Add(new LimsRawDataDTO
                            {
                                SampleNumber = reader.GetInt32(ordSample),
                                Analysis = reader.IsDBNull(ordAnalysis) ? "" : reader.GetInt32(ordAnalysis).ToString(),
                                CostItemTl = reader.IsDBNull(ordCI) ? (int?)null : reader.GetInt32(ordCI),
                                Customer = reader.IsDBNull(ordCust) ? 0 : reader.GetInt32(ordCust),
                                Project = reader.IsDBNull(ordProj) ? "" : reader.GetString(ordProj),
                                FechaEmision = reader.IsDBNull(ordDate) ? (DateTime?)null : reader.GetDateTime(ordDate)
                            });
                        }
                    }
                }
            }
            return lista;
        }


        #endregion

        #region SAP HANA

        public async Task<List<SapCustomerMasterDTO>> ObtenerClientesMaestrosSapAsync(string bd)
        {
            var lista = new List<SapCustomerMasterDTO>();
            string sql = @"SELECT ""U_LIMSReferenceNumber"", ""CardCode"", ""CardName"" 
                           FROM OCRD WHERE ""U_LIMSReferenceNumber"" IS NOT NULL";

            using (var conn = _conexion.CrearConexionHana(bd))
            {
                await conn.OpenAsync();
                using (var cmd = new HanaCommand(sql, conn))
                using (var reader = (HanaDataReader)await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        lista.Add(new SapCustomerMasterDTO
                        {
                            LimsReference = Convert.ToInt64(ObtenerValorHana<object>(reader, "U_LIMSReferenceNumber")),
                            CardCode = ObtenerValorHana<string>(reader, "CardCode") ?? "",
                            CardName = ObtenerValorHana<string>(reader, "CardName") ?? ""
                        });
                    }
                }
            }
            return lista;
        }

        public async Task<List<SapItemMasterDTO>> ObtenerArticulosMaestrosSapAsync(string bd)
        {
            var lista = new List<SapItemMasterDTO>();
            string sql = @"SELECT ""U_IntegrationCode"", ""ItemCode"", ""ItemName"" 
                           FROM OITM WHERE ""U_IntegrationCode"" IS NOT NULL";

            using (var conn = _conexion.CrearConexionHana(bd))
            {
                await conn.OpenAsync();
                using (var cmd = new HanaCommand(sql, conn))
                using (var reader = (HanaDataReader)await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        lista.Add(new SapItemMasterDTO
                        {
                            IntegrationCode = ObtenerValorHana<string>(reader, "U_IntegrationCode") ?? "",
                            ItemCode = ObtenerValorHana<string>(reader, "ItemCode") ?? "",
                            ItemName = ObtenerValorHana<string>(reader, "ItemName") ?? ""
                        });
                    }
                }
            }
            return lista;
        }

        public async Task<List<SapIntegrationDataDTO>> ObtenerVincualcionesSapRawAsync(int anio, string bd)
        {
            var lista = new List<SapIntegrationDataDTO>();

            string sql = @"
            SELECT 
                T0.""DocEntry"", 
                T0.""DocNum"" AS ""OrderDocNum"", 
                T0.""DocStatus"", 
                T0.""DocCur"", 
                T0.""DocRate"",
                T1.""U_SMC_MUESTRAS_LIMS"" AS ""SampleNumber"",
                T2.""U_IntegrationCode"", 
                T1.""LineTotal"",
                T1.""LineNum"" AS ""LineaBase"",
                Draft.""DocEntry"" AS ""BorradorDocEntry"",
                -- Lógica de Cascada: Si hay factura firme, manda; si no, el borrador.
                COALESCE(Firm.""InvDocNum"", Draft.""InvDocNum"") AS ""FinalInvDocNum"",
                COALESCE(Firm.""InvMes"", Draft.""InvMes"") AS ""FinalInvMes"",

                CASE 
                    WHEN Draft.""BaseEntry"" IS NOT NULL OR Firm.""BaseEntry"" IS NOT NULL THEN 1 
                    ELSE 0 
                END AS ""IsFacturado"",

                 CASE 
                    WHEN Firm.""BaseEntry"" IS NOT NULL THEN 1
                    WHEN Draft.""BaseEntry"" IS NOT NULL THEN 2
                    ELSE 0 
                END AS ""TipoFT""

            FROM ORDR T0
            INNER JOIN RDR1 T1 ON T0.""DocEntry"" = T1.""DocEntry""
            INNER JOIN OITM T2 ON T1.""ItemCode"" = T2.""ItemCode""

            -- Subconsulta para Borradores de Factura (ODRF)
            LEFT JOIN (
                SELECT L.""BaseEntry"", MAX(I.""DocEntry"") AS ""DocEntry"", L.""BaseLine"", MAX(I.""DocNum"") AS ""InvDocNum"", MAX(MONTH(I.""DocDate"")) AS ""InvMes""
                FROM ODRF I 
                INNER JOIN DRF1 L ON I.""DocEntry"" = L.""DocEntry""
                WHERE L.""BaseType"" = 17 AND I.""ObjType"" = '13' AND I.""DocStatus"" = 'O'
                GROUP BY L.""BaseEntry"", L.""BaseLine""
            ) Draft ON Draft.""BaseEntry"" = T0.""DocEntry"" AND Draft.""BaseLine"" = T1.""LineNum""

            -- Subconsulta para Facturas Firmes (OINV)
            LEFT JOIN (
                SELECT L.""BaseEntry"", L.""BaseLine"", MAX(I.""DocNum"") AS ""InvDocNum"", MAX(MONTH(I.""DocDate"")) AS ""InvMes""
                FROM OINV I 
                INNER JOIN INV1 L ON I.""DocEntry"" = L.""DocEntry""
                WHERE L.""BaseType"" = 17
                GROUP BY L.""BaseEntry"", L.""BaseLine""
            ) Firm ON Firm.""BaseEntry"" = T0.""DocEntry"" AND Firm.""BaseLine"" = T1.""LineNum""

            WHERE YEAR(T0.""DocDate"") = ? AND T1.""U_SMC_MUESTRAS_LIMS"" IS NOT NULL";

            using (var conn = _conexion.CrearConexionHana(bd))
            {
                await conn.OpenAsync();
                using (var cmd = new HanaCommand(sql, conn))
                {
                    cmd.Parameters.Add(new HanaParameter("", anio));
                    using (var reader = (HanaDataReader)await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            string rawSample = ObtenerValorHana<string>(reader, "SampleNumber") ?? "";
                            if (long.TryParse(rawSample, out long sNum))
                            {
                               
                                string finalInvDocNum = reader["FinalInvDocNum"].ToString() ?? "";
                                string finalInvMes = reader["FinalInvMes"].ToString() ?? "";
                             
                                lista.Add(new SapIntegrationDataDTO
                                {
                                    DocEntry = ObtenerValorHana<int>(reader, "DocEntry"),
                                    DocNum = ObtenerValorHana<int>(reader, "OrderDocNum"),
                                    DocStatus = ObtenerValorHana<string>(reader, "DocStatus") ?? "",
                                    SampleNumber = sNum,
                                    U_IntegrationCode = ObtenerValorHana<string>(reader, "U_IntegrationCode") ?? "",
                                    LineTotal = ObtenerValorHana<decimal>(reader, "LineTotal"),
                                    BaseLine = ObtenerValorHana<int>(reader, "LineaBase"),
                                    InvDocNum = finalInvDocNum,
                                    MesFactura = finalInvMes,
                                    EsFirme = ObtenerValorHana<int>(reader, "TipoFT") == 1,
                                    IsInvoiced = ObtenerValorHana<int>(reader, "IsFacturado") == 1,
                                    BorradorDocEntry = ObtenerValorHana<string>(reader, "BorradorDocEntry"),
                                    DocCur = ObtenerValorHana<string>(reader, "DocCur") ?? "SOL",
                                    DocRate = ObtenerValorHana<decimal>(reader, "DocRate")
                                });
                            }
                        }
                    }
                }
            }
            return lista;
        }

        public async Task<List<SapQuotationPriceDTO>> ObtenerPreciosUltimasCotizacionesAsync(string bd)
        {
            var lista = new List<SapQuotationPriceDTO>();

            string sql = @"
        SELECT 
            T0.""CardCode"", 
            T2.""U_IntegrationCode"", 
            T1.""Price""
        FROM OQUT T0
        INNER JOIN QUT1 T1 ON T0.""DocEntry"" = T1.""DocEntry""
        INNER JOIN OITM T2 ON T1.""ItemCode"" = T2.""ItemCode""
        WHERE YEAR(T0.""DocDate"") = YEAR(CURRENT_DATE)
          AND T0.""DocStatus"" IN ('O', 'C')
          AND T2.""U_IntegrationCode"" IS NOT NULL
        ORDER BY T0.""DocDate"" DESC, T0.""DocEntry"" DESC";


            using (var conn = _conexion.CrearConexionHana(bd))
            {
                await conn.OpenAsync();
                using (var cmd = new HanaCommand(sql, conn))
                {
                    using (var reader = (HanaDataReader)await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            lista.Add(new SapQuotationPriceDTO
                            {
                                CardCode = ObtenerValorHana<string>(reader, "CardCode") ?? "",
                                IntegrationCode = ObtenerValorHana<string>(reader, "U_IntegrationCode") ?? "",
                                UnitPrice = ObtenerValorHana<decimal>(reader, "Price")
                            });
                        }
                    }
                }
            }
            return lista;
        }

        #endregion
    }
}
