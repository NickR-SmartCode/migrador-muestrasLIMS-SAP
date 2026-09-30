using System;
using System.Collections.Generic;
using System.Text;

namespace DTO
{
    public class ResultadoPaginado<T>
    {
        public int TotalSinFiltrar { get; set; }
        public int TotalFiltrado { get; set; }
        public List<T> Data { get; set; } = new List<T>();
    }
    public class ListarPaginadoPeticion
    {
        public int Pagina { get; set; } = 1;
        public int Cantidad { get; set; } = 10;
        public string? Busqueda { get; set; }
        public string? Estado { get; set; } = string.Empty;
        public string? FechaIni { get;  set; }
        public string? FechaFin { get; set; }
        public int Offset
        {
            get
            {
                return (Pagina - 1) * Cantidad;
            }
        }
    }
}
