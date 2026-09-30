using System;
using System.Collections.Generic;
using System.Text;

namespace DTO
{
    internal class AuthDTO
    {
    }
    public class LoginRequestDTO
    {
        public string Usuario { get; set; }
        public string Password { get; set; }
        public bool Mantener { get; set; }
    }
}
