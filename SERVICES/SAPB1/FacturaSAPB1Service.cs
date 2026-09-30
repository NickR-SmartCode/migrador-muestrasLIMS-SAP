using DTO;
using DTO.SAPB1;
using DTO.SAPB1ServiceLayer;
using INTERFACES.Services.SAPB1;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace SERVICES.SAPB1
{
    public class FacturaSAPB1Service : ServiceLayerBase, IFacturaSAPService
    {
        public FacturaSAPB1Service(IOptions<ServiceLayerSettings> options,
            IHttpClientFactory httpClientFactory, IMemoryCache cache)
            : base(options, httpClientFactory, cache) { }

        public async Task<ResultOp<SapOrderCreationResponseDTO>> CrearFacturaAsync(SapInvoiceDTO factura)
        {
            return await PostAsync<SapInvoiceDTO, SapOrderCreationResponseDTO>("Invoices", factura);
        }
        public async Task<ResultOp<SapOrderCreationResponseDTO>> CrearFacturaBorradorAsync(dynamic factura)
        {
            return await PostAsync<dynamic, SapOrderCreationResponseDTO>("Drafts", factura);
        }
    }
}
