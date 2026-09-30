using DTO;
using DTO.SAPB1;
using INTERFACES.Repositories.SAPB1;
using Sap.Data.Hana;
using System;
using System.Collections.Generic;
using System.Text;

namespace DAO.SAPLINKER.SAPB1
{
    public class PedidosDAO : HANADAOBase<PedidoSapDTO>, IPedidosSAPRepository
    {
        public PedidosDAO(Conexion conexion) : base(conexion, "ORDR") { }

        protected override PedidoSapDTO Mapear(HanaDataReader reader)
        {
            return new PedidoSapDTO
            {
                DocEntry = ObtenerValor<int>(reader, "DocEntry"),
                DocNum = ObtenerValor<int>(reader, "DocNum"),
                Series = ObtenerValor<double>(reader, "Series"),
                CardCode = ObtenerValor<string>(reader, "CardCode") ?? "",
                CardName = ObtenerValor<string>(reader, "CardName") ?? "",
                DocDate = ObtenerValor<DateTime>(reader, "DocDate"),
                DocTotal = ObtenerValor<decimal>(reader, "DocTotal"),
                DocCur = ObtenerValor<string>(reader, "DocCur") ?? "",
                DocStatus = ObtenerValor<string>(reader, "DocStatus") ?? "",
                NumFactura = ObtenerValor<int?>(reader, "NumFactura"),
                SerieFactura = ObtenerValor<string?>(reader, "SerieFactura"),
                DocRate = ObtenerValor<decimal>(reader, "DocRate"),
                U_ProjectoLims = ObtenerValor<string>(reader, "U_ProjectoLims") ?? "",
                GroupNum = ObtenerValor<string>(reader, "GroupNum") ?? ""
            };
        }

        public async new Task<ResultadoPaginado<PedidoSapDTO>>
            ListarPaginadoAsync(ListarPaginadoPeticion peticion, string bd, string orderBy = "T0.\"DocEntry\" DESC")
        {
            var resultado = new ResultadoPaginado<PedidoSapDTO>();
            int offset = (peticion.Pagina - 1) * peticion.Cantidad;


            List<string> condiciones = new() { "1=1" };
            if (!string.IsNullOrWhiteSpace(peticion.Busqueda))
                condiciones.Add("(T0.\"DocNum\" LIKE ? OR T0.\"CardName\" LIKE ? OR T0.\"CardCode\" LIKE ?)");

            if (!string.IsNullOrWhiteSpace(peticion.Estado))
                condiciones.Add("T0.\"DocStatus\" = ?");


            string whereSql = string.Join(" AND ", condiciones);


            string sqlData = $@"
                SELECT T0.""DocEntry"", T0.""Series"", T0.""DocNum"", T0.""CardCode"", T0.""CardName"", 
                       T0.""DocDate"", T0.""DocTotal"", T0.""DocCur"", T0.""DocStatus"",
                       (SELECT MAX(I.""DocNum"") 
            FROM OINV I INNER JOIN INV1 L ON I.""DocEntry"" = L.""DocEntry"" 
            WHERE L.""BaseEntry"" = T0.""DocEntry"" AND L.""BaseType"" = 17) as ""NumFactura"",

           (SELECT MAX(I.""Series"") 
            FROM OINV I INNER JOIN INV1 L ON I.""DocEntry"" = L.""DocEntry"" 
            WHERE L.""BaseEntry"" = T0.""DocEntry"" AND L.""BaseType"" = 17) as ""SerieFactura""
    
                
                FROM ORDR T0
                WHERE {whereSql}
                ORDER BY {orderBy}
                LIMIT {peticion.Cantidad} OFFSET {offset}";

            using var conn = CrearConexion("");
            await conn.OpenAsync();

            void AddParameters(HanaCommand cmd)
            {
                if (!string.IsNullOrWhiteSpace(peticion.Busqueda))
                {
                    cmd.Parameters.Add(new HanaParameter("", $"%{peticion.Busqueda}%"));
                    cmd.Parameters.Add(new HanaParameter("", $"%{peticion.Busqueda}%"));
                    cmd.Parameters.Add(new HanaParameter("", $"%{peticion.Busqueda}%"));
                }
                if (!string.IsNullOrWhiteSpace(peticion.Estado))
                {
                    cmd.Parameters.Add(new HanaParameter("", peticion.Estado));
                }
            }


            string sqlCount = $@"SELECT COUNT(*) FROM ORDR T0 WHERE {whereSql}";
            using (var cmdCount = new HanaCommand(sqlCount, conn))
            {
                AddParameters(cmdCount);
                resultado.TotalFiltrado = Convert.ToInt32(await cmdCount.ExecuteScalarAsync());
            }

            using (var cmd = new HanaCommand(sqlData, conn))
            {
                AddParameters(cmd);
                using var reader = (HanaDataReader)await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync()) resultado.Data.Add(Mapear(reader));
            }

            return resultado;
        }

        public async Task<PedidoSapDTO> ObtenerPorIdAsync(int docEntry)
        {

            string sqlData = $@"
                SELECT T0.""DocEntry"", T0.""Series"", T0.""DocNum"", T0.""CardCode"", T0.""CardName"", 
                        T0.""GroupNum"",
                       T0.""DocDate"", T0.""DocTotal"", T0.""DocCur"", T0.""DocStatus"",  T0.""DocRate"",
                       (SELECT MAX(I.""DocNum"") 
            FROM OINV I INNER JOIN INV1 L ON I.""DocEntry"" = L.""DocEntry"" 
            WHERE L.""BaseEntry"" = T0.""DocEntry"" AND L.""BaseType"" = 17) as ""NumFactura"",

           (SELECT MAX(I.""Series"") 
            FROM OINV I INNER JOIN INV1 L ON I.""DocEntry"" = L.""DocEntry"" 
            WHERE L.""BaseEntry"" = T0.""DocEntry"" AND L.""BaseType"" = 17) as ""SerieFactura""
    
                
                FROM ORDR T0
                WHERE T0.""DocEntry"" = ?";
            using var conn = CrearConexion("");
            await conn.OpenAsync();
            using var cmd = new HanaCommand(sqlData, conn);
            cmd.Parameters.Add(new HanaParameter("", docEntry));
            using var reader = (HanaDataReader)await cmd.ExecuteReaderAsync();
            PedidoSapDTO res = new();
            if (await reader.ReadAsync()) res = Mapear(reader);
            return res;
        }

