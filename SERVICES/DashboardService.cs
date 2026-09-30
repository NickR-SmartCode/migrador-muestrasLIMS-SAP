using DAO;
using DTO;
using DTO.SAPB1;
using INTERFACES.Repositories;
using INTERFACES.Services;
using Microsoft.Data.SqlClient;
using Sap.Data.Hana;
using System.Text.Json;
using System.Text.RegularExpressions;


namespace SERVICES
{
    public class DashboardService : IDashboardService
    {
        private readonly IDashboardRepository _repo;
        private readonly IEstadoMuestrasPreFTRepository _estadoMuestrasPreFTRepo;
        private readonly IEstadoMuestrasPreFTService _estadoMuestrasPreFTService;

        public DashboardService(IDashboardRepository repo, IEstadoMuestrasPreFTRepository estadoMuestrasPreFTRepo,
            IEstadoMuestrasPreFTService estadoMuestrasPreFTService)
        {
            _repo = repo;
            _estadoMuestrasPreFTRepo = estadoMuestrasPreFTRepo;
            _estadoMuestrasPreFTService = estadoMuestrasPreFTService;
        }

        public async Task<ResultOp<Dictionary<string, DashboardMetricsDTO>>>
      ObtenerResumenDashboardAsync(int anio, string bd)
        {
            var tLims = _repo.ObtenerMuestrasLimsRawAsync(anio, bd);
            var tCustomers = _repo.ObtenerClientesMaestrosSapAsync(bd);
            var tItems = _repo.ObtenerArticulosMaestrosSapAsync(bd);
            var tSapTrans = _repo.ObtenerVincualcionesSapRawAsync(anio, bd);
            var tPrices = _repo.ObtenerPreciosUltimasCotizacionesAsync(bd);

            await Task.WhenAll(tLims, tCustomers, tItems, tSapTrans, tPrices);

            var itemNamesLookup = tItems.Result.GroupBy(x => x.IntegrationCode).ToDictionary(g => g.Key, g => g.First().ItemName);
            var customerLookup = tCustomers.Result.GroupBy(x => x.LimsReference).ToDictionary(g => g.Key, g => g.First());
            var sapTransLookup = tSapTrans.Result.ToLookup(x => x.SampleNumber);
            var limsData = tLims.Result;

            var priceLookup = new Dictionary<string, decimal>();
            foreach (var p in tPrices.Result)
            {
                string key = $"{p.CardCode}_{p.IntegrationCode}";
                if (!priceLookup.ContainsKey(key)) priceLookup.Add(key, p.UnitPrice);
            }

            var resumen = new Dictionary<string, DashboardMetricsDTO>
            {
                ["W1_Facturacion"] = new DashboardMetricsDTO { Titulo = "Eficiencia de Facturación" },
                ["W2_Match_Cant"] = new DashboardMetricsDTO { Titulo = "Estado de Pedidos" },
                ["W3_Match_Monto"] = new DashboardMetricsDTO { Titulo = "Valorización (PEN)" },
                ["W4_Cant_Analisis"] = new DashboardMetricsDTO { Titulo = "Ranking de Análisis" },
                ["W5_Cant_Paquete"] = new DashboardMetricsDTO { Titulo = "Ranking de Paquetes" },
                ["W6_Monto_Analisis"] = new DashboardMetricsDTO { Titulo = "Monto por Análisis" },
                ["W7_Monto_Paquete"] = new DashboardMetricsDTO { Titulo = "Monto por Paquete" },
                ["W8_Ranking_Clientes"] = new DashboardMetricsDTO { Titulo = "Ranking de Clientes" },
                ["W9_Facturacion_Detalle"] = new DashboardMetricsDTO { Titulo = "Facturaciones Firmes" },
                ["W10_Facturacion_Detalle_Borrador"] = new DashboardMetricsDTO { Titulo = "Facturaciones Borrador" }
            };

            var limsPorMes = limsData.GroupBy(x => x.Mes);
            foreach (var grupoMes in limsPorMes)
            {
                int mes = grupoMes.Key;

                resumen["W1_Facturacion"].ValoresMensuales.Add(new MetricValueDTO
                {
                    Mes = mes,
                    ValorPrincipal = grupoMes.Count(),
                    ValorSecundario = grupoMes.Count(m => sapTransLookup[m.SampleNumber].Any(s => s.IsInvoiced))
                });

                resumen["W2_Match_Cant"].ValoresMensuales.Add(new MetricValueDTO
                {
                    Mes = mes,
                    ValorPrincipal = grupoMes.Count(m => sapTransLookup[m.SampleNumber].Any()),
                    ValorSecundario = grupoMes.Count(m => !sapTransLookup[m.SampleNumber].Any())
                });

                decimal mReal = 0; decimal mPotencial = 0;
                foreach (var m in grupoMes)
                {
                    string matchCode = (m.CostItemTl > 0) ? m.CostItemTl.ToString()! : m.Analysis;
                    var trans = sapTransLookup[m.SampleNumber];
                    if (trans.Any())
                    {
                        mReal += trans.Sum(t => (t.DocCur == "USD" ? t.LineTotal * (t.DocRate > 0 ? t.DocRate : 1) : t.LineTotal));
                    }
                    else if (customerLookup.TryGetValue(m.Customer, out var cli))
                    {
                        if (priceLookup.TryGetValue($"{cli.CardCode}_{matchCode}", out decimal price)) mPotencial += price;
                    }
                }
                resumen["W3_Match_Monto"].ValoresMensuales.Add(new MetricValueDTO { Mes = mes, ValorPrincipal = mReal, ValorSecundario = mPotencial });
            }

            resumen["W4_Cant_Analisis"].Series = limsData
                .GroupBy(x => new { x.Mes, x.Analysis })
                .Select(g => new ChartDataPoint
                {
                    Mes = g.Key.Mes,
                    Etiqueta = itemNamesLookup.TryGetValue(g.Key.Analysis, out var n) ? n : $"{g.Key.Analysis} - sin match",
                    Valor = g.Count()
                }).ToList();

            resumen["W5_Cant_Paquete"].Series = limsData.Where(x => x.CostItemTl > 0)
                .GroupBy(x => new { x.Mes, CI = x.CostItemTl.ToString() })
                .Select(g => new ChartDataPoint
                {
                    Mes = g.Key.Mes,
                    Etiqueta = itemNamesLookup.TryGetValue(g.Key.CI!, out var n) ? n : $"{g.Key.CI} - sin match",
                    Valor = g.Count()
                }).ToList();

            resumen["W8_Ranking_Clientes"].Series = limsData
                .Where(m => customerLookup.ContainsKey(m.Customer))
                .GroupBy(x => new { x.Mes, Cliente = customerLookup[x.Customer].CardName })
                .Select(g => new ChartDataPoint
                {
                    Mes = g.Key.Mes,
                    Etiqueta = g.Key.Cliente,
                    Valor = g.Count()
                }).ToList();


            var limsPackageKeys = new HashSet<string>(limsData.Where(x => x.CostItemTl > 0).Select(x => $"{x.SampleNumber}_{x.CostItemTl}"));
            var facturadasSap = tSapTrans.Result.Where(x => x.IsInvoiced).ToList();


            resumen["W6_Monto_Analisis"].Series = facturadasSap
                .Where(sap => !limsPackageKeys.Contains($"{sap.SampleNumber}_{sap.U_IntegrationCode}"))
                .GroupBy(sap => new { sap.MesFactura, sap.U_IntegrationCode })
                .Select(g => new ChartDataPoint
                {
                    Mes = Convert.ToInt32(g.Key.MesFactura),
                    Etiqueta = itemNamesLookup.TryGetValue(g.Key.U_IntegrationCode, out var name) ? name : $"{g.Key.U_IntegrationCode} - sin match",
                    Valor = g.Sum(s => (s.DocCur == "USD" ? s.LineTotal * (s.DocRate > 0 ? s.DocRate : 1) : s.LineTotal))
                }).ToList();


            resumen["W7_Monto_Paquete"].Series = facturadasSap
                .Where(sap => limsPackageKeys.Contains($"{sap.SampleNumber}_{sap.U_IntegrationCode}"))
                .GroupBy(sap => new { sap.MesFactura, sap.U_IntegrationCode })
                .Select(g => new ChartDataPoint
                {
                    Mes = Convert.ToInt32(g.Key.MesFactura),
                    Etiqueta = itemNamesLookup.TryGetValue(g.Key.U_IntegrationCode, out var name) ? name : $"{g.Key.U_IntegrationCode} - sin match",
                    Valor = g.Sum(s => (s.DocCur == "USD" ? s.LineTotal * (s.DocRate > 0 ? s.DocRate : 1) : s.LineTotal))
                }).ToList();

            var facturadasReales = tSapTrans.Result
    .Where(x => x.IsInvoiced && !string.IsNullOrEmpty(x.InvDocNum))
    .ToList();


            var facturasPorMes = facturadasReales.GroupBy(x => x.MesFactura);

            foreach (var grupoMes in facturasPorMes)
            {
                int mes = 0;
                if (int.TryParse(grupoMes.Key, out var iMes))
                {
                    mes = iMes;
                }
                if (mes <= 0) continue;



                int cantidadFacturasUnicasFirmes = grupoMes
                    .Where(t => t.EsFirme)
                    .Select(t => t.InvDocNum)
                    .Distinct()
                    .Count();

                int count = grupoMes.Where(t => !t.EsFirme).GroupBy(t => t.BorradorDocEntry).Count();



                decimal montoTotalMesFirmes = grupoMes.Where(t => t.EsFirme).Sum(t =>
                    (t.DocCur == "USD" ? t.LineTotal * (t.DocRate > 0 ? t.DocRate : 1) : t.LineTotal)
                );

                decimal montoTotalMesBorrador = grupoMes.Where(t => !t.EsFirme).Sum(t =>
                    (t.DocCur == "USD" ? t.LineTotal * (t.DocRate > 0 ? t.DocRate : 1) : t.LineTotal)
                );


                resumen["W9_Facturacion_Detalle"].ValoresMensuales.Add(new MetricValueDTO
                {
                    Mes = mes,
                    ValorPrincipal = cantidadFacturasUnicasFirmes,
                    ValorSecundario = montoTotalMesFirmes
                });

                resumen["W10_Facturacion_Detalle_Borrador"].ValoresMensuales.Add(new MetricValueDTO
                {
                    Mes = mes,
                    ValorPrincipal = count,
                    ValorSecundario = montoTotalMesBorrador
                });
            }

            return ResultOp<Dictionary<string, DashboardMetricsDTO>>.Ok(resumen);
        }



