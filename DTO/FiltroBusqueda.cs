using Microsoft.Data.SqlClient;

namespace DTO;

public class FiltroBusqueda
{
    public string Condicion { get; set; }
    public SqlParameter Parametro { get; set; }

    public FiltroBusqueda(string condicion, string nombreParam, object valor)
    {
        Condicion = condicion;
        Parametro = new SqlParameter(nombreParam, valor ?? DBNull.Value);
    }
}