using DTO.SAPB1;
using System;
using System.Collections.Generic;
using System.Text;

namespace DTO
{
    public class FacturarPreFTEnSAPDTO
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
        public int? Analysis {get; set;} 
        public int? CostItemTL {get; set;}
        public string Description { get; set; } = string.Empty;
        public string Moneda {get ;set;} = string.Empty;
    }

    public class FacturarPreFTEnSAPReqDTO {
        public List<EstadoMuestrasPreFT> Identificadores { get; set; } = [];
    }


    public class FacturarPreFTEnSAPResDTO
    {
        public int DocEntry { get; set; }
        public int DocNum { get; set; }
        public int Series { get; set; }
    }
}
