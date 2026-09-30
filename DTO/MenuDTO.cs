using System;
using System.Collections.Generic;
using System.Text;

namespace DTO
{
    public class MenuDTO
    {
        public int IdMenu { get; set; }
        public string? Descripcion { get; set; }
        public int IdMenuPadre { get; set; }
        public bool Estado { get; set; }
        public int Orden { get; set; }
        public string? Controller { get; set; }
        public int IdPerfil { get; set; }
        public bool Acceso { get; set; }
        public string? Action { get; set; }
        public IList<ItemMenu> Items { get; set; } = new List<ItemMenu>();
    }

    public class ItemMenu
    {
        public int IdMenu { get; set; }
        public string? Descripcion { get; set; }
        public int IdMenuPadre { get; set; }
        public bool Estado { get; set; }
        public int Orden { get; set; }
        public string? Controller { get; set; }
        public int IdPerfil { get; set; }
        public bool Acceso { get; set; }
        public string? Action { get; set; }
        public bool EsSubMenu { get; set; }
        public string? MenuPadre { get; set; }
        public IList<ItemMenu> ItemsSubMenu { get; set; } = new List<ItemMenu>();
    }
}
