using DTO;
using Helpers;
using INTERFACES.Repositories;
using INTERFACES.Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Transactions;

namespace SERVICES
{
    public class ConfiguracionService : IConfiguracionService
    {
        private readonly IConfiguracionRepository _configuracionRepository;
        private readonly Encrypter _encrypter;
        private readonly MailHelper _mailHelper;
        public ConfiguracionService(IConfiguracionRepository configuracionRepository, Encrypter encrypter, MailHelper mailHelper)
        {
            _configuracionRepository = configuracionRepository;
            _encrypter = encrypter;
            _mailHelper = mailHelper;
        }
        public async Task<ConfiguracionDTO> ObtenerConfiguracion(string BaseDatos)
        {
            ConfiguracionDTO configuracionDTO = await _configuracionRepository.ObtenerConfiguracion(BaseDatos);
            configuracionDTO.PasswordSMTP = _encrypter.Decrypt(configuracionDTO.PasswordSMTP);
            return configuracionDTO;
        }

        public async Task<int> UpdateConfiguracion(ConfiguracionDTO oConfiguracionDTO, int IdUsuario, string baseDatos)
        {
            var transactionOptions = new TransactionOptions
            {
                IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted,
                Timeout = TimeSpan.FromSeconds(60)
            };

            using var scope = new TransactionScope(
                TransactionScopeOption.Required,
                transactionOptions,
                TransactionScopeAsyncFlowOption.Enabled);

            oConfiguracionDTO.PasswordSMTP = _encrypter.Encrypt(oConfiguracionDTO.PasswordSMTP);
            int resultado = await _configuracionRepository.UpdateConfiguracion(oConfiguracionDTO, IdUsuario, baseDatos);

            scope.Complete();

            return resultado;
        }

        public async Task<bool> EnviarEmailPrueba(string BaseDatos)
        {
            ConfiguracionDTO configuracionDTO = await _configuracionRepository.ObtenerConfiguracion(BaseDatos);
            configuracionDTO.PasswordSMTP = _encrypter.Decrypt(configuracionDTO.PasswordSMTP);

            List<string> Destinatarios = new List<string>();
            Destinatarios.Add("cristhian.chacaliaza@smartcode.pe");
            bool CorreoEnviado = await _mailHelper.EnviarEmail(configuracionDTO, "EMAIL DE PRUEBA", Destinatarios, "Email de Prueba");           
            return CorreoEnviado;
        }
    }
}
