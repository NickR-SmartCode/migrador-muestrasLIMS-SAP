using System;
using System.Collections.Generic;
using System.Text;

namespace DTO
{
    public class SMC_MuestraProcesadaDTO
    {
        public long Project { get; set; }
        public long Customer { get; set; }
        public string Analysis { get; set; } = string.Empty;
        public long SampleNumber { get; set; }
        public int Series { get; set; }
        public int DocNum { get; set; }
        public int CostItemTL { get; set; }
    }
}
