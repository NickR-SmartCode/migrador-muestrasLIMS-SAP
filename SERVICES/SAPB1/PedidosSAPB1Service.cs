using DAO;
using DTO;
using DTO.SAPB1;
using DTO.SAPB1ServiceLayer;
using INTERFACES.Repositories.SAPB1;
using INTERFACES.Services.SAPB1;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Sap.Data.Hana;


namespace SERVICES.SAPB1
{
    public class PedidosSAPB1Service : ServiceLayerBase, IPedidosSAPService
    {
        private readonly IPedidosSAPRepository _pedidosRepository;

        private readonly IFacturaSAPService _facturaSAPService;
        private readonly Conexion _cn;

        public PedidosSAPB1Service(IOptions<ServiceLayerSettings> options,
            IHttpClientFactory httpClientFactory, IMemoryCache cache, IPedidosSAPRepository pedidosRepository,
            IFacturaSAPService facturaSAPService, Conexion cn) : base(options, httpClientFactory, cache)
        {
            _pedidosRepository = pedidosRepository;
            _facturaSAPService = facturaSAPService;
            this._cn = cn;
        }

        public async Task<ResultOp<ResultadoPaginado<PedidoSapDTO>>> ListarPaginadoAsync(ListarPaginadoPeticion peticion, string bd)
        {

            var data = await _pedidosRepository.ListarPaginadoAsync(peticion, bd, "\"DocEntry\"");
            return ResultOp<ResultadoPaginado<PedidoSapDTO>>.Ok(data);

        }

        public async Task<ResultOp<List<PedidoDetalleSapDTO>>> ObtenerDetalleAsync(int docEntry, string bd)
        {
            try
            {
                if (docEntry <= 0)
                    return ResultOp<List<PedidoDetalleSapDTO>>.Fallo("El identificador del pedido no es válido.");

                var detalle = await _pedidosRepository.ObtenerDetalleAsync(docEntry);
                return ResultOp<List<PedidoDetalleSapDTO>>.Ok(detalle);
            }
            catch (Exception ex)
            {
                return ResultOp<List<PedidoDetalleSapDTO>>.Fallo($"Error al obtener el detalle del pedido {docEntry}: {ex.Message}");
            }
        }

        /*  public async Task<ResultOp<SapOrderCreationResponseDTO>> FacturarPedidosAsync(FacturarMultipleDTO facturarMultiple)
          {
              try
              {
                  var lineasFactura = new List<SapInvoiceLineDTO>();

                  foreach (var docEntry in docEntries)
                  {
                      var detalles = await _pedidosRepository.ObtenerDetalleAsync(docEntry);
                      var orden = await _pedidosRepository.ObtenerPorIdAsync(docEntry);
                      decimal tipoCambio = 1;
                      //if(orden.DocCur != "SOL")
                      //{
                                                                  //}
                      foreach (var det in detalles)
                      {
                          decimal lineTotal = det.LineTotal * tipoCambio;

                          string grupoDet = lineTotal > 700 ? "037" : "099";
                          string paymentGroup = grupoDet == "099" ? "-1" : "8";
                          lineasFactura.Add(new SapInvoiceLineDTO
                          {
                              BaseEntry = docEntry,
                              BaseLine = det.LineNum,
                              U_EXX_GRUPODET = grupoDet,
                              PaymentGroupCode = paymentGroup
                          });
                      }
                  }


                  var nuevaFactura = new SapInvoiceDTO
                  {
                      CardCode = cardCode,
                      DocDueDate = DateTime.Now.ToString("yyyy-MM-dd"),
                      DocumentLines = lineasFactura,
                  };

                  var resultSap =  await _facturaSAPService.CrearFacturaAsync(nuevaFactura);

                  //TODO: Crear un objeto de repsuesta de creacion de factura
                  //if (resultSap.Datos is SapOrderCreationResponseDTO facturaCreada
                                    //{
                                                                                                                              
                                                                                                                                                                  //}
                  return resultSap;
              }
              catch (Exception ex)
              {
                  return ResultOp<SapOrderCreationResponseDTO>.Fallo("Error al procesar la factura: " + ex.Message);
              }
          }
  */

