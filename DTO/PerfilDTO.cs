using System;
using System.Collections.Generic;
using System.Text;

namespace DTO
{
    public class PerfilDTO
    {
        public int IdPerfil { get; set; }
        public string Descripcion { get; set; }
        public int IdMenuInicial { get; set; }
        public string MenuInicial { get; set; }
        public bool Estado { get; set; }
    }
}
