using System;
using System.Collections.Generic;
using System.Text;

namespace DTO
{
    public class DatabaseSettings
    {
        public string Server { get; set; }
        public string User { get; set; }
        public string Password { get; set; }
    }
    public class LIMSDatabaseSettings
    {
        public string Server { get; set; }
        public string User { get; set; }
        public string Password { get; set; }
        public string Database { get; set; }
    }
    public class HANADatabaseSettings
    {
        public string Server { get; set; }
        public string User { get; set; }
        public string Password { get; set; }
        public string Schema { get; set; }
    }
}
