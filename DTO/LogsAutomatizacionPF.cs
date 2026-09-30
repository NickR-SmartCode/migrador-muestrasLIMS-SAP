
namespace DTO
{
    public class LogsAutomatizacionPF
    {
        public int Id { get; set; }
        public DateTime Fecha_Ejecucion { get; set; }
        public long? Project { get; set; }
        public long? Customer { get; set; }
        public string CardCodeSAP { get; set; } = string.Empty;
        public string CardName { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public string Mensaje { get; set; } = string.Empty;
        public int? DocNumSAP { get; set; }
        public int? SeriesSAP { get; set; }
        public string NroPF { get; set; } = string.Empty;
        public string Detalle { get; set; } = string.Empty;
    }
}
