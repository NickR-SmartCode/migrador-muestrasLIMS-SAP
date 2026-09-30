

namespace DTO.SAPB1
{
    public class PedidoSapDTO
    {
        public int DocEntry { get; set; }
        public double Series { get; set; }
        public int DocNum { get; set; }
        public string CardCode { get; set; } = string.Empty;
        public string CardName { get; set; } = string.Empty;
        public DateTime DocDate { get; set; }
        public decimal DocTotal { get; set; }
        public string DocCur { get; set; } = string.Empty;
        public string DocStatus { get; set; } = string.Empty;
        public string? SerieFactura { get; set; }
        public int? NumFactura { get; set; }
        public string U_ProjectoLims { get; set; } = string.Empty;
        public string GroupNum { get; set; } = string.Empty;
        public decimal DocRate { get; set; }
    }

    public class PedidoDetalleSapDTO
    {
        public string ItemCode { get; set; } = string.Empty;
        public string Dscription { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal LineTotal { get; set; }
        public int LineNum { get; set; }
        public string? U_IntegrationCode { get; set; }
        public string LineStatus { get; set; } = string.Empty;
        public string SampleNumber { get; set; } = string.Empty;
        public bool EsPreliminar { get; set; }
        public string EstadoFacturacion { get; set; } = string.Empty;
    }

    public class PedidoDetalleConCabeceraSapDTO
    {
        public PedidoSapDTO Pedido { get; set; } = new();
        public PedidoDetalleSapDTO Linea { get; set; } = new();
    }

    public class PedidoDetallesListarPaginadosReqDTO : ListarPaginadoPeticion
    {
        public string? Cliente { get; set; }
    }
}