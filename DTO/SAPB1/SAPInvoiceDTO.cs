using System;
using System.Collections.Generic;
using System.Text;

namespace DTO.SAPB1
{
    public class SapInvoiceDTO
    {
        public string CardCode { get; set; } = string.Empty;
        public string CardName { get; set; } = string.Empty;
        public string DocDueDate { get; set; } = string.Empty;
        public string DocDate { get; set; } = string.Empty;
        public List<SapInvoiceLineDTO> DocumentLines { get; set; } = new();
        public string Indicator { get; set; } = "01";
        public string PaymentGroupCode { get; set; } = string.Empty;

        public string DocObjectCode { get; set; } = "13";
    }

    public class SapInvoiceLineDTO
    {
        public int BaseType { get; set; } = 17;
        public int BaseEntry { get; set; }

        public int BaseLine { get; set; }
        public string WTLiable { get; set; } = "tNO";
        public string U_EXX_GRUPODET { get; set; } = "";
        public string PaymentGroupCode { get; set; } = "";
    }
    public class SapPfInvoiceLineDTO
    {
        public string ItemCode { get; set; } = string.Empty;
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public string TaxCode { get; set; } = "IGV";
        public string WTLiable { get; set; } = "tNO";
        public string U_EXX_GRUPODET { get; set; } = "";
        public string PaymentGroupCode { get; set; } = "";
        public string AccountCode { get; set; } = "";
        public string FreeText { get; set; } = string.Empty;

        public string U_SMC_TipoAfecIGV { get; set; } = "";
        public string U_SMC_MUESTRAS_LIMS { get; set; } = string.Empty;
        public int U_SMC_LOTE { get; set; }
        public string U_SMC_AGRUPADO { get; set; } = string.Empty;
        public string U_Identification { get; set; } = string.Empty;

        public string U_SMC_RDLIMS1 { get; set; } = string.Empty;
        public string U_SMC_RDLIMS { get; set; } = string.Empty;
        
    }
}
