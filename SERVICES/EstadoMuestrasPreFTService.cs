using DAO;
using DTO;
using DTO.SAPB1;
using INTERFACES;
using INTERFACES.Repositories;
using INTERFACES.Services;
using INTERFACES.Services.SAPB1;
using Sap.Data.Hana;
using SERVICES.SAPB1;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace SERVICES
{
    public class EstadoMuestrasPreFTService : IEstadoMuestrasPreFTService
    {
        private readonly IEstadoMuestrasPreFTRepository _estadoMuestrasPreFTRepo;
        private readonly IEstadoMuestrasRepository _estadoMuestrasRepo;
        private readonly IFacturaSAPService _facturaSAPService;
        private readonly Conexion _cn;
        private readonly AdminAutomatPFService _adminAutomatPFService;
        private readonly ILogsAutomatizacionPFRepository _logsAutomatizacionPFRepo;

        public EstadoMuestrasPreFTService(IEstadoMuestrasPreFTRepository estadoMuestrasPreFTRepo,
         IEstadoMuestrasRepository estadoMuestrasRepo, IFacturaSAPService facturaSAPService, Conexion cn, AdminAutomatPFService adminAutomatPFService,
         ILogsAutomatizacionPFRepository logsAutomatizacionPFRepo)
        {
            _estadoMuestrasPreFTRepo = estadoMuestrasPreFTRepo;
            _estadoMuestrasRepo = estadoMuestrasRepo;
            _facturaSAPService = facturaSAPService;
            _cn = cn;
            _adminAutomatPFService = adminAutomatPFService;
            _logsAutomatizacionPFRepo = logsAutomatizacionPFRepo;
        }

        public async Task<ResultOp<FacturarPreFTEnSAPResDTO>> FacturarPreFTEnSAP(FacturarPreFTEnSAPReqDTO peticion, string bd, int idUsuario, bool desdeAutomatico = false)
        {
            if (peticion.Identificadores == null || !peticion.Identificadores.Any())
                return ResultOp<FacturarPreFTEnSAPResDTO>.Fallo("No se seleccionaron líneas para facturar.");

            /*
                        var partesPrimerId = peticion.Identificadores.First().Split('|');
                        if (partesPrimerId.Length < 3)
                            return ResultOp<FacturarPreFTEnSAPResDTO>.Fallo("El formato del identificador no es válido.");
            */



            //var preFts = new List<EstadoMuestrasPreFT>();
            var preFts = peticion.Identificadores;

            /* NO ORDENAMOS PORQUE YA VIENE ORDENADO DE LA VISTA, ADEMAS,  LA COLUMNA ORDEN DE LA VISTA
            EXPRESA UN ORDEN INCORRECTO SI LO TOMAMOS COMO PARAMETRO DE ORDENACION.
            */
            //preFts = preFts.OrderBy(p => p.Orden).ToList();


            /*
                        foreach (var dbLine in peticion.Identificadores)
                        {

                            var parts = dbLine.Split("|");


                            var lineaSegura = await _estadoMuestrasPreFTRepo.ObtenerPorIdentificadorAsync(parts[2],
                                parts[1], parts[3], parts[4], Convert.ToInt32(parts[5]), Convert.ToInt32(parts[6]), bd);
                            if (lineaSegura != null)
                            {
                                preFts.Add(lineaSegura);
                            }

                        }*/


            if (!preFts.Any() || preFts.Count != peticion.Identificadores.Count)
                return ResultOp<FacturarPreFTEnSAPResDTO>.Fallo("Algunas de las líneas seleccionadas ya no existen o los datos han cambiado en la base de datos.");


            if (preFts.Select(pf => pf.Customer).Distinct().Count() > 1)
                return ResultOp<FacturarPreFTEnSAPResDTO>.Fallo("No se pueden facturar líneas de diferentes clientes.");

            if (preFts.Select(pf => pf.NroPF).Distinct().Count() > 1)
                return ResultOp<FacturarPreFTEnSAPResDTO>.Fallo("No se pueden facturar líneas de diferentes facturas.");

            var todasLasLineasDeLaPF = await ListarPaginadoAsync(new EstadoMuestrasPreFTListarPaginado
            {
                Cantidad = Int32.MaxValue,
                Pagina = 1,
                NroPF = preFts.First().NroPF,
                ClienteRuc = preFts.First().Cliente
            }, bd, new CancellationToken());

            if (todasLasLineasDeLaPF.Datos?.Data.Count != preFts.Count && desdeAutomatico == false)
            {
                return ResultOp<FacturarPreFTEnSAPResDTO>.Fallo("Por favor, seleccione todas las lineas de prefactura para facturar.");
            }

            //var lineasYaFacturadas = await ObtenerLineasYaFacturadasEnSAPAsync(preFts, bd);
            //if (lineasYaFacturadas.Any())
            //{
            //    return ResultOp<FacturarPreFTEnSAPResDTO>.Fallo(
            //        $"Las siguientes líneas ya se encuentran facturadas o en borrador en SAP:\n- {string.Join("\n- ", lineasYaFacturadas)}");
            //}



            /*if (preFts.Where(p => p.StatusLIMS != "FINALIZADO").Any())
            {
                return ResultOp<FacturarPreFTEnSAPResDTO>.Fallo("No se pueden facturar prefacturas sin finalizar en LIMS");
            }
*/

            decimal tasaCambioHoy = await ObtenerTasaCambioDiaAsync("USD");
            if (tasaCambioHoy == -1)
            {
                return ResultOp<FacturarPreFTEnSAPResDTO>.Fallo("No se pudo encontrar la tasa de cambio actualizada en SAP");
            }
            var lineasFactura = new List<SapPfInvoiceLineDTO>();
            decimal totalAcumuladoDocCur = 0;

            string monedaFactura = preFts[0].Moneda == "DOLARES" ? "USD" : "SOL";

            string ItemCode = "SRV00001";
            string cuentaItem = await ObtenerSAPAcctCodeParaLineaPorMoneda(monedaFactura, ItemCode, "");

            /*BUSCAR POR RUC*/

            ClienteSAPDTO clienteSAP = await _estadoMuestrasRepo.BuscarCardCodeByRUC(preFts.First().Cliente.Split("-")[0]);
            if (string.IsNullOrEmpty(clienteSAP.CardCode))
                return ResultOp<FacturarPreFTEnSAPResDTO>.Fallo($"El Socio de Negocio {clienteSAP.CardCode} no existe en SAP.");

            bool Extranjero = false;
            if (clienteSAP.GrupoSN.ToUpper().Contains("EXT"))
            {
                Extranjero = true;
            }
            ;

            foreach (var linea in preFts)
            {
                decimal totalLinea = linea.Cantidad * linea.SubTotalLineaPF;
                totalAcumuladoDocCur += totalLinea;


                lineasFactura.Add(new SapPfInvoiceLineDTO
                {
                    UnitPrice = Extranjero ? linea.SubTotalLineaPF : linea.ValorUnitarioAnaPF, //  extranjero = ValorUnitarioAnaPF + igv
                    TaxCode = Extranjero ? "EXO" : "IGV", // extranjero = EXO
                    ItemCode = ItemCode,
                    Quantity = linea.Cantidad,
                    AccountCode = cuentaItem,
                    U_SMC_MUESTRAS_LIMS = linea.Project,
                    /*
                      U_SMC_LOTE = linea.CostItemTL > 0 ? (int)linea.CostItemTL : (int)linea.Analysis!,
                      U_SMC_AGRUPADO = linea.TipoServicio,
                      U_Identification = linea.Description,*/
                    U_SMC_RDLIMS1 = linea.DetalleLinea,
                    U_SMC_TipoAfecIGV = Extranjero ? "40" : "10" // extranjero = 40
                });
            }


            decimal factorConversion = (monedaFactura == "SOL" || monedaFactura == "PEN") ? 1 : tasaCambioHoy;
            decimal totalFacturaSoles = totalAcumuladoDocCur * factorConversion;

            string grupoDetFinal = totalFacturaSoles > 700 ? "037" : "099";
            lineasFactura.ForEach(l => l.U_EXX_GRUPODET = grupoDetFinal);



            //bool existeProveedor = await _estadoMuestrasRepo.ExisteProveedorEnSAP($"C{preFts.First().Cliente.Split("-")[0]}");
            //if (!existeProveedor)
            //    return ResultOp<FacturarPreFTEnSAPResDTO>.Fallo($"El Socio de Negocio {preFts.First().Cliente.Split("-")[1]} no existe en SAP.");

          
            string ? paymentGroupCode = await _estadoMuestrasRepo.ObtenerGroupNumClienteAsync(Extranjero, $"{clienteSAP.CardCode}", grupoDetFinal == "037", bd);

            if (string.IsNullOrEmpty(paymentGroupCode))
                return ResultOp<FacturarPreFTEnSAPResDTO>.Fallo($"No se pudo encontrar condición de pago para el Socio de Negocio {clienteSAP.CardCode} en SAP.");
            string paymentGroup = paymentGroupCode ?? "1";


            string ctrlAccount = await ObtenerSAPAcctCodePorMoneda(monedaFactura, "");
            string correosDeCliente = await ObtenerCorreosDeCliente(preFts.First().RucLIMSParaSAP);

            var nuevaFactura = new
            {
                CardCode = clienteSAP.CardCode,
                CardName = preFts.First().Cliente.Split("-")[1],
                DocDate = DateTime.Now.ToString("yyyy-MM-dd"),
                DocCurrency = monedaFactura,
                Indicator = "01",
                TaxCode = "IGV",
                U_ProjectoLims = preFts.First().NroPF,
                DocType = "dDocument_Items",
                PaymentGroupCode = paymentGroup,
                Comments = preFts.First().NroPF,
                DocumentLines = lineasFactura,
                ControlAccount = ctrlAccount,
                DocObjectCode = "13",
                U_SMC_CORREOS = correosDeCliente,
                U_EXX_TIPEXP = Extranjero ? "01" : "00"
            };

            var resSap = await _facturaSAPService.CrearFacturaBorradorAsync(nuevaFactura);

            if (resSap.Exito && resSap.Datos != null && grupoDetFinal == "037")
            {
                int docEntryCreado = (int)resSap.Datos.DocEntry;
                await ActualizarCuotasDetraccionHanaAsync(docEntryCreado);
            }

            if (resSap.Exito && resSap.Datos != null)
            {
                return ResultOp<FacturarPreFTEnSAPResDTO>.Ok(new FacturarPreFTEnSAPResDTO
                {
                    DocEntry = (int)resSap.Datos.DocEntry,
                    DocNum = (int)resSap.Datos.DocNum,
                    Series = (int)resSap.Datos.Series
                });
            }

            return ResultOp<FacturarPreFTEnSAPResDTO>.Fallo("No se ha podido crear la factura en SAP");
        }

        private async Task<string> ObtenerSAPAcctCodePorMoneda(string moneda, string bd)
        {
            string segment0 = moneda == "USD" ? "1212103" : "1212101";

            string sql = @$"SELECT ""AcctCode"" FROM OACT WHERE ""Segment_0"" = '{segment0}' AND ""Segment_1"" = '00' AND ""Segment_2"" = '00'";

            using var cn = _cn.CrearConexionHana("");

            await cn.OpenAsync();

            using var cmd = new HanaCommand(sql, cn);

            var res = await cmd.ExecuteScalarAsync();

            return res?.ToString() ?? "";
        }


        private async Task<string> ObtenerSAPAcctCodeParaLineaPorMoneda(string moneda, string itemCode, string bd)
        {
            string sql = @$"
                   SELECT B.""RevenuesAc"",B.""FrRevenuAc""
					FROM
					OITM A JOIN OITB B ON A.""ItmsGrpCod""=B.""ItmsGrpCod""
					where
					A.""ItemCode""='{itemCode}'
            ";

            using var cn = _cn.CrearConexionHana("");
            await cn.OpenAsync();

            using var cmd = new HanaCommand(sql, cn);

            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                string cuentaSoles = reader["RevenuesAc"]?.ToString() ?? "";
                string cuentaUSD = reader["FrRevenuAc"]?.ToString() ?? "";

                return moneda == "SOL" ? cuentaSoles : cuentaUSD;
            }

            return "";
        }

        private async Task<string> ObtenerCorreosDeCliente(string cardCode)
        {
            string query = $@"
            SELECT ""E_MailL"" FROM OCPR WHERE ""CardCode""='{cardCode}' and IFNULL(""EmlGrpCode"",'N') = 'Y'
            ";
            List<string> correos = new List<string>();
            using var cn = _cn.CrearConexionHana("");
            await cn.OpenAsync();
            using var cmd = new HanaCommand(query, cn);

            using var reader = await cmd.ExecuteReaderAsync();
            if (!reader.HasRows) return "";
            while(await reader.ReadAsync())
            {
                correos.Add(reader["E_MailL"] != DBNull.Value ? reader["E_MailL"].ToString() ?? "" : "");
            }
            return string.Join(";", correos);
        }

        private async Task<List<string>> ObtenerLineasYaFacturadasEnSAPAsync(List<EstadoMuestrasPreFT> preFts, string bd)
        {
            var lineasYaExistentes = new List<string>();
            string nroPF = preFts.First().NroPF;
            string cardCode = $"{preFts.First().Cliente.Split("-")[0]}";

            using var cn = _cn.CrearConexionHana(bd);
            await cn.OpenAsync();

            string query = @"
                WITH FirmData AS (
                    SELECT L.""U_SMC_RDLIMS1""
                    FROM OINV I 
                    INNER JOIN INV1 L ON I.""DocEntry"" = L.""DocEntry""
                    INNER JOIN OCRD T0 ON I.""CardCode"" = T0.""CardCode""
                    WHERE I.""Comments"" = ? AND T0.""CardType""='C' and T0.""LicTradNum"" = ? AND L.""BaseType"" = '-1' 
                ),
                DraftData AS (
                    SELECT L.""U_SMC_RDLIMS1""
                    FROM ODRF I 
                    INNER JOIN DRF1 L ON I.""DocEntry"" = L.""DocEntry""
                    INNER JOIN OCRD T0 ON I.""CardCode"" = T0.""CardCode""
                    WHERE I.""Comments"" = ? AND  T0.""CardType""='C' and T0.""LicTradNum"" = ? AND I.""ObjType"" = '13' AND I.""DocStatus"" = 'O' AND L.""BaseType"" = '-1'
                )
                SELECT * FROM FirmData
                UNION ALL
                SELECT * FROM DraftData";

            using var cmd = new HanaCommand(query, cn);
            cmd.Parameters.Add(new HanaParameter("", nroPF));
            cmd.Parameters.Add(new HanaParameter("", cardCode));

            cmd.Parameters.Add(new HanaParameter("", nroPF));
            cmd.Parameters.Add(new HanaParameter("", cardCode));

            var lineasSap = new List<string>();

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                lineasSap.Add(
                    reader["U_SMC_RDLIMS1"]?.ToString() ?? ""
                );
            }

            foreach (var linea in preFts)
            {

                string detalleLineComp = linea.DetalleLinea ?? "";


                bool yaExiste = lineasSap.Any(s =>
                    s == detalleLineComp.Replace("\n", "")
                    );

                if (yaExiste)
                {
                    lineasYaExistentes.Add($"PF: {nroPF} Linea: {detalleLineComp}");
                }
            }

            return lineasYaExistentes;
        }

        public async Task<decimal> ObtenerTasaCambioDiaAsync(string moneda)
        {
            using var cn = _cn.CrearConexionHana("");
            await cn.OpenAsync();
            string query = @"SELECT TOP 1 ""Rate"" FROM ORTT WHERE ""Currency"" = ? AND ""RateDate"" = TO_DATE(?)";
            using var cmd = new HanaCommand(query, cn);
            cmd.Parameters.Add(new HanaParameter("", moneda));
            cmd.Parameters.Add(new HanaParameter("", DateTime.Now.ToString("yyyy-MM-dd")));
            var result = await cmd.ExecuteScalarAsync();

            return result != null ? Convert.ToDecimal(result) : -1;
        }

        private async Task ActualizarCuotasDetraccionHanaAsync(int docEntry)
        {
            using var cn = _cn.CrearConexionHana("");
            await cn.OpenAsync();
            
            string query = @"
        UPDATE DRF6 
        SET ""U_EXX_CONFTIPODET"" = CASE WHEN ""InstlmntID"" = 1 THEN 'SI' ELSE 'NO' END 
        WHERE ""DocEntry"" = ? AND ""InstlmntID"" IN (1, 2)";

            using var cmd = new HanaCommand(query, cn);
            cmd.Parameters.Add(new HanaParameter("", docEntry));

            await cmd.ExecuteNonQueryAsync();
        }


        public async Task<ResultOp<ResultadoPaginado<EstadoMuestrasPreFT>>> ListarPaginadoAsync(EstadoMuestrasPreFTListarPaginado peticion,
            string bd,
        CancellationToken ct)
        {
            return ResultOp<ResultadoPaginado<EstadoMuestrasPreFT>>.
            Ok(await _estadoMuestrasPreFTRepo.ListarPaginadoAsync(peticion, bd, (cmd) =>
            {
                cmd.Parameters.AddWithValue("Cliente", string.IsNullOrEmpty(peticion.ClienteRuc) ? DBNull.Value : peticion.ClienteRuc);
                cmd.Parameters.AddWithValue("NroPF", string.IsNullOrEmpty(peticion.NroPF) ? DBNull.Value : peticion.NroPF);
                cmd.Parameters.AddWithValue("StatusLIMS", string.IsNullOrEmpty(peticion.StatusLIMS) ? DBNull.Value : peticion.StatusLIMS);
            }, ct));
        }

        public async Task EjecutarProcesamientoAutomaticoAsync(string bd, int idUsuario, CancellationToken ct)
        {
            try
            {
                
                _adminAutomatPFService.UpdateProgress(bd, p =>
                {
                    p.Procesadas = 0;
                    p.TotalFilas = 0;
                    p.ClienteActual = "";
                    p.ProyectoActual = "";
                });

                _adminAutomatPFService.AddAction(bd, "🔍 Buscando nuevas prefacturas...");

                var prefacturas = await _estadoMuestrasPreFTRepo.ObtenerPrefacturasPlanasPorAnioAsync(DateTime.Now.Year, "");

                var prefacturasPendientes = prefacturas.Where(m => m.DocumentosSAP.Count == 0).ToList();

                if (!prefacturasPendientes.Any())
                {
                    _adminAutomatPFService.AddAction(bd, "⚠️ No se encontraron muestras pendientes para procesar.");
                    return; 
                }

                _adminAutomatPFService.UpdateProgress(bd, p => p.TotalFilas = prefacturasPendientes.GroupBy(pf => new { pf.NroPF, pf.Customer }).Count());
                _adminAutomatPFService.AddAction(bd, $"📊 Se encontraron {prefacturasPendientes.GroupBy(pf => new { pf.NroPF, pf.Customer }).Count()} prefacturas.");

                var gruposPorCliente = prefacturasPendientes.OrderBy(x => x.RazonSocialLIMSParaSAP).GroupBy(x => x.RucLIMSParaSAP);

                foreach (var grupoCliente in gruposPorCliente)
                {
                    if (ct.IsCancellationRequested) break;

                    var nombreCliente = grupoCliente.First().RazonSocialLIMSParaSAP;
                    _adminAutomatPFService.UpdateProgress(bd, p => p.ClienteActual = nombreCliente);
                    _adminAutomatPFService.AddAction(bd, $"👤 Procesando Cliente: {nombreCliente}");

                    foreach (var grupoPorPrefactura in grupoCliente.GroupBy(x => x.NroPF))
                    {
                        if (ct.IsCancellationRequested) break;

                        var nroPF = grupoPorPrefactura.Key?.ToString() ?? "---";
                        _adminAutomatPFService.UpdateProgress(bd, p => p.ProyectoActual = nroPF);
                        _adminAutomatPFService.AddAction(bd, $"📁 Entrando a Prefactura: {nroPF}");

                        await _adminAutomatPFService.WaitIfPaused(bd, ct);

                        _adminAutomatPFService.AddAction(bd, $"📦 Creando factura con ({grupoPorPrefactura.Count()} líneas)");

                        var reqParaFacturar = new FacturarPreFTEnSAPReqDTO
                        {
                            Identificadores = grupoPorPrefactura.ToList()
                        };

                        try
                        {
                            var facturacionEnSAPRes = await FacturarPreFTEnSAP(reqParaFacturar, bd, idUsuario, true);

                            if (!facturacionEnSAPRes.Exito && facturacionEnSAPRes.Error is not null)
                            {
                                string errorMsg = facturacionEnSAPRes.Error.Msg;

                                if (_adminAutomatPFService.DeboNotificarError(bd, nroPF, errorMsg))
                                {
                                    _adminAutomatPFService.AddAction(bd, $"⚠️ Error en PF {nroPF}: {errorMsg}");

                                    await _logsAutomatizacionPFRepo.GuardarLogAutomatizacionAsync(
                                        grupoPorPrefactura.First(), "ERROR", errorMsg, null, null, null, idUsuario, bd);
                                }
                            }
                            else if (facturacionEnSAPRes.Datos is not null)
                            {
                                int docNum = facturacionEnSAPRes.Datos.DocNum;
                                int series = facturacionEnSAPRes.Datos.Series;
                                int docEntry = facturacionEnSAPRes.Datos.DocEntry;

                                await _logsAutomatizacionPFRepo.GuardarLogAutomatizacionAsync(
                                    grupoPorPrefactura.First(), "SUCCESS", "factura generada", docNum, series, docEntry, idUsuario, bd);

                                _adminAutomatPFService.AddAction(bd, $"✨ Factura #{series}-{docNum} generada exitosamente para PF: {grupoPorPrefactura.Key}");
                                _adminAutomatPFService.UpdateProgress(bd, p => p.Procesadas += 1);

                                await Task.Delay(TimeSpan.FromSeconds(3), ct);
                            }
                        }
                        catch (Exception innerEx)
                        {
                            string errorMsg = $"Falla inesperada al facturar: {innerEx.Message}";

                            if (_adminAutomatPFService.DeboNotificarError(bd, nroPF, errorMsg))
                            {
                                _adminAutomatPFService.AddAction(bd, $"❌ Error en PF {nroPF}: {errorMsg}");
                                await _logsAutomatizacionPFRepo.GuardarLogAutomatizacionAsync(
                                    grupoPorPrefactura.First(), "CRITICAL_ERROR", errorMsg, null, null, null, idUsuario, bd);
                            }
                        }
                    }
                }

                if (ct.IsCancellationRequested)
                {
                    _adminAutomatPFService.AddAction(bd, "🛑 Ciclo detenido por el usuario.");
                }
            }
            catch (OperationCanceledException)
            {
                _adminAutomatPFService.AddAction(bd, "🛑 Ciclo detenido (CancellationToken).");
            }
            catch (Exception ex)
            {
                string msgGlobal = $"Falla general en el ciclo: {ex.Message}";

               
                if (_adminAutomatPFService.DeboNotificarError(bd, "GLOBAL", msgGlobal))
                {
                    _adminAutomatPFService.AddAction(bd, $"❌ {msgGlobal}");

                    var dummyParaLog = new EstadoMuestrasPreFT
                    {
                        NroPF = "1"
                    };

                    await _logsAutomatizacionPFRepo.GuardarLogAutomatizacionAsync(
                        dummyParaLog, "GLOBAL_CYCLE_ERROR", msgGlobal, null, null, null, idUsuario, bd);
                }
                else
                {
                    _adminAutomatPFService.AddAction(bd, $"❌ Error general persistente. Reintentando luego...");
                }
            }
            finally
            {
               
                _adminAutomatPFService.UpdateProgress(bd, p =>
                {
                    p.ClienteActual = "---";
                    p.ProyectoActual = "---";
                });
            }
        }
        private void FinalizarProgreso(string bd, string mensaje)
        {
            _adminAutomatPFService.UpdateProgress(bd, p =>
            {
                p.IsRunning = false;
                p.UltimoMensaje = mensaje;
            });
        }

        public async Task<ResultOp<ResultadoPaginado<LogsAutomatizacionPF>>> ListarLogsPaginadosAsync(ListarPaginadoPeticion peticion, string bd)
        {
            return ResultOp<ResultadoPaginado<LogsAutomatizacionPF>>.
            Ok(await _logsAutomatizacionPFRepo.ListarPaginadoAsync(peticion, bd, null, null));
        }
    }
}




