using System;
using System.Collections.Generic;
using System.Text;

namespace DTO.SAPB1
{
    public class SapQuotationListResponse
    {
        public List<SapQuotationDTO> Value { get; set; } = new();
    }

    public class SapQuotationDTO
    {
        public int DocEntry { get; set; }
        public double Series { get; set; }
        public double DocNum { get; set; }
        public string GroupNum { get; set; } = string.Empty;
        public List<SapOrderLineDTO> DocumentLines { get; set; } = new();
    }
}
