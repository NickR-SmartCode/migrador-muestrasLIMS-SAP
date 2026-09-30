using System;
using System.Collections.Generic;
using System.Text;

namespace DTO
{
    public class EtapaAutorizacionDTO
    {
        public int IdEtapaAutorizacion { get; set; }
        public string NombreEtapa { get; set; }
        public string DescripcionEtapa { get; set; }
        public int AutorizacionesRequeridas { get; set; }
        public int RechazosRequeridos { get; set; }
        public bool Estado { get; set; }

        public List<EtapaAutorizacionDetalleDTO> Detalle { get; set; } = new List<EtapaAutorizacionDetalleDTO>();
    }

    public class EtapaAutorizacionDetalleDTO
    {
        public int IdEtapaAutorizacion { get; set; }
        public int IdUsuario { get; set; }
    }
}