        public async Task<ResultOp<Dictionary<string, DashboardMetricsDTO>>> ObtenerResumenDashboardPreFTAsync(int anio, string bd)
        {


            List<EstadoMuestrasPreFT> dataPlana = await _estadoMuestrasPreFTRepo.ObtenerPrefacturasPlanasPorAnioAsync(anio, bd);


            decimal tasaDolar = await _estadoMuestrasPreFTService.ObtenerTasaCambioDiaAsync("USD");

            foreach (var pf in dataPlana)
            {
                if (!string.IsNullOrEmpty(pf.Moneda) && pf.Moneda.ToUpper().Contains("DOLAR"))
                {
                    pf.SubTotalLineaPF = pf.SubTotalLineaPF * tasaDolar;
                    pf.ValorUnitarioAnaPF = pf.ValorUnitarioAnaPF * tasaDolar;
                    pf.Moneda = "SOLES";
                }
            }




            var prefacturasLims = dataPlana
                .GroupBy(x => new { x.Customer, x.NroPF })
                .Select(g => new
                {
                    Customer = g.Key.Customer,
                    NroPF = g.Key.NroPF,
                    ClienteNombre = g.First().RazonSocialLIMSParaSAP,
                    MesEmision = g.First().FechaEmision.Month,
                    MontoTotalLIMS = g.Sum(x => x.SubTotalLineaPF),
                    TieneSAP = g.First().DocumentosSAP.Any()
                }).ToList();



            var documentosSAPUnicos = dataPlana
                .SelectMany(x => x.DocumentosSAP)
                .GroupBy(x => x.DocEntry)
                .Select(g => g.First())
                .ToList();


            var resumen = new Dictionary<string, DashboardMetricsDTO>
            {
                ["W1_Facturacion"] = new DashboardMetricsDTO { Titulo = "Eficiencia de Facturación" },
                ["W3_Match_Monto"] = new DashboardMetricsDTO { Titulo = "Valorización (PEN)" },
                ["W4_Cant_Analisis"] = new DashboardMetricsDTO { Titulo = "Ranking de Análisis" },
                ["W8_Ranking_Clientes"] = new DashboardMetricsDTO { Titulo = "Ranking de Clientes" },
                ["W9_Facturacion_Detalle"] = new DashboardMetricsDTO { Titulo = "Facturaciones Firmes" },
                ["W10_Facturacion_Detalle_Borrador"] = new DashboardMetricsDTO { Titulo = "Facturaciones Borrador" }
            };


            for (int mes = 1; mes <= 12; mes++)
            {

                var pfMesLims = prefacturasLims.Where(p => p.MesEmision == mes).ToList();


                var sapMes = documentosSAPUnicos.Where(s => s.MesSAP == mes).ToList();


                resumen["W1_Facturacion"].ValoresMensuales.Add(new MetricValueDTO
                {
                    Mes = mes,
                    ValorPrincipal = pfMesLims.Count,
                    ValorSecundario = pfMesLims.Count(p => p.TieneSAP)
                });



                resumen["W3_Match_Monto"].ValoresMensuales.Add(new MetricValueDTO
                {
                    Mes = mes,
                    ValorPrincipal = sapMes.Where(s => !s.IsDraft).Sum(s => s.TotalSoles),
                    ValorSecundario = pfMesLims.Sum(p => p.MontoTotalLIMS)
                });


                resumen["W9_Facturacion_Detalle"].ValoresMensuales.Add(new MetricValueDTO
                {
                    Mes = mes,
                    ValorPrincipal = sapMes.Count(s => !s.IsDraft),
                    ValorSecundario = sapMes.Where(s => !s.IsDraft).Sum(s => s.TotalSoles)
                });


                resumen["W10_Facturacion_Detalle_Borrador"].ValoresMensuales.Add(new MetricValueDTO
                {
                    Mes = mes,
                    ValorPrincipal = sapMes.Count(s => s.IsDraft),
                    ValorSecundario = sapMes.Where(s => s.IsDraft).Sum(s => s.TotalSoles)
                });
            }

            Func<string, List<string>> extraerAnalisis = (detallePF) =>
            {
                if (string.IsNullOrEmpty(detallePF)) return new List<string>();

                var match = Regex.Match(detallePF, @"Análisis:\s*(.*?)(?=\r|\n|Informes Ensayo:|$)", RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    return match.Groups[1].Value.Split(',')
                                .Select(a => a.Trim())
                                .Where(a => !string.IsNullOrEmpty(a))
                                .ToList();
                }
                return new List<string>();
            };

            var analisisDesglosados = dataPlana.SelectMany(pf =>
            {
                var listaAnalisis = extraerAnalisis(pf.DetalleLinea);
                int cantidadAnalisis = listaAnalisis.Count > 0 ? listaAnalisis.Count : 1;

                return listaAnalisis.Select(nombreAnalisis => new
                {
                    Mes = pf.FechaEmision.Month,
                    Nombre = nombreAnalisis,
                
                    MontoProrrateado = pf.SubTotalLineaPF / cantidadAnalisis
                });
            }).ToList();

            Func<string, string> obtenerDescDePF = (detalleLinea) =>
            {
                try
                {
                    var match = Regex.Match(detalleLinea ?? "", @"SERVICIO:\s?.+");
                    return match.Success ? match.Value.Replace("SERVICIO: ", "").Trim() : string.Empty;
                }
                catch { return string.Empty; }
            };
    
            resumen["W4_Cant_Analisis"].Series = analisisDesglosados
         .GroupBy(x => new { x.Mes, x.Nombre })
        .Select(g => new ChartDataPoint { Mes = g.Key.Mes, Etiqueta = g.Key.Nombre, Valor = g.Count() })
        .ToList();


           
            

            resumen["W8_Ranking_Clientes"].Series = prefacturasLims
                .GroupBy(x => new { Mes = x.MesEmision, x.ClienteNombre })
                .Select(g => new ChartDataPoint { Mes = g.Key.Mes, Etiqueta = g.Key.ClienteNombre, Valor = g.Count() }).ToList();

            return ResultOp<Dictionary<string, DashboardMetricsDTO>>.Ok(resumen);

        }
    }

}