        public async Task<ResultOp<SapOrderCreationResponseDTO>> FacturarPedidosAsync(FacturarMultipleDTO req)
        {
            try
            {
                if (req.Lineas == null || !req.Lineas.Any())
                    return ResultOp<SapOrderCreationResponseDTO>.Fallo("No se seleccionaron líneas para facturar.");

                decimal tasaCambioHoy = await ObtenerTasaCambioDiaAsync("USD");

                var lineasFactura = new List<SapInvoiceLineDTO>();
                var gruposPorPedido = req.Lineas.GroupBy(x => x.BaseEntry);

                string? paymentGroup = "";
                string? monedaFactura = "";
                decimal totalAcumuladoDocCur = 0;

                foreach (var grupo in gruposPorPedido)
                {
                    int docEntryPedido = (int)grupo.Key;

                    var cabecera = await _pedidosRepository.ObtenerPorIdAsync(docEntryPedido);
                    var detallesHana = await _pedidosRepository.ObtenerDetalleAsync(docEntryPedido);

                    if (cabecera == null) continue;

                    if (cabecera.DocStatus == "C")
                        return ResultOp<SapOrderCreationResponseDTO>.Fallo($"El pedido N° {cabecera.DocNum} ya está CERRADO.");

                    if (string.IsNullOrEmpty(monedaFactura))
                    {
                        monedaFactura = cabecera.DocCur;
                        paymentGroup = cabecera.GroupNum;
                    }

                    foreach (var lineaReq in grupo)
                    {
                        var det = detallesHana.FirstOrDefault(l => l.LineNum == lineaReq.BaseLine);
                        if (det == null || det.LineStatus == "C") continue;

                        totalAcumuladoDocCur += det.LineTotal;

                        lineasFactura.Add(new SapInvoiceLineDTO
                        {
                            BaseType = 17,
                            BaseEntry = docEntryPedido,
                            BaseLine = det.LineNum,
                        });
                    }
                }

                decimal factorConversion = (monedaFactura == "SOL" || monedaFactura == "PEN") ? 1 : tasaCambioHoy;
                decimal totalFacturaSoles = totalAcumuladoDocCur * factorConversion;

                string grupoDetFinal = totalFacturaSoles > 700 ? "037" : "099";

                lineasFactura.ForEach(l => l.U_EXX_GRUPODET = grupoDetFinal);

                var nuevaFactura = new SapInvoiceDTO
                {
                    CardCode = req.CardCode,
                    DocDueDate = DateTime.Now.ToString("yyyy-MM-dd"),
                    Indicator = "01",
                    PaymentGroupCode = paymentGroup,
                    DocumentLines = lineasFactura,
                    DocObjectCode = "13"
                };

                var resSap = await _facturaSAPService.CrearFacturaBorradorAsync(nuevaFactura);

                if (resSap.Exito && resSap.Datos != null && grupoDetFinal == "037")
                {
                    int docEntryCreado = (int)resSap.Datos.DocEntry;
                    await ActualizarCuotasDetraccionHanaAsync(docEntryCreado);
                }

                return resSap;
            }
            catch (Exception ex)
            {
                return ResultOp<SapOrderCreationResponseDTO>.Fallo("Error crítico: " + ex.Message);
            }
        }

        private async Task<decimal> ObtenerTasaCambioDiaAsync(string moneda)
        {
            using var cn = _cn.CrearConexionHana("");
            await cn.OpenAsync();
            string query = @"SELECT TOP 1 ""Rate"" FROM ORTT WHERE ""Currency"" = ? AND ""RateDate"" = TO_DATE(CURRENT_DATE)";
            using var cmd = new HanaCommand(query, cn);
            cmd.Parameters.Add(new HanaParameter("", moneda));
            var result = await cmd.ExecuteScalarAsync();

            return result != null ? Convert.ToDecimal(result) : 1;
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
        public async Task<ResultOp<SapOrderCreationResponseDTO>> CrearOrdenAsync(object orden)
        {
            return await PostAsync<object, SapOrderCreationResponseDTO>("Orders", orden);
        }

        public async Task<ResultOp<SapQuotationDTO>> ObtenerUltimaCotizacionHanaAsync(string cardCode)
        {
            return ResultOp<SapQuotationDTO>.Ok(await _pedidosRepository.ObtenerUltimaCotizacionHanaAsync(cardCode));
        }
        public async Task<ResultOp<List<SapQuotationDTO>>> ListarCotizacionesVigentesAnioActualAsync(string cardCode, string bd)
        {
            return ResultOp<List<SapQuotationDTO>>.Ok(await _pedidosRepository.ListarCotizacionesVigentesAnioActualAsync(cardCode, bd));
        }

        public async Task<ResultOp<ResultadoPaginado<PedidoDetalleConCabeceraSapDTO>>> ListarDetallesPaginadosAsync(PedidoDetallesListarPaginadosReqDTO peticion)
        {
            return ResultOp<ResultadoPaginado<PedidoDetalleConCabeceraSapDTO>>.
                Ok(await _pedidosRepository.ListarDetallesPaginadosAsync(peticion));
        }
    }
}
