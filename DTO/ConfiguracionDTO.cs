using System;
using System.Collections.Generic;
using System.Text;

namespace DTO
{
    public class ConfiguracionDTO
    {
        public string RUC { get; set; }
        public string RazonSocial { get; set; }
        public string ServidorSMTP { get; set; }
        public int PuertoSMTP { get; set; }
        public bool SSLSMTP { get; set; }
        public string EmailSMTP { get; set; }
        public string PasswordSMTP { get; set; }
    }
}
