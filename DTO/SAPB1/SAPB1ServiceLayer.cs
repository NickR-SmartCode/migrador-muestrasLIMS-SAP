using System;
using System.Collections.Generic;
using System.Text;

namespace DTO.SAPB1ServiceLayer
{
    public class ServiceLayerSettings
    {
        public string BaseUrl { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string CompanyDB { get; set; } = string.Empty;
    }

    public class ServiceLayerSession
    {
        public string SessionId { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
        public int SessionTimeout { get; set; }
    }
}
