using System;
using System.Collections.Generic;
using System.Text;

namespace DTO
{
    public class AutomatizacionPedidoProgDTO
    {
        public string ClienteActual { get; set; } = "Iniciando...";
        public string ProyectoActual { get; set; } = "...";
        public bool IsPaused { get; set; }
        public int TotalFilas { get; set; }
        public int Procesadas { get; set; }
        public bool IsRunning { get; set; }
        public string UltimoMensaje { get; set; } = string.Empty;
        public List<AccionConsolaDTO> UltimasAcciones { get; set; } = new List<AccionConsolaDTO>();
    }
    public class AccionConsolaDTO
    {
        public string Mensaje { get; set; } = "";
        public DateTime Fecha { get; set; }
    }
}
