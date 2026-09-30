using System;
using System.Collections.Generic;
using System.Text;

namespace DTO
{
    public class ChequeTestDTO
    {
        public int IdCheque { get; set; }
        public string Nombre { get; set; }
        public DateTime Fecha { get; set; }
        public string Moneda { get; set; }
        public decimal Monto { get; set; }
        public int EstadoAprobacion { get; set; }
        public int IdModuloAprobacionModelo { get; set; }
    }
}
