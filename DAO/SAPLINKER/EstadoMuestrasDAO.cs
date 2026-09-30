using DAO;
using DAO.SAPLINKER;
using DTO;
using DTO.SAPB1;
using INTERFACES.Repositories;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Caching.Memory;
using Sap.Data.Hana;
using System.Data;
using System.Diagnostics;
using System.Text.RegularExpressions;

public class EstadoMuestrasDAO : DAOBase<EstadoMuestraDTO>, IEstadoMuestrasRepository
{
    private readonly Conexion _conexion;
    private readonly IMemoryCache _cache;
    private readonly TimeSpan _cacheDuration = TimeSpan.FromMinutes(10);

    public EstadoMuestrasDAO(Conexion conexion, IMemoryCache cache) : base(conexion, "View_Muestras_LIMS_SAP")
    {
        _conexion = conexion;
        _cache = cache;
        UsarConexionLIMS(true);
    }

    protected override EstadoMuestraDTO Mapear(SqlDataReader reader)
    {
        return new EstadoMuestraDTO
        {
            TextId = ObtenerValor<string>(reader, "TEXT_ID") ?? string.Empty,
            SampleNumber = ObtenerValor<long?>(reader, "SAMPLE_NUMBER"),
            Description = ObtenerValor<string>(reader, "DESCRIPTION") ?? string.Empty,
            CostItemTl = ObtenerValor<long?>(reader, "COST_ITEM_TL") ?? 0,
            TestListName = ObtenerValor<string>(reader, "TEST_LIST_NAME") ?? string.Empty,
            StatusLims = ObtenerValor<string>(reader, "STATUS_LIMS") ?? string.Empty,
            ReportNumber = ObtenerValor<long?>(reader, "REPORT_NUMBER"),
            Project = ObtenerValor<long?>(reader, "PROJECT"),
            Customer = ObtenerValor<long?>(reader, "CUSTOMER"),
            Analysis = ObtenerValor<string>(reader, "ANALYSIS") ?? string.Empty,
            ChangedOn = ObtenerValor<DateTime?>(reader, "CHANGED_ON"),
            FechaAprobacion = ObtenerValor<DateTime?>(reader, "FECHA_APROBACION"),
            FechaEmision = ObtenerValor<DateTime?>(reader, "FECHA_EMISION"),
            XFechaIni = ObtenerValor<DateTime?>(reader, "X_FECHA_INI")
        };
    }

    public async Task<ResultadoPaginado<EstadoMuestraDTO>> ListarPaginadoAsync(ListarPaginadoPeticion peticion, string bd, CancellationToken ct, Action<SqlCommand>? agregarParametrosExtra = null)
    {
        var resultado = new ResultadoPaginado<EstadoMuestraDTO>();
        int offset = (peticion.Pagina - 1) * peticion.Cantidad;

        string totalSinFiltrarKey = $"TotalSinFiltrar_{_nombreTabla}_{bd}";
        if (!_cache.TryGetValue(totalSinFiltrarKey, out int totalSinFiltrar)) totalSinFiltrar = -1;

        string busquedaLimpia = peticion.Busqueda?.Trim().ToLower() ?? "";
        string estadoLimpio = peticion.Estado?.Trim().ToLower() ?? "";
        string totalFiltradoKey = $"TotalFiltrado_{_nombreTabla}_{bd}_{busquedaLimpia}_{estadoLimpio}";
        if (!_cache.TryGetValue(totalFiltradoKey, out int totalFiltrado)) totalFiltrado = -1;

        List<long> localCustomerIds = new();
        List<string> localAnalysisIds = new();

        if (!string.IsNullOrWhiteSpace(peticion.Busqueda))
        {
            string searchMappingKey = $"SearchMapping_{bd}_{busquedaLimpia}";
            if (!_cache.TryGetValue(searchMappingKey, out (List<long> Cids, List<string> Aids) mapping))
            {
                using (var connHana = _conexion.CrearConexionHana(bd))
                {
                    await connHana.OpenAsync(ct);
                    string sqlC = @"SELECT ""U_LIMSReferenceNumber"" FROM OCRD WHERE (""LicTradNum"" LIKE ? OR ""CardName"" LIKE ?) AND ""U_LIMSReferenceNumber"" IS NOT NULL LIMIT 101";
                    using (var cmdC = new HanaCommand(sqlC, connHana))
                    {
                        cmdC.Parameters.Add(new HanaParameter("", $"%{peticion.Busqueda}%"));
                        cmdC.Parameters.Add(new HanaParameter("", $"%{peticion.Busqueda}%"));
                        using var rC = (HanaDataReader)await cmdC.ExecuteReaderAsync(ct);
                        while (await rC.ReadAsync()) localCustomerIds.Add(Convert.ToInt64(rC[0]));
                    }

                    string sqlA = @"SELECT ""U_IntegrationCode"" FROM OITM WHERE ""ItemName"" LIKE ? AND ""U_IntegrationCode"" IS NOT NULL LIMIT 101";
                    using (var cmdA = new HanaCommand(sqlA, connHana))
                    {
                        cmdA.Parameters.Add(new HanaParameter("", $"%{peticion.Busqueda}%"));
                        using var rA = (HanaDataReader)await cmdA.ExecuteReaderAsync(ct);
                        while (await rA.ReadAsync()) localAnalysisIds.Add(rA[0].ToString() ?? string.Empty);
                    }
                }

                if (localCustomerIds.Count > 100 || localAnalysisIds.Count > 100)
                    throw new InvalidOperationException("La búsqueda coincide con demasiados registros en SAP. Por favor, sea más específico.");

                mapping = (localCustomerIds, localAnalysisIds);
                _cache.Set(searchMappingKey, mapping, TimeSpan.FromMinutes(10));
            }
            localCustomerIds = mapping.Cids;
            localAnalysisIds = mapping.Aids;
        }

        UsarConexionLIMS(true);
        using (var connLims = CrearConexion(""))
        {
            await connLims.OpenAsync(ct);
            List<string> filtros = new() { "1=1" };
            if (!string.IsNullOrWhiteSpace(peticion.Busqueda))
            {
                string orClause = "(DESCRIPTION LIKE @b OR TEXT_ID LIKE @b OR ANALYSIS LIKE @b OR CAST(CUSTOMER AS VARCHAR) LIKE @b)";
                if (localCustomerIds.Any()) orClause += $" OR CUSTOMER IN ({string.Join(",", localCustomerIds)})";
                if (localAnalysisIds.Any()) orClause += $" OR ANALYSIS IN ({string.Join(",", localAnalysisIds.Select(id => $"'{id}'"))})";
                filtros.Add($"({orClause})");
            }
            if (!string.IsNullOrWhiteSpace(peticion.Estado)) filtros.Add(" STATUS_LIMS = @estado ");

            string whereSql = string.Join(" AND ", filtros);
            string sqlCountSin = (totalSinFiltrar == -1) ? "SELECT COUNT(*) FROM View_Muestras_LIMS_SAP;" : "";
            string sqlCountFil = (totalFiltrado == -1) ? $"SELECT COUNT(*) FROM View_Muestras_LIMS_SAP WHERE {whereSql};" : "";

            string sqlLims = $@"SET NOCOUNT ON; {sqlCountSin} {sqlCountFil}
                SELECT TEXT_ID, SAMPLE_NUMBER, COST_ITEM_TL,
TEST_LIST_NAME, DESCRIPTION, STATUS_LIMS, CUSTOMER, ANALYSIS, CHANGED_ON, REPORT_NUMBER, REPLACE(PROJECT, '-', '') AS 'PROJECT', FECHA_EMISION, FECHA_APROBACION, X_FECHA_INI
                FROM View_Muestras_LIMS_SAP WHERE {whereSql}
                ORDER BY SAMPLE_NUMBER DESC OFFSET @offset ROWS FETCH NEXT @cantidad ROWS ONLY";

            using var cmd = new SqlCommand(sqlLims, connLims);
            cmd.Parameters.AddWithValue("@offset", offset);
            cmd.Parameters.AddWithValue("@cantidad", peticion.Cantidad);
            if (!string.IsNullOrWhiteSpace(peticion.Busqueda)) cmd.Parameters.AddWithValue("@b", $"%{peticion.Busqueda}%");
            if (!string.IsNullOrWhiteSpace(peticion.Estado)) cmd.Parameters.AddWithValue("@estado", peticion.Estado);

            using var reader = await cmd.ExecuteReaderAsync(ct);
            if (totalSinFiltrar == -1)
            {
                if (await reader.ReadAsync())
                {
                    totalSinFiltrar = reader.GetInt32(0);
                    _cache.Set(totalSinFiltrarKey, totalSinFiltrar, TimeSpan.FromMinutes(15));
                }
                await reader.NextResultAsync();
            }
            resultado.TotalSinFiltrar = totalSinFiltrar;

            if (totalFiltrado == -1)
            {
                if (await reader.ReadAsync())
                {
                    totalFiltrado = reader.GetInt32(0);
                    _cache.Set(totalFiltradoKey, totalFiltrado, TimeSpan.FromMinutes(5));
                }
                await reader.NextResultAsync();
            }
            resultado.TotalFiltrado = totalFiltrado;

            while (await reader.ReadAsync()) resultado.Data.Add(Mapear(reader));
        }

        if (resultado.Data.Count == 0) return resultado;

        int cacheVersion = _cache.GetOrCreate($"Version_Match_{bd}", entry => 1);
        var missingCustomerIds = new List<long>();
        var missingAnalysisCodes = new List<string>();
        var missingMatchItems = new List<EstadoMuestraDTO>();

        foreach (var item in resultado.Data)
        {
            if (item.Customer.HasValue && !_cache.TryGetValue($"Cliente_{bd}_{item.Customer}", out (string, string, string) _))
                missingCustomerIds.Add(item.Customer.Value);

            string valorMatchArt = (item.CostItemTl > 0) ? item.CostItemTl.Value.ToString() : item.Analysis;
            if (!string.IsNullOrEmpty(valorMatchArt) && !_cache.TryGetValue($"Articulo_{bd}_{valorMatchArt}", out (string?, string?) _))
                missingAnalysisCodes.Add(valorMatchArt);

            string mKey = $"Match_{bd}_v{cacheVersion}_{item.SampleNumber}_{item.Project ?? 0}_{item.Customer ?? 0}_{item.Analysis}_{item.CostItemTl ?? 0}";
            if (!_cache.TryGetValue(mKey, out (int, int) _)) missingMatchItems.Add(item);
        }

        if (missingCustomerIds.Any() || missingAnalysisCodes.Any())
            await CargarMaestrosDesdeHanaAsync(missingCustomerIds.Distinct().ToList(), missingAnalysisCodes.Distinct().ToList(), bd);

        if (missingMatchItems.Any())
        {
            var sns = missingMatchItems.Where(x => x.SampleNumber.HasValue).Select(x => x.SampleNumber!.Value).Distinct().ToList();
            await CargarMatchesDesdeSqlLocalAsync(sns, bd, cacheVersion);
        }

        foreach (var item in resultado.Data)
        {
            if (item.Customer.HasValue && _cache.TryGetValue($"Cliente_{bd}_{item.Customer}", out (string Ruc, string Nombre, string CardCode) cInfo))
            {
                item.ClienteRuc = cInfo.Ruc;
                item.ClienteRazonSocial = cInfo.Nombre;
                item.CardCodeSAP = cInfo.CardCode;
            }

            string vMatch = (item.CostItemTl > 0) ? item.CostItemTl.Value.ToString() : item.Analysis;
            if (!string.IsNullOrEmpty(vMatch) && _cache.TryGetValue($"Articulo_{bd}_{vMatch}", out (string? ItemName, string? ItemCode) aInfo))
            {
                item.ArticuloDescripcion = aInfo.ItemName;
                item.ItemCodeSAP = aInfo.ItemCode;
            }

            string mKey = $"Match_{bd}_v{cacheVersion}_{item.SampleNumber}_{item.Project ?? 0}_{item.Customer ?? 0}_{item.Analysis}_{item.CostItemTl ?? 0}";
            if (_cache.TryGetValue(mKey, out (int DocNum, int Series) mInfo))
            {
                item.DocNumSAP = mInfo.DocNum;
                item.SeriesSAP = mInfo.Series;
            }
        }
        return resultado;
    }

    public async Task<List<EstadoMuestraDTO>> ObtenerPendientesAutomatizacionAsync(string bd, CancellationToken ct)
    {
        var yaProcesados = new List<long>();
        UsarConexionLIMS(false);
        using (var connLocal = CrearConexion(bd))
        {
            await connLocal.OpenAsync(ct);
            using var cmdP = new SqlCommand("SELECT DISTINCT SampleNumber FROM SMC_Muestras_Procesadas", connLocal);
            using var rP = await cmdP.ExecuteReaderAsync(ct);
            while (await rP.ReadAsync()) yaProcesados.Add(Convert.ToInt64(rP[0]));
        }

        var lista = new List<EstadoMuestraDTO>();
        UsarConexionLIMS(true);
        using (var connLims = CrearConexion(""))
        {
            await connLims.OpenAsync(ct);
            string fIni = new DateTime(DateTime.Now.Year, 1, 1).ToString("yyyy-MM-dd");
            string exc = yaProcesados.Any() ? $"AND SAMPLE_NUMBER NOT IN ({string.Join(",", yaProcesados)})" : "";
            string sql = $@"SELECT TEXT_ID, SAMPLE_NUMBER, COST_ITEM_TL, TEST_LIST_NAME, DESCRIPTION, STATUS_LIMS, CUSTOMER, ANALYSIS, CHANGED_ON, REPORT_NUMBER, REPLACE(PROJECT, '-', '') AS 'PROJECT', FECHA_EMISION, FECHA_APROBACION, X_FECHA_INI 
                            FROM View_Muestras_LIMS_SAP WHERE FECHA_EMISION >= '{fIni}' {exc} ORDER BY SAMPLE_NUMBER ASC";
            using var cmd = new SqlCommand(sql, connLims);
            using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync()) lista.Add(Mapear(reader));
        }
        return lista;
    }

    public async Task CargarMaestrosDesdeHanaAsync(List<long> customerIds, List<string> analysisCodes, string schema)
    {
        using var conn = _conexion.CrearConexionHana(schema);
        await conn.OpenAsync();
        if (customerIds.Any())
        {
            string sqlC = $@"SELECT ""U_LIMSReferenceNumber"",
""LicTradNum"", ""CardName"", ""CardCode"" FROM OCRD WHERE ""U_LIMSReferenceNumber"" IN ({string.Join(",", customerIds)})";
            using var cmdC = new HanaCommand(sqlC, conn);
            using var r = (HanaDataReader)await cmdC.ExecuteReaderAsync();
            while (await r.ReadAsync()) _cache.Set($"Cliente_{schema}_{Convert.ToInt64(r[0])}", (r[1].ToString(), r[2].ToString(), r[3].ToString()), _cacheDuration);
        }
        if (analysisCodes.Any())
        {
            string codes = string.Join(",", analysisCodes.Select(c => $"'{c}'"));
            string sqlA = $@"SELECT ""U_IntegrationCode"", 
""ItemName"", ""ItemCode"" FROM OITM WHERE ""U_IntegrationCode"" IN ({codes})";
            using var cmdA = new HanaCommand(sqlA, conn);
            using var r = (HanaDataReader)await cmdA.ExecuteReaderAsync();
            while (await r.ReadAsync()) _cache.Set($"Articulo_{schema}_{r[0]}", (r[1].ToString(), r[2].ToString()), _cacheDuration);
        }
    }

    public async Task CargarMatchesDesdeSqlLocalAsync(List<long> sampleNumbers, string bd, int version)
    {
        UsarConexionLIMS(false);
        using var conn = CrearConexion(bd);
        await conn.OpenAsync();
        string sql = $@"SELECT Project, Customer, Series, Analysis, SampleNumber, DocNum, 
    CostItemTL FROM SMC_Muestras_Procesadas WHERE SampleNumber IN ({string.Join(",", sampleNumbers)})";
        using var cmd = new SqlCommand(sql, conn);
        using var r = await cmd.ExecuteReaderAsync();
        while (await r.ReadAsync())
        {
            string key = $"Match_{bd}_v{version}_{r["SampleNumber"]}_{r["Project"]}_{r["Customer"]}_{r["Analysis"]}_{r["CostItemTL"]}";
            _cache.Set(key, (Convert.ToInt32(r["DocNum"]), Convert.ToInt32(r["Series"])), _cacheDuration);
        }
    }

    public async Task<List<SMC_MuestraProcesadaDTO>> ObtenerMatchesProcesadosLocalAsync(List<long> sampleNumbers, string bd)
    {
        var lista = new List<SMC_MuestraProcesadaDTO>();
        UsarConexionLIMS(false);
        using var conn = CrearConexion(bd);
        await conn.OpenAsync();
        string sql = $"SELECT Project, Customer, Series, Analysis, SampleNumber, DocNum, CostItemTL " +
            $"FROM SMC_Muestras_Procesadas WHERE SampleNumber IN ({string.Join(",", sampleNumbers)})";
        using var cmd = new SqlCommand(sql, conn);
        using var r = await cmd.ExecuteReaderAsync();
        while (await r.ReadAsync())
        {
            lista.Add(new SMC_MuestraProcesadaDTO
            {
                Project = Convert.ToInt64(r["Project"]),
                Customer = Convert.ToInt64(r["Customer"]),
                Analysis = r["Analysis"].ToString() ?? "",
                SampleNumber = Convert.ToInt64(r["SampleNumber"]),
                Series = Convert.ToInt32(r["Series"]),
                DocNum = Convert.ToInt32(r["DocNum"]),
                CostItemTL = Convert.ToInt32(r["CostItemTL"])
            });
        }
        return lista;
    }

    public async Task GuardarRelacionMuestraSapAsync(MuestraIdentificadorDTO m, int docEntry, int series, int docNum, string bd, int idUsuario)
    {
        UsarConexionLIMS(false);
        using var conn = CrearConexion(bd);
        await conn.OpenAsync();
        string sql = @"INSERT INTO SMC_Muestras_Procesadas (Project, Customer, Analysis, SampleNumber,
DocEntry, Series, DocNum, IdUsuario, CostItemTL) VALUES (@p, @c, @a, @s, @de, @se, @dn, @u, @ci)";
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@p", m.Project);
        cmd.Parameters.AddWithValue("@c", m.Customer);
        cmd.Parameters.AddWithValue("@a", m.Analysis);
        cmd.Parameters.AddWithValue("@s", m.SampleNumber);
        cmd.Parameters.AddWithValue("@de", docEntry);
        cmd.Parameters.AddWithValue("@se", series);
        cmd.Parameters.AddWithValue("@dn", docNum);
        cmd.Parameters.AddWithValue("@u", idUsuario);
        cmd.Parameters.AddWithValue("@ci", m.CostItemTl ?? 0);
        await cmd.ExecuteNonQueryAsync();
        int v = _cache.GetOrCreate($"Version_Match_{bd}", e => 1);
        _cache.Set($"Version_Match_{bd}", v + 1);
    }

    public async Task GuardarLogAutomatizacionAsync(EstadoMuestraDTO m, string estado, string mensaje, int? docNum, int? series, int? docEntry, int idUsuario, string bd)
    {
        UsarConexionLIMS(false);
        using var conn = CrearConexion(bd);
        await conn.OpenAsync();
        string sql = @"INSERT INTO 
SMC_Logs_Automatizacion 
(SampleNumber, Project, Customer, Analysis, CostItemTl, CardCodeSAP,
Estado, Mensaje, DocNumSAP, IdUsuario, SeriesSAP, DocEntrySAP) VALUES (@s, @p, @c, @a, @ci,
@cc, @est, @msg, @dn, @u, @sSap, @deSap)";
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@s", m.SampleNumber ?? 0);
        cmd.Parameters.AddWithValue("@p", (object?)m.Project ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@c", (object?)m.Customer ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@a", m.Analysis ?? string.Empty);
        cmd.Parameters.AddWithValue("@ci", m.CostItemTl ?? 0);
        cmd.Parameters.AddWithValue("@cc", m.CardCodeSAP ?? string.Empty);
        cmd.Parameters.AddWithValue("@est", estado);
        cmd.Parameters.AddWithValue("@msg", mensaje);
        cmd.Parameters.AddWithValue("@dn", (object?)docNum ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@u", idUsuario);
        cmd.Parameters.AddWithValue("@sSap", (object?)series ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@deSap", (object?)docEntry ?? DBNull.Value);
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task<string?> ObtenerGroupNumClienteAsync(bool esSNExtranjero, string cardCode, bool tieneDet, string schema)
    {
        using var conn = _conexion.CrearConexionHana(schema);
        await conn.OpenAsync();

        string sql = @"SELECT ""GroupNum"" FROM OCRD WHERE ""CardCode"" = ?";

        using var cmd = new HanaCommand(sql, conn);
        cmd.Parameters.Add(new HanaParameter("", cardCode));

        var res = await cmd.ExecuteScalarAsync();
        string? unmodifiedGroupNumSN = res != null && res != DBNull.Value ? Convert.ToString(res) : "";

        if (string.IsNullOrEmpty(unmodifiedGroupNumSN) | unmodifiedGroupNumSN == "") return unmodifiedGroupNumSN;

        string sql2 = @$"SELECT ""PymntGroup"" FROM OCTG WHERE ""GroupNum"" = '{unmodifiedGroupNumSN}'";

        using var cmdPymntGroup = new HanaCommand(sql2, conn);
        var pymntGroupSNRes = await cmdPymntGroup.ExecuteScalarAsync();

        string? unmodifiedPymntGroupSN = pymntGroupSNRes != null && pymntGroupSNRes != DBNull.Value ? Convert.ToString(pymntGroupSNRes) : "";

        if (string.IsNullOrEmpty(unmodifiedPymntGroupSN)) return unmodifiedGroupNumSN;


        // MAPEAR COND. PAGO A LA QUE ES SIN DET.
        if ((esSNExtranjero && unmodifiedPymntGroupSN.StartsWith("DT")) || (!tieneDet && unmodifiedPymntGroupSN.StartsWith("DT")))
        {
            string pymntGroupSNSinDT = Regex.Replace(unmodifiedPymntGroupSN, @"^DT\d{1,18}\d{1,18}\s?", "");

            string sql3 = $@"SELECT ""GroupNum"" FROM OCTG WHERE ""PymntGroup"" = '{pymntGroupSNSinDT}' ";

            using var cmdPymntGroupSinDT = new HanaCommand(sql3, conn);

            var resCMDPymntGroupSinDT = await cmdPymntGroupSinDT.ExecuteScalarAsync();

            string? dbPymntGroupSNSinDT = resCMDPymntGroupSinDT != null && resCMDPymntGroupSinDT != DBNull.Value ? Convert.ToString(resCMDPymntGroupSinDT) : "";

            if (string.IsNullOrEmpty(dbPymntGroupSNSinDT)) return unmodifiedGroupNumSN;
            return dbPymntGroupSNSinDT;

        }
        // MAPEAR COND. PAGO A LA QUE ES CONT DET.
        else if(tieneDet && !unmodifiedPymntGroupSN.StartsWith("DT") && !esSNExtranjero)
        {
            string sql3 = $@"SELECT ""GroupNum"" FROM OCTG WHERE ""PymntGroup"" = 'DT12 {unmodifiedPymntGroupSN}'";

            using var cmdPymntGroupContDT = new HanaCommand(sql3, conn);

            var resCMDPymntGroupConDT = await cmdPymntGroupContDT.ExecuteScalarAsync();

            string? dbPymntGroupSNContDT = resCMDPymntGroupConDT != null && resCMDPymntGroupConDT != DBNull.Value 
                ? Convert.ToString(resCMDPymntGroupConDT) : "";

            if (string.IsNullOrEmpty(dbPymntGroupSNContDT)) return unmodifiedGroupNumSN;

            return dbPymntGroupSNContDT;
        }
        return unmodifiedGroupNumSN;
    }

    public async Task<bool> ExisteProveedorEnSAP(string cardCode)
    {
        using var conn = _conexion.CrearConexionHana("");
        await conn.OpenAsync();

        string sql = @"SELECT ""CardCode"" FROM OCRD WHERE ""CardCode"" = ?";

        using var cmd = new HanaCommand(sql, conn);
        cmd.Parameters.Add(new HanaParameter("", cardCode));

        var res = await cmd.ExecuteScalarAsync();

        string? cardcodeRes = res != null && res != DBNull.Value ? Convert.ToString(res) : "";
        if (string.IsNullOrEmpty(cardcodeRes)) return false;
        return true;
    }

    public async Task<ClienteSAPDTO> BuscarCardCodeByRUC(string ruc)
    {
        ClienteSAPDTO cliente = new ClienteSAPDTO();    
        using var conn = _conexion.CrearConexionHana("");
        await conn.OpenAsync();

        string sql = @"SELECT T0.""CardCode"",T1.""GroupName""
                    FROM OCRD T0 
                    INNER JOIN OCRG T1  ON T0.""GroupCode"" = T1.""GroupCode""  
                    WHERE T0.""CardType""='C' and T0.""LicTradNum"" = ?";

        using var cmd = new HanaCommand(sql, conn);
        cmd.Parameters.Add(new HanaParameter("", ruc));
        using var r = await cmd.ExecuteReaderAsync();
        while (await r.ReadAsync())
        {
            cliente = (new ClienteSAPDTO
            {
                CardCode = r["CardCode"].ToString(),
                GrupoSN = (r["GroupName"].ToString()),
            });
        }
        return cliente;

    }

}