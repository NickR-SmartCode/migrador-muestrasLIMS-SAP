using DAO;
using DTO;
using DTO.SAPB1;
using INTERFACES.Repositories;
using INTERFACES.Repositories.SAPB1;
using INTERFACES.Services;
using INTERFACES.Services.SAPB1;
using Microsoft.Extensions.Caching.Memory;
using Sap.Data.Hana;
using SERVICES.SAPB1;

namespace SERVICES
{
    public class EstadoMuestrasService : IEstadoMuestrasService
    {
        private readonly IEstadoMuestrasRepository _estadoMuestrasRepository;
        private readonly IPedidosSAPService _pedidosSAPService;
        private readonly IMemoryCache _cache;
        private readonly AdminAutomatizadorPedidoService _adminAutomatizacionPedidoSrvc;
        private readonly ILogsAutomatizcacionRepository _logsAutomatizcacionRepository;
        private readonly TimeSpan _cacheDuration = TimeSpan.FromMinutes(10);

        public EstadoMuestrasService(
            IEstadoMuestrasRepository estadoMuestrasRepository,
            IPedidosSAPService pedidosSAPService,
            IMemoryCache cache,
            AdminAutomatizadorPedidoService adminAutomatizadorPedidoService,
            ILogsAutomatizcacionRepository logsAutomatizcacionRepository)
        {
            _estadoMuestrasRepository = estadoMuestrasRepository;
            _pedidosSAPService = pedidosSAPService;
            _cache = cache;
            _adminAutomatizacionPedidoSrvc = adminAutomatizadorPedidoService;
            _logsAutomatizcacionRepository = logsAutomatizcacionRepository;
        }

        public async Task<ResultOp<ResultadoPaginado<EstadoMuestraDTO>>> ListarPaginadoAsync(ListarPaginadoPeticion peticion, string bd, CancellationToken ct)
        {
            try
            {
                var datos = await _estadoMuestrasRepository.ListarPaginadoAsync(peticion, bd, ct);
                return ResultOp<ResultadoPaginado<EstadoMuestraDTO>>.Ok(datos);
            }
            catch (InvalidOperationException ex)
            {
                return ResultOp<ResultadoPaginado<EstadoMuestraDTO>>.Fallo(ex.Message, ResultOpErrores.VALIDATION_ERROR);
            }
        }

        public async Task EjecutarMockAutomatizacionAsync(string bd, int idUsuario, CancellationToken ct)
        {
            int totalSimulado = 1000;
            _adminAutomatizacionPedidoSrvc.UpdateProgress(bd, p =>
            {
                p.IsRunning = true;
                p.Procesadas = 0;
                p.TotalFilas = totalSimulado;
                p.UltimoMensaje = "Iniciando Simulación de 1000 registros...";
            });

            try
            {
                var muestrasDummy = Enumerable.Range(1, totalSimulado).Select(i => new EstadoMuestraDTO
                {
                    SampleNumber = i,
                    ClienteRazonSocial = $"CLIENTE MOCK {(i / 100) + 1}",
                    CardCodeSAP = $"C{((i / 100) + 1):D4}",
                    Project = (i / 50) + 1,
                    Analysis = "AN-MOCK",
                    CostItemTl = (i % 2 == 0) ? 0 : 500
                }).ToList();

                var gruposPorCliente = muestrasDummy.OrderBy(x => x.ClienteRazonSocial).GroupBy(x => x.CardCodeSAP);

                foreach (var grupoCliente in gruposPorCliente)
                {
                    if (ct.IsCancellationRequested) break;



                    _adminAutomatizacionPedidoSrvc.UpdateProgress(bd, p => p.ClienteActual = grupoCliente.First().ClienteRazonSocial);

                    foreach (var grupoProyecto in grupoCliente.GroupBy(x => x.Project))
                    {
                        if (ct.IsCancellationRequested) break;

                        _adminAutomatizacionPedidoSrvc.UpdateProgress(bd, p => p.ProyectoActual = $"PROY-{grupoProyecto.Key}");

                        var lotes = new List<List<EstadoMuestraDTO>>();

                        var conCI = grupoProyecto.Where(x => x.CostItemTl > 0).ToList();
                        if (conCI.Any()) lotes.Add(conCI);

                        var sinCI = grupoProyecto.Where(x => x.CostItemTl <= 0).ToList();
                        if (sinCI.Any()) lotes.Add(sinCI);

                        foreach (var lote in lotes)
                        {
                            if (ct.IsCancellationRequested) break;

                            foreach (var item in lote)
                            {
                                if (ct.IsCancellationRequested) break;

                                await Task.Delay(1000, ct);

                                _adminAutomatizacionPedidoSrvc.UpdateProgress(bd, p =>
                                {
                                    p.Procesadas++;
                                    p.UltimoMensaje = $"Procesando Muestra {item.SampleNumber}...";
                                });
                            }


                        }
                    }
                }

                _adminAutomatizacionPedidoSrvc.UpdateProgress(bd, p =>
                {
                    p.IsRunning = false;
                    p.UltimoMensaje = ct.IsCancellationRequested ? "Simulación CANCELADA por el usuario." : "Simulación finalizada con éxito.";
                });
            }
            catch (Exception ex)
            {
                _adminAutomatizacionPedidoSrvc.UpdateProgress(bd, p =>
                {
                    p.IsRunning = false;
                    p.UltimoMensaje = $"Error en simulación: {ex.Message}";
                });
            }
        }

        public async Task<ResultOp<SapOrderCreationResponseDTO>> ProcesarMuestrasASap(ProcesarMuestraSAPDTO req, string bd, int idUsuario, CancellationToken ct)
        {
            var resFinal = new SapOrderCreationResponseDTO();

            var sampleNumbersRequest = req.Muestras.Select(m => m.SampleNumber).Distinct().ToList();
            var yaProcesados = await _estadoMuestrasRepository.ObtenerMatchesProcesadosLocalAsync(sampleNumbersRequest, bd);

            var muestrasCandidatas = req.Muestras.Where(m => !yaProcesados.Any(p =>
                p.Project == m.Project && p.Customer == m.Customer &&
                ((p.Analysis == m.Analysis && p.CostItemTL == 0) || p.CostItemTL == m.CostItemTl) &&
                p.SampleNumber == m.SampleNumber
            )).ToList();

            if (!muestrasCandidatas.Any())
                return ResultOp<SapOrderCreationResponseDTO>.Fallo("Todas las muestras seleccionadas ya cuentan con un Pedido en SAP.");

            int conCostItem = muestrasCandidatas.Count(m => m.CostItemTl.HasValue && m.CostItemTl.Value != 0);
            if (conCostItem > 0 && conCostItem < muestrasCandidatas.Count)
                return ResultOp<SapOrderCreationResponseDTO>.Fallo("No se pueden procesar muestras mixtas (con y sin Cost Item).");


            string? paymentGroupCode = await _estadoMuestrasRepository.ObtenerGroupNumClienteAsync(false, req.CardCode, false, bd);
            if (paymentGroupCode == "-1")
                return ResultOp<SapOrderCreationResponseDTO>.Fallo($"No se pudo encontrar el Socio de Negocio {req.CardCode} en SAP.");


            var cotizacionesAnio = await _pedidosSAPService.ListarCotizacionesVigentesAnioActualAsync(req.CardCode, bd);

            if (cotizacionesAnio == null || !cotizacionesAnio.Exito || cotizacionesAnio.Datos == null || !cotizacionesAnio.Datos!.Any())
                return ResultOp<SapOrderCreationResponseDTO>.Fallo($"El cliente {req.CardCode} no tiene Cotizaciones de Venta en el año actual.");

            var lineasParaPedido = new List<SapOrderLineDTO>();
            var muestrasAProcesarEnSap = new List<MuestraIdentificadorDTO>();

            foreach (var m in muestrasCandidatas)
            {
                string refValue = (m.CostItemTl.HasValue && m.CostItemTl.Value != 0) ? m.CostItemTl.Value.ToString() : m.Analysis;
                string filaKey = $"{m.SampleNumber}_{m.Analysis}_{m.CostItemTl ?? 0}";

                SapOrderLineDTO? lineaEncontrada = null;

                
                foreach (var cotizacion in cotizacionesAnio.Datos)
                {
                    var match = cotizacion.DocumentLines
                        .FirstOrDefault(l => l.ItemCode == m.ItemCodeSAP && l.U_IntegrationCode == refValue);

                    if (match != null)
                    {
                        lineaEncontrada = match;
                        break; 
                    }
                }

                if (lineaEncontrada != null)
                {
                    lineasParaPedido.Add(new SapOrderLineDTO
                    {
                        ItemCode = m.ItemCodeSAP,
                        Quantity = 1,
                        UnitPrice = lineaEncontrada.UnitPrice,
                        TaxCode = lineaEncontrada.TaxCode,
                        U_SMC_MUESTRAS_LIMS = m.SampleNumber.ToString(),
                        WarehouseCode = lineaEncontrada.WarehouseCode ?? "01",
                        U_IntegrationCode = refValue

                    });
                    muestrasAProcesarEnSap.Add(m);
                    resFinal.FilasExitosasKeys.Add(filaKey);
                }
                else
                {
                    resFinal.MuestrasSinMatchQuotation[filaKey] =
                        $"Item SAP # {m.ItemCodeSAP} con Ref Lims {refValue} no hallado en NINGUNA cotización del año actual para este cliente.";
                }
            }

            if (!lineasParaPedido.Any())
                return ResultOp<SapOrderCreationResponseDTO>.Fallo("Ninguna de las muestras seleccionadas coincide con los artículos de la última cotización.");

            var nuevaOrden = new SapOrderDTO
            {
                CardCode = req.CardCode,
                PaymentGroupCode = paymentGroupCode,
                U_ProjectoLims = muestrasCandidatas[0].Project.ToString(),
                DocDueDate = DateTime.Now.ToString("yyyy-MM-dd"),
                DocumentLines = lineasParaPedido
            };

            var resSAP = await _pedidosSAPService.CrearOrdenAsync(nuevaOrden);

            if (resSAP.Exito && resSAP.Datos != null)
            {
                resFinal.DocEntry = resSAP.Datos.DocEntry;
                resFinal.DocNum = resSAP.Datos.DocNum;
                resFinal.Series = resSAP.Datos.Series;

                foreach (var muestra in muestrasAProcesarEnSap)
                {
                    await _estadoMuestrasRepository.GuardarRelacionMuestraSapAsync(muestra,
                        (int)resFinal.DocEntry, (int)resFinal.Series, (int)resFinal.DocNum, bd, idUsuario);
                }

                return ResultOp<SapOrderCreationResponseDTO>.Ok(resFinal);
            }

            return resSAP;
        }

