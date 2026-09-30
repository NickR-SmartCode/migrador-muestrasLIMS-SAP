using System;
using System.Collections.Generic;
using System.Text;

namespace DTO
{
    public class EstadoMuestraDTO
    {

        public string TextId { get; set; } = "";
        public DateTime? ChangedOn { get; set; }
        public long? SampleNumber { get; set; }
        public long? Project { get; set; }
        public string Description { get; set; } = "";
        public long? Customer { get; set; }
        public long? ReportNumber { get; set; }
        public string Lab { get; set; } = "";
        public string XFormato { get; set; } = "";
        public string Country { get; set; } = "";
        public long? CostItem { get; set; }
        public string Analysis { get; set; } = "";
        public long? CostItemTl { get; set; }
        public string TestListName { get; set; } = "";
        public DateTime? DateReviewed { get; set; }
        public DateTime? LoginDate { get; set; }
        public DateTime? XFechaIni { get; set; }
        public string TPoNumber { get; set; } = "";
        public string StatusLims { get; set; } = "";
        public string XClaveCliente { get; set; } = "";
        public int? Cancelado { get; set; }
        public string Obs { get; set; } = "";
        public string DraftKey { get; set; } = "";
        public long? DocEntry { get; set; }
        public DateTime? FechaEmision { get; set; }
        public string Especie { get; set; } = "";
        public DateTime? FechaAprobacion { get; set; }

        public string? ClienteRuc { get; set; } 
        public string? ClienteRazonSocial { get; set; }
        public string? ArticuloDescripcion { get; set; }
        public string? ItemCodeSAP { get; set; }
        public string? CardCodeSAP { get; set; }
        public int? DocNumSAP { get; set; }
        public int? SeriesSAP { get; set; }
    }
}
