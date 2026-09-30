using DTO;
using DTO.SAPB1;
using System;
using System.Collections.Generic;
using System.Text;

namespace INTERFACES.Services.SAPB1
{
    public interface IFacturaSAPService
    {
        Task<ResultOp<SapOrderCreationResponseDTO>> CrearFacturaAsync(SapInvoiceDTO factura);
        Task<ResultOp<SapOrderCreationResponseDTO>> CrearFacturaBorradorAsync(dynamic factura);

    }
}
