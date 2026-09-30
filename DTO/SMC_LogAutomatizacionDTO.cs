using System;
using System.Collections.Generic;
using System.Text;

namespace DTO
{
    public class SMC_LogAutomatizacionDTO
    {

        public int Id { get; set; }
        public DateTime Fecha_Ejecucion { get; set; }
        public long SampleNumber { get; set; }
        public long? Project { get; set; }
        public long? Customer { get; set; }
        public string Analysis { get; set; } = string.Empty;
        public long? CostItemTl { get; set; }
        public string CardCodeSAP { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public string Mensaje { get; set; } = string.Empty;
        public int? DocNumSAP { get; set; }
        public int? SeriesSAP { get; set; }
    }
}
