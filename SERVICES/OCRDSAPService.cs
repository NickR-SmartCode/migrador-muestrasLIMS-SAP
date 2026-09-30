using DAO.SAPLINKER;
using DTO;
using INTERFACES;
using Sap.Data.Hana;



namespace SERVICES
{
    public class OCRDSAPService : IOCRDSAPService
    {
        private readonly OCRDSAPDAO _dao;

        public OCRDSAPService(OCRDSAPDAO dao)
        {
            _dao = dao;
        }

        public async Task<ResultOp<List<OCRDSAPDTO>>> ListarAsync(string busqueda)
        {
            return ResultOp<List<OCRDSAPDTO>>.
            Ok(await _dao.ListarAsync("", new string[] {@"""CardCode""", @"""CardName""", @"""U_LIMSReferenceNumber"""},
             new List<FiltroBusqueda>() { new FiltroBusqueda(@"""CardCode"" LIKE ? OR ""CardName"" LIKE ?", "b",  $"%{busqueda}%"), new FiltroBusqueda("1=1", "b", $"%{busqueda}%")}, null));
        }
    }
}