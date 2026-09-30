using DTO;

namespace INTERFACES
{
    public interface IOCRDSAPService
    {
        public Task<ResultOp<List<OCRDSAPDTO>>> ListarAsync(string busqueda);
    }
}