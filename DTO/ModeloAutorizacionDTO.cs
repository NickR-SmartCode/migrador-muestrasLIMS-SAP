using System;
using System.Collections.Generic;
using System.Text;

namespace DTO
{
    public class ModeloAutorizacionDTO
    {
        public int IdModeloAutorizacion { get; set; }
        public string NombreModelo { get; set; }
        public string DescripcionModelo { get; set; }
        public bool IncluirTodosUsuarios { get; set; }
        public bool Estado { get; set; }
        public List<ModeloAutorizacionAutorDTO> DetalleAutor { get; set; } = new List<ModeloAutorizacionAutorDTO>();
        public List<ModeloAutorizacionEtapaDTO> DetalleEtapa { get; set; } = new List<ModeloAutorizacionEtapaDTO>();
        public List<ModeloAutorizacionModuloDTO> DetalleModulo { get; set; } = new List<ModeloAutorizacionModuloDTO>();
        public List<ModeloAutorizacionCondicionDTO> DetalleCondicion { get; set; } = new List<ModeloAutorizacionCondicionDTO>();
    }

    public class ModeloAutorizacionAutorDTO
    {
        public int IdModeloAutorizacion { get; set; }
        public int IdAutor { get; set; }
    }

    public class ModeloAutorizacionCondicionDTO
    {
        public int IdModeloAutorizacion { get; set; }
        public string Condicion { get; set; }
    }

    public class ModeloAutorizacionModuloDTO
    {
        public int IdModeloAutorizacion { get; set; }
        public int IdModulo { get; set; }
    }

    public class ModeloAutorizacionEtapaDTO
    {
        public int IdModeloAutorizacion { get; set; }
        public int IdEtapa { get; set; }
    }

    public class ModuloDTO
    {
        public int Id { get; set; }
        public string Descripcion { get; set; }
        public bool DebeExistirModelo { get; set; }
    }
}
