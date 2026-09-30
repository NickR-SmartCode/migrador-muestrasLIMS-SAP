using System;
using System.Collections.Generic;
using System.Text;

namespace DTO.SAPB1
{
    public class SapOrderDTO
    {
        public string CardCode { get; set; } = string.Empty;
        public string DocDueDate { get; set; } = string.Empty;
        public List<SapOrderLineDTO> DocumentLines { get; set; } = new();
        public string U_ProjectoLims { get; set; } = string.Empty;
        public string PaymentGroupCode { get; set; } = string.Empty;
    }

    public class SapOrderLineDTO
    {
        public string ItemCode { get; set; } = string.Empty;
        public double Quantity { get; set; }
        public string TaxCode { get; set; } = string.Empty;
        public decimal UnitPrice { get; set; }
        public string WarehouseCode { get; set; } = string.Empty;
        public string U_IntegrationCode { get; set; } = string.Empty;
        public string U_SMC_MUESTRAS_LIMS { get; set; } = string.Empty;
    }

    public class SapOrderCreationResponseDTO 
    {
        public double DocNum { get; set; }
        public long DocEntry { get; set; }
        public double Series { get; set; }
        public HashSet<string> FilasExitosasKeys { get; set; } = new();
        public Dictionary<string, string> MuestrasSinMatchQuotation { get; set; } = new();
    }


    public class MuestraIdentificadorDTO
    {
        public long Project { get; set; }
        public long Customer { get; set; }
        public string Analysis { get; set; } = string.Empty;
        public long? CostItemTl { get; set; }
        public long SampleNumber { get; set; }
        public string ItemCodeSAP { get; set; } = string.Empty;
    }




    public class ProcesarMuestraSAPDTO
    {
        public string CardCode { get; set; } = string.Empty;
        public List<string> ItemCodes { get; set; } = new List<string>();
        public List<MuestraIdentificadorDTO> Muestras { get; set; } = new();
    }
/*
    public class ProcesarMuestraSAPDTO
    {
        public string CardCode { get; set; } = string.Empty;
        public List<ProcesarMuestraSAPItemCodesDTO> ItemCodes { get; set; } = [];
        public List<MuestraIdentificadorDTO> Muestras { get; set; } = [];
    }

    public class ProcesarMuestraSAPItemCodesDTO
    {
        public string ItemCode { get; set; } = string.Empty;
        public string CostItemTl { get; set; } = string.Empty;
    }*/
}
