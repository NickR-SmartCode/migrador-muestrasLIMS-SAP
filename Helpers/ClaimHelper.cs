using DTO;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;

namespace Helpers
{
    public class ClaimHelper
    {
        private readonly Encrypter _encrypter;
        public ClaimHelper(Encrypter encrypter)
        {
            _encrypter = encrypter;
        }

        public ClaimDTO ObtenerClaims(ClaimsIdentity identity)
        {
            ClaimDTO oClaimDTO = new ClaimDTO();
            oClaimDTO.BaseDatos = _encrypter.Decrypt(identity.FindFirst("HashedData").Value);
            oClaimDTO.IdPerfil = int.Parse(identity.FindFirst("IdPerfil").Value);
            oClaimDTO.IdUsuario = int.Parse(identity.FindFirst("IdUsuario").Value);
            return oClaimDTO;
        }
    }
}
