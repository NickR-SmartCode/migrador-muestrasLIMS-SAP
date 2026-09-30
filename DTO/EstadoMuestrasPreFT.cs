using System;
using System.Collections.Generic;
using System.Text;

namespace DTO.SAPB1
{
    public class EstadoMuestrasPreFT
    {
        public string Project { get; set; } = string.Empty;
        public string Customer { get; set; } = string.Empty;
        public string RazSocCliPF { get; set; } = string.Empty;
        public string RucCliPF { get; set; } = string.Empty;
        public DateTime FechaEmision { get; set; }
        public string NroPF { get; set; } = string.Empty;
        public decimal SubTotalLineaPF { get; set; }
        public decimal ValorUnitarioAnaPF { get; set; }
        public int Cantidad { get; set; }
        public string DetalleLinea { get; set; } = string.Empty;
        public string TipoServicio { get; set; } = string.Empty;
        public string StatusLIMS { get; set; } = string.Empty;
        public int? Analysis { get; set; }
        public int? CostItemTL { get; set; }
        public string Description { get; set; } = string.Empty;
        public string Moneda { get; set; } = string.Empty;
        public int SampleNumber { get; set; }

        public string Cliente { get; set; } = string.Empty;
        public int? DocNumSAP { get; set; }
        public int? DocEntrySAP { get; set; }
        public int? SeriesSAP { get; set; }
        public int Orden { get; set; }
        public bool? IsDraftSAP { get; set; } = null;
        public string DetallePF { get; set; } = string.Empty;


        public List<MatchSAP> DocumentosSAP { get; set; } = new List<MatchSAP>();

        public string RucLIMSParaSAP
        {
            get
            {
                return "C" + Cliente.Split("-")[0];
            }
        }

        public string RazonSocialLIMSParaSAP
        {
            get
            {
                return Cliente.Split("-")[1];
            }
        }
    }

    public class MatchSAP
    {
        public int DocEntry { get; set; }
        public int DocNum { get; set; }
        public int Series { get; set; }
        public bool IsDraft { get; set; }
        public DateTime DocDate { get; set; }
        public int MesSAP => DocDate.Month;

        public decimal DocTotal { get; set; }
        public string DocCur { get; set; } = string.Empty;
        public decimal DocRate { get; set; }



        public decimal TotalSoles => DocCur == "USD" ? DocTotal * (DocRate > 0 ? DocRate : 1) : DocTotal;
    }

    public class EstadoMuestrasPreFTListarPaginado : ListarPaginadoPeticion
    {
        public string? ClienteRuc { get; set; }
        public string? NroPF { get; set; }
        public string? StatusLIMS { get; set; }
    }
}