        public async Task<List<PedidoDetalleSapDTO>> ObtenerDetalleAsync(int docEntry)
        {
            var detalle = new List<PedidoDetalleSapDTO>();
            string sql = @"SELECT T0.""ItemCode"", T0.""Dscription"",
             T0.""Quantity"", T0.""Price"", T0.""LineTotal"", T0.""LineNum"",
                T0.""LineStatus"",
                    T1.""U_IntegrationCode"" FROM RDR1 T0
                        LEFT JOIN OITM T1 ON T1.""ItemCode"" = T0.""ItemCode""
                        WHERE T0.""DocEntry"" = ?";

            using var conn = CrearConexion("");
            await conn.OpenAsync();
            using var cmd = new HanaCommand(sql, conn);
            cmd.Parameters.Add(new HanaParameter("", docEntry));

            using var reader = (HanaDataReader)await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                detalle.Add(new PedidoDetalleSapDTO
                {
                    ItemCode = reader["ItemCode"].ToString() ?? "",
                    Dscription = reader["Dscription"].ToString() ?? "",
                    Quantity = Convert.ToDecimal(reader["Quantity"]),
                    Price = Convert.ToDecimal(reader["Price"]),
                    LineTotal = Convert.ToDecimal(reader["LineTotal"]),
                    LineNum = Convert.ToInt32(reader["LineNum"]),
                    U_IntegrationCode = reader["U_IntegrationCode"]?.ToString(),
                    LineStatus = reader["LineStatus"]?.ToString() ?? ""
                });
            }
            return detalle;
        }

        public async Task<SapQuotationDTO> ObtenerUltimaCotizacionHanaAsync(string cardCode)
        {
            using var conn = CrearConexion("");
            await conn.OpenAsync();

            string sqlId = @"SELECT MAX(""DocEntry"") FROM OQUT WHERE ""CardCode"" = ?";
            int lastDocEntry = 0;

            using (var cmdId = new HanaCommand(sqlId, conn))
            {
                cmdId.Parameters.Add(new HanaParameter("", cardCode));
                var res = await cmdId.ExecuteScalarAsync();
                if (res == null || res == DBNull.Value) return null;
                lastDocEntry = Convert.ToInt32(res);
            }

            var quotation = new SapQuotationDTO();
            string sqlLines = $@"
     SELECT T0.""DocEntry"", T0.""DocNum"", T0.""Series"", 
                T0.""GroupNum"",
            T1.""ItemCode"", T1.""Price"" as ""UnitPrice"", T1.""VatGroup"" as ""TaxCode"", 
            T1.""WhsCode"" as ""WarehouseCode"", T2.""U_IntegrationCode""
     FROM OQUT T0 
     INNER JOIN QUT1 T1 ON T0.""DocEntry"" = T1.""DocEntry""
        INNER JOIN OITM T2 ON T1.""ItemCode"" = T2.""ItemCode""
     WHERE T0.""DocEntry"" = {lastDocEntry}";

            using (var cmdLines = new HanaCommand(sqlLines, conn))
            {
                using var reader = (HanaDataReader)await cmdLines.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    if (quotation.DocEntry == 0)
                    {
                        quotation.DocEntry = Convert.ToInt32(reader["DocEntry"]);
                        quotation.DocNum = Convert.ToInt32(reader["DocNum"]);
                        quotation.Series = Convert.ToInt32(reader["Series"]);
                        quotation.GroupNum = reader["GroupNum"].ToString() ?? "";
                    }

                    quotation.DocumentLines.Add(new SapOrderLineDTO
                    {
                        ItemCode = reader["ItemCode"].ToString() ?? "",
                        UnitPrice = Convert.ToDecimal(reader["UnitPrice"]),
                        TaxCode = reader["TaxCode"].ToString() ?? "",
                        WarehouseCode = reader["WarehouseCode"].ToString() ?? "",
                        U_IntegrationCode = reader["U_IntegrationCode"]?.ToString() ?? ""
                    });
                }
            }

            return quotation.DocEntry > 0 ? quotation : null;
        }

        public async Task<ResultadoPaginado<PedidoDetalleConCabeceraSapDTO>> ListarDetallesPaginadosAsync(PedidoDetallesListarPaginadosReqDTO peticion)
        {
            var resultado = new ResultadoPaginado<PedidoDetalleConCabeceraSapDTO>();
            int offset = (peticion.Pagina - 1) * peticion.Cantidad;

            List<string> condiciones = new List<string>() { "1=1" };

            if (!string.IsNullOrEmpty(peticion.Cliente))
            {
                condiciones.Add(@"(T2.""CardCode"" LIKE ? OR T2.""CardName"" LIKE ?)");
            }

            if (!string.IsNullOrEmpty(peticion.Busqueda))
            {
                condiciones.Add(@"(T0.""U_SMC_MUESTRAS_LIMS"" LIKE ? OR T1.""ItemName"" LIKE ? OR T0.""ItemCode"" LIKE ? OR T2.""U_ProjectoLims"" LIKE ? OR CAST(T2.""DocNum"" AS VARCHAR) LIKE ?)");
            }

            if (!string.IsNullOrEmpty(peticion.Estado))
            {
                condiciones.Add(@"T2.""DocStatus"" = ?");
            }

            string whereSql = "WHERE " + string.Join(" AND ", condiciones);

            string sql = $@"
        SELECT 
            T0.""ItemCode"", T0.""Dscription"", T0.""Quantity"", T0.""Price"", 
            T0.""LineTotal"", T0.""LineNum"", T0.""LineStatus"", T0.""U_SMC_MUESTRAS_LIMS"",
            T1.""U_IntegrationCode"",               

            /* DATOS CABECERA */
            T2.""DocEntry"", T2.""Series"", T2.""DocNum"", T2.""CardCode"", T2.""CardName"", 
            T2.""DocDate"", T2.""DocTotal"", T2.""DocCur"", T2.""DocStatus"",
            T2.""U_ProjectoLims"", T2.""GroupNum"", T2.""DocRate"",

            /* LÓGICA DE FACTURAS (BORRADOR VS FIRME) */
            COALESCE(Draft.""DocNum"", Firm.""DocNum"") AS ""NumFactura"",
            COALESCE(Draft.""Series"", Firm.""Series"") AS ""SerieFactura"",
            
            CASE WHEN Draft.""DocNum"" IS NOT NULL THEN 1 ELSE 0 END AS ""EsPreliminar"",
            
            CASE 
                WHEN Draft.""DocNum"" IS NOT NULL THEN 'Borrador'
                WHEN Firm.""DocNum"" IS NOT NULL THEN 'Facturado'
                ELSE 'Sin Facturar'
            END AS ""EstadoFacturacion""

        FROM RDR1 T0
        LEFT JOIN OITM T1 ON T1.""ItemCode"" = T0.""ItemCode""
        INNER JOIN ORDR T2 ON T0.""DocEntry"" = T2.""DocEntry""

        LEFT JOIN (
            SELECT L.""BaseEntry"", L.""BaseLine"", MAX(I.""DocNum"") AS ""DocNum"", MAX(I.""Series"") AS ""Series""
            FROM ODRF I 
            INNER JOIN DRF1 L ON I.""DocEntry"" = L.""DocEntry""
            WHERE L.""BaseType"" = 17 AND I.""ObjType"" = '13' AND I.""DocStatus"" = 'O'
            GROUP BY L.""BaseEntry"", L.""BaseLine""
        ) Draft ON Draft.""BaseEntry"" = T0.""DocEntry"" AND Draft.""BaseLine"" = T0.""LineNum""

        LEFT JOIN (
            SELECT L.""BaseEntry"", L.""BaseLine"", MAX(I.""DocNum"") AS ""DocNum"", MAX(I.""Series"") AS ""Series""
            FROM OINV I 
            INNER JOIN INV1 L ON I.""DocEntry"" = L.""DocEntry""
            WHERE L.""BaseType"" = 17
            GROUP BY L.""BaseEntry"", L.""BaseLine""
        ) Firm ON Firm.""BaseEntry"" = T0.""DocEntry"" AND Firm.""BaseLine"" = T0.""LineNum""

        {whereSql}
        ORDER BY T2.""DocEntry"" DESC, T0.""LineNum"" ASC 
        LIMIT {peticion.Cantidad} OFFSET {offset}";

            string sqlCount = $@"
        SELECT COUNT(*) 
        FROM RDR1 T0 
        LEFT JOIN OITM T1 ON T1.""ItemCode"" = T0.""ItemCode""
        INNER JOIN ORDR T2 ON T0.""DocEntry"" = T2.""DocEntry"" 
        {whereSql}";

            using var conn = CrearConexion("");
            await conn.OpenAsync();

            void AddParameters(HanaCommand cmdObj)
            {
                if (!string.IsNullOrEmpty(peticion.Cliente))
                {
                    string c = "%" + peticion.Cliente + "%";
                    cmdObj.Parameters.Add(new HanaParameter("", c)); 
                    cmdObj.Parameters.Add(new HanaParameter("", c)); 
                }

                if (!string.IsNullOrEmpty(peticion.Busqueda))
                {
                    string b = "%" + peticion.Busqueda + "%";
                    cmdObj.Parameters.Add(new HanaParameter("", b)); 
                    cmdObj.Parameters.Add(new HanaParameter("", b)); 
                    cmdObj.Parameters.Add(new HanaParameter("", b)); 
                    cmdObj.Parameters.Add(new HanaParameter("", b)); 
                    cmdObj.Parameters.Add(new HanaParameter("", b)); 
                }

                if (!string.IsNullOrEmpty(peticion.Estado))
                {
                    cmdObj.Parameters.Add(new HanaParameter("", peticion.Estado)); 
                }
            }

            using (var cmdCount = new HanaCommand(sqlCount, conn))
            {
                AddParameters(cmdCount);
                resultado.TotalFiltrado = Convert.ToInt32(await cmdCount.ExecuteScalarAsync());
            }

            using var cmd = new HanaCommand(sql, conn);
            AddParameters(cmd);

            using var reader = (HanaDataReader)await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var cabeceraConDetalle = new PedidoDetalleConCabeceraSapDTO();

                cabeceraConDetalle.Pedido = Mapear(reader);

                cabeceraConDetalle.Linea = new PedidoDetalleSapDTO
                {
                    ItemCode = reader["ItemCode"].ToString() ?? "",
                    Dscription = reader["Dscription"].ToString() ?? "",
                    Quantity = Convert.ToDecimal(reader["Quantity"]),
                    Price = Convert.ToDecimal(reader["Price"]),
                    LineTotal = Convert.ToDecimal(reader["LineTotal"]),
                    LineNum = Convert.ToInt32(reader["LineNum"]),
                    U_IntegrationCode = reader["U_IntegrationCode"]?.ToString(),
                    LineStatus = reader["LineStatus"]?.ToString() ?? "",
                    SampleNumber = reader["U_SMC_MUESTRAS_LIMS"].ToString() ?? "",
                    EsPreliminar = Convert.ToInt32(reader["EsPreliminar"]) == 1,
                    EstadoFacturacion = reader["EstadoFacturacion"].ToString() ?? "Sin Facturar"
                };

                resultado.Data.Add(cabeceraConDetalle);
            }

            return resultado;
        }
        public async Task<List<SapQuotationDTO>> ListarCotizacionesVigentesAnioActualAsync(string cardCode, string bd)
        {
            var lista = new List<SapQuotationDTO>();

            // SQL optimizado para traer todas las cotizaciones del año actual
            // Ordenamos por DocDate y DocEntry DESC para asegurar que la primera sea la más reciente
            string sql = @"
        SELECT 
            T0.""DocEntry"", T0.""DocNum"", T0.""Series"", T0.""GroupNum"", T0.""DocDate"",
            T1.""ItemCode"", T1.""Price"" as ""UnitPrice"", T1.""VatGroup"" as ""TaxCode"", 
            T1.""WhsCode"" as ""WarehouseCode"", T2.""U_IntegrationCode""
        FROM OQUT T0 
        INNER JOIN QUT1 T1 ON T0.""DocEntry"" = T1.""DocEntry""
        INNER JOIN OITM T2 ON T1.""ItemCode"" = T2.""ItemCode""
        WHERE T0.""CardCode"" = ? 
          AND YEAR(T0.""DocDate"") = YEAR(CURRENT_DATE)
          AND T0.""DocStatus"" IN ('O', 'C')
        ORDER BY T0.""DocDate"" DESC, T0.""DocEntry"" DESC";

            using (var conn = CrearConexion(bd))
            {
                await conn.OpenAsync();
                using (var cmd = new HanaCommand(sql, conn))
                {
                    cmd.Parameters.Add(new HanaParameter("", cardCode));
                    using (var reader = (HanaDataReader)await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            int docEntry = Convert.ToInt32(reader["DocEntry"]);
                            var quotation = lista.FirstOrDefault(x => x.DocEntry == docEntry);

                            if (quotation == null)
                            {
                                quotation = new SapQuotationDTO
                                {
                                    DocEntry = docEntry,
                                    DocNum = Convert.ToInt32(reader["DocNum"]),
                                    Series = Convert.ToInt32(reader["Series"]),
                                    GroupNum = reader["GroupNum"].ToString() ?? "",
                                    DocumentLines = new List<SapOrderLineDTO>()
                                };
                                lista.Add(quotation);
                            }

                            quotation.DocumentLines.Add(new SapOrderLineDTO
                            {
                                ItemCode = reader["ItemCode"].ToString() ?? "",
                                UnitPrice = Convert.ToDecimal(reader["UnitPrice"]),
                                TaxCode = reader["TaxCode"].ToString() ?? "",
                                WarehouseCode = reader["WarehouseCode"].ToString() ?? "",
                                U_IntegrationCode = reader["U_IntegrationCode"]?.ToString() ?? ""
                            });
                        }
                    }
                }
            }
            return lista;
        }
    }
}

