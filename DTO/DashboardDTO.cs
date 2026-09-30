using System;
using System.Collections.Generic;
using System.Text;

namespace DTO
{
    public class LimsRawDataDTO
    {
        public int SampleNumber { get; set; }
        public string Analysis { get; set; } = string.Empty;
        public int? CostItemTl { get; set; }
        public int Customer { get; set; }
        public string Project { get; set; } = string.Empty;
        public DateTime? FechaEmision { get; set; }
        public int Mes => FechaEmision?.Month ?? 0;
        public int Anio => FechaEmision?.Year ?? 0;
    }


    public class SapCustomerMasterDTO
    {
        public long LimsReference { get; set; }
        public string CardCode { get; set; } = string.Empty;
        public string CardName { get; set; } = string.Empty;
    }


    public class SapItemMasterDTO
    {
        public string IntegrationCode { get; set; } = string.Empty;
        public string ItemCode { get; set; } = string.Empty;
        public string ItemName { get; set; } = string.Empty;
    }


    public class SapIntegrationDataDTO
    {
        public int DocEntry { get; set; }
        public int DocNum { get; set; }
        public string DocStatus { get; set; } = string.Empty;
        public long SampleNumber { get; set; }
        public string U_IntegrationCode { get; set; } = string.Empty;
        public decimal LineTotal { get; set; }
        public bool IsInvoiced { get; set; }
        public int BaseLine {get; set;}
        public bool EsFirme {get; set;}
        public string BorradorDocEntry {get; set; } = string.Empty;

        public string DocCur { get; set; } = string.Empty;
        public decimal DocRate { get; set; }
        public string InvDocNum { get; set; } = string.Empty;
        public string MesFactura { get; set; } = string.Empty;
        public DateTime DocDate { get; set; }
        public int Mes { 
            get 
            {
                return DocDate.Month;
            }
        }
            
    }


    public class SapQuotationPriceDTO
    {
        public string CardCode { get; set; } = string.Empty;
        public string IntegrationCode { get; set; } = string.Empty;
        public decimal UnitPrice { get; set; }
    }

    public class MetricValueDTO
    {
        public int Mes { get; set; }
        public decimal ValorPrincipal { get; set; }
        public decimal ValorSecundario { get; set; }
    }

    public class DashboardMetricsDTO
    {
        public string Titulo { get; set; } = "";
        public List<MetricValueDTO> ValoresMensuales { get; set; } = new();
        public List<ChartDataPoint> Series { get; set; } = new();
    }
   

    public class ChartDataPoint
    {
        public string Etiqueta { get; set; } = "";
        public decimal Valor { get; set; }
        public int Mes { get; set; }
    }
}
