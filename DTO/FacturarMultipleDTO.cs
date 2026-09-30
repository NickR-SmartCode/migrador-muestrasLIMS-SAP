using System;
using System.Collections.Generic;
using System.Text;

namespace DTO
{
    public class FacturarMultipleDTO
    {
        public List<FacturarMultipleLineaDTO> Lineas { get; set; } = [];
        public string CardCode { get; set; } = string.Empty;
    }
    public class FacturarMultipleLineaDTO { 
        public long BaseEntry { get; set; }
        public int BaseLine { get; set; } 
    }
}
