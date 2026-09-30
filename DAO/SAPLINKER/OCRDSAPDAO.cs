using DTO;
using Sap.Data.Hana;

namespace DAO.SAPLINKER;

public class OCRDSAPDAO : HANADAOBase<OCRDSAPDTO>
{
    public OCRDSAPDAO(Conexion conexion) : base(conexion, @"""OCRD""") {}
    protected override OCRDSAPDTO Mapear(HanaDataReader reader)
    {
        var ins = new OCRDSAPDTO();
        ins.CardCode = ObtenerValor<string>(reader, "CardCode") ?? "";
        ins.CardName = ObtenerValor<string>(reader, "CardName") ?? "";
        ins.U_LIMSReferenceNumber = ObtenerValor<string>(reader, "U_LIMSReferenceNumber") ?? "";

        return ins;
    }

   
}