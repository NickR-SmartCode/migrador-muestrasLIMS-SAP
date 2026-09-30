using System;
using System.Collections.Generic;
using System.Text;

namespace DTO
{
    public class AprobacionesDTO
    {
        public int IdModulo { get; set; }
        public int IdEtapa { get; set; }
        public int AutorizacionesRequeridas { get; set; }
        public int RechazosRequeridos { get; set; }
        public int IdModelo { get; set; }
        public string Condicion { get; set; }

    }

    public class AprobacionUsuarioDTO
    {
        public string NombreEtapa { get; set; }
        public int IdEtapa { get; set; }
        public string NombreUsuario { get; set; }
        public int EstadoAprobacion { get; set; }
        public int IdModuloAprobacionModelo { get; set; }
        public string Comentario { get; set; }
        public string FechaAprobacion { get; set; }


    }

    public class TurnoAprobarDTO
    {
        public int IdModuloAprobacionModelo { get; set; }
        public string Usuario { get; set; }
        public string Nombre { get; set; }
        public string Email { get; set; }
        public bool correoEnviado { get; set; }
    }

    public class AprobacionEmailParametros
    {
        public string Param1 { get; set; }
        public string Param2 { get; set; }
        public string Param3 { get; set; }
    }
}