        public async Task EjecutarProcesamientoAutomaticoAsync(string bd, int idUsuario, CancellationToken ct)
        {
            _adminAutomatizacionPedidoSrvc.UpdateProgress(bd, p =>
            {
                p.IsRunning = true;
                p.Procesadas = 0;
                p.UltimasAcciones = new List<AccionConsolaDTO>();
            });

            _adminAutomatizacionPedidoSrvc.AddAction(bd, "🚀 Iniciando extracción de muestras pendientes...");

            try
            {
                var muestras = await _estadoMuestrasRepository.ObtenerPendientesAutomatizacionAsync(bd, ct);
                if (!muestras.Any())
                {
                    _adminAutomatizacionPedidoSrvc.AddAction(bd, "⚠️ No se encontraron muestras pendientes para procesar.");
                    FinalizarProgreso(bd, "Sin muestras pendientes.");
                    return;
                }

                _adminAutomatizacionPedidoSrvc.UpdateProgress(bd, p => p.TotalFilas = muestras.Count);
                _adminAutomatizacionPedidoSrvc.AddAction(bd, $"📊 Se encontraron {muestras.Count} muestras. Validando datos maestros...");

                var muestrasListas = await EnriquecerYValidarMuestrasAsync(muestras, bd, idUsuario, ct);

                _adminAutomatizacionPedidoSrvc.AddAction(bd, $"✅ Validación terminada. {muestrasListas.Count} muestras listas para SAP.");

                var gruposPorCliente = muestrasListas.OrderBy(x => x.ClienteRazonSocial).GroupBy(x => x.CardCodeSAP);

                foreach (var grupoCliente in gruposPorCliente)
                {
                    if (ct.IsCancellationRequested) break;


                    var nombreCliente = grupoCliente.First().ClienteRazonSocial;
                    _adminAutomatizacionPedidoSrvc.UpdateProgress(bd, p => p.ClienteActual = nombreCliente);
                    _adminAutomatizacionPedidoSrvc.AddAction(bd, $"👤 Procesando Cliente: {nombreCliente}");

                    foreach (var grupoProyecto in grupoCliente.GroupBy(x => x.Project))
                    {
                        if (ct.IsCancellationRequested) break;

                        var idProyecto = grupoProyecto.Key?.ToString() ?? "---";
                        _adminAutomatizacionPedidoSrvc.UpdateProgress(bd, p => p.ProyectoActual = idProyecto);
                        _adminAutomatizacionPedidoSrvc.AddAction(bd, $"📁 Entrando a Proyecto: {idProyecto}");

                        var muestrasProyecto = grupoProyecto.ToList();
                        await _adminAutomatizacionPedidoSrvc.WaitIfPaused(bd, ct);


                        var conCostItem = muestrasProyecto.Where(x => x.CostItemTl > 0).GroupBy(x => x.CostItemTl);
                        foreach (var grupoCI in conCostItem)
                        {
                            if (ct.IsCancellationRequested) break;
                            await _adminAutomatizacionPedidoSrvc.WaitIfPaused(bd, ct);


                            _adminAutomatizacionPedidoSrvc.AddAction(bd, $"📦 Creando lote por Paquete (CostItem): {grupoCI.Key}");
                            await ProcesarLoteAsync(grupoCliente.Key!, grupoCI.ToList(), bd, idUsuario, ct);
                            _adminAutomatizacionPedidoSrvc.UpdateProgress(bd, p => p.Procesadas += grupoCI.Count());
                        }

                        var sinCostItem = muestrasProyecto.Where(x => x.CostItemTl <= 0).ToList();
                        if (sinCostItem.Any() && !ct.IsCancellationRequested)
                        {
                            await _adminAutomatizacionPedidoSrvc.WaitIfPaused(bd, ct);


                            _adminAutomatizacionPedidoSrvc.AddAction(bd, $"📦 Creando lote por Análisis Individual ({sinCostItem.Count} ítems)");
                            await ProcesarLoteAsync(grupoCliente.Key!, sinCostItem, bd, idUsuario, ct);
                            _adminAutomatizacionPedidoSrvc.UpdateProgress(bd, p => p.Procesadas += sinCostItem.Count);
                        }
                    }
                }

                string msgFinal = ct.IsCancellationRequested ? "🛑 Sincronización DETENIDA por el usuario." : "🏁 Sincronización finalizada con éxito.";
                _adminAutomatizacionPedidoSrvc.AddAction(bd, msgFinal);
                FinalizarProgreso(bd, msgFinal);
            }
            catch (Exception ex)
            {
                _adminAutomatizacionPedidoSrvc.AddAction(bd, $"❌ ERROR CRÍTICO: {ex.Message}");
                FinalizarProgreso(bd, $"Error crítico: {ex.Message}");
            }
        }

        private async Task ProcesarLoteAsync(string cardCode, List<EstadoMuestraDTO> lote, string bd, int idUsuario, CancellationToken ct)
        {
            var req = new ProcesarMuestraSAPDTO
            {
                CardCode = cardCode,
                ItemCodes = lote.Select(x => x.ItemCodeSAP!).ToList(),
                Muestras = lote.Select(x => new MuestraIdentificadorDTO
                {
                    Project = x.Project ?? 0,
                    Customer = x.Customer ?? 0,
                    Analysis = x.Analysis,
                    CostItemTl = x.CostItemTl,
                    SampleNumber = x.SampleNumber ?? 0,
                    ItemCodeSAP = x.ItemCodeSAP!
                }).ToList()
            };

            var res = await ProcesarMuestrasASap(req, bd, idUsuario, ct);

            if (res.Exito && res.Datos != null)
            {
                string msgExito = $"✨ Creado Pedido #{res.Datos.Series}-{res.Datos.DocNum} con {lote.Count} muestras. Proyecto: {req.Muestras[0].Project}";
                _adminAutomatizacionPedidoSrvc.AddAction(bd, msgExito);

                _adminAutomatizacionPedidoSrvc.AddAction(bd, "⏳ Pausa de 40s para verificación visual... (Presione Detener o Reanudar para saltar)");

                try
                {
                    await Task.Delay(40000, ct);

                    _adminAutomatizacionPedidoSrvc.AddAction(bd, "▶️ Reanudando procesamiento del siguiente lote...");
                }
                catch (OperationCanceledException)
                {
                    return;
                }
            }
            else
            {
                string msgError = $"⚠️ Error en proc. de lote para proyecto #{req.Muestras[0].Project}: {res.Error?.Msg ?? "Error en SAP"}";
                _adminAutomatizacionPedidoSrvc.AddAction(bd, msgError);
            }

            foreach (var m in lote)
            {
                string estado;
                string mensaje;
                int? docNum = null;
                int? series = null;
                int? docEntry = null;

                string filaKey = $"{m.SampleNumber}_{m.Analysis}_{m.CostItemTl ?? 0}";

                if (res.Exito && res.Datos != null)
                {
                    if (res.Datos.FilasExitosasKeys.Contains(filaKey))
                    {
                        estado = "SUCCESS";
                        mensaje = "Pedido creado correctamente";
                        docNum = (int)res.Datos.DocNum;
                        series = (int)res.Datos.Series;
                        docEntry = (int)res.Datos.DocEntry;
                    }
                    else if (res.Datos.MuestrasSinMatchQuotation.TryGetValue(filaKey, out var msgError))
                    {
                        estado = "ERROR";
                        mensaje = msgError;
                    }
                    else
                    {
                        estado = "SKIPPED";
                        mensaje = "Omitido: Ya contaba con pedido previo.";
                    }
                }
                else
                {
                    estado = "ERROR";
                    mensaje = res.Error?.Msg ?? "Error en el procesamiento del lote";
                }

                await _estadoMuestrasRepository.GuardarLogAutomatizacionAsync(m, estado, mensaje, docNum, series, docEntry, idUsuario, bd);
            }
        }



        private async Task<List<EstadoMuestraDTO>> EnriquecerYValidarMuestrasAsync(List<EstadoMuestraDTO> muestras, string bd, int idUsuario, CancellationToken ct)
        {
            var customerIds = muestras.Where(m => m.Customer.HasValue).Select(m => m.Customer!.Value).Distinct().ToList();

            var analysisCodes = muestras.Select(m => (m.CostItemTl > 0) ? m.CostItemTl.Value.ToString() : m.Analysis)
                                        .Where(code => !string.IsNullOrEmpty(code)).Distinct().ToList();

            await _estadoMuestrasRepository.CargarMaestrosDesdeHanaAsync(customerIds, analysisCodes, bd);

            var muestrasValidadas = new List<EstadoMuestraDTO>();

            foreach (var m in muestras)
            {
                if (ct.IsCancellationRequested) break;

                bool tieneError = false;
                string mensajeError = "";

                if (m.Customer.HasValue && _cache.TryGetValue($"Cliente_{bd}_{m.Customer}", out (string Ruc, string Nombre, string CardCode) cInfo))
                {
                    m.ClienteRuc = cInfo.Ruc;
                    m.ClienteRazonSocial = cInfo.Nombre;
                    m.CardCodeSAP = cInfo.CardCode;
                }
                else
                {
                    tieneError = true;
                    mensajeError = $"Cliente LIMS {m.Customer} no tiene match en OCRD.";
                }

                string valorMatchArt = (m.CostItemTl > 0) ? m.CostItemTl.Value.ToString() : m.Analysis;

                if (!tieneError && !string.IsNullOrEmpty(valorMatchArt) &&
                    _cache.TryGetValue($"Articulo_{bd}_{valorMatchArt}", out (string? ItemName, string? ItemCode) aInfo))
                {
                    m.ArticuloDescripcion = aInfo.ItemName;
                    m.ItemCodeSAP = aInfo.ItemCode;
                }
                else if (!tieneError)
                {
                    tieneError = true;
                    mensajeError = $"Código '{valorMatchArt}' no tiene match en OITM (U_IntegrationCode).";
                }

                if (tieneError)
                    await _estadoMuestrasRepository.GuardarLogAutomatizacionAsync
                        (m, "ERROR", mensajeError, null, null, null, idUsuario, bd);
                else
                    muestrasValidadas.Add(m);
            }

            return muestrasValidadas;
        }

        private void FinalizarProgreso(string bd, string mensaje)
        {
            _adminAutomatizacionPedidoSrvc.UpdateProgress(bd, p =>
            {
                p.IsRunning = false;
                p.UltimoMensaje = mensaje;
            });
        }

        public AutomatizacionPedidoProgDTO GetAutomationProgress(string bd)
        {
            return _adminAutomatizacionPedidoSrvc.GetProgress(bd);
        }

        public async Task<ResultOp<ResultadoPaginado<SMC_LogAutomatizacionDTO>>> ListarLogsPaginadosAsync(ListarPaginadoPeticion peticion, string bd)
        {
            return ResultOp<ResultadoPaginado<SMC_LogAutomatizacionDTO>>.
            Ok(await _logsAutomatizcacionRepository.ListarPaginadoAsync(peticion, bd, null, null));
        }
    }
}