using DTO;
using INTERFACES.Repositories;
using INTERFACES.Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Transactions;

namespace SERVICES
{
    public class EtapaAutorizacionService : IEtapaAutorizacionService
    {
        private readonly IEtapaAutorizacionRepository _etapaAutorizacionRepository;

        public EtapaAutorizacionService(IEtapaAutorizacionRepository etapaAutorizacionRepository)
        {
            _etapaAutorizacionRepository = etapaAutorizacionRepository;
        }

        public async Task<List<EtapaAutorizacionDTO>> ListarEtapaAutorizacion(bool MostrarInactivos, string Base) =>await _etapaAutorizacionRepository.ListarEtapaAutorizacion(MostrarInactivos, Base);

        public async Task<int> UpdateInsertEtapaAutorizacion(EtapaAutorizacionDTO oEtapaAutorizacionDTO, int IdUsuario, string baseDatos)
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

            int resultado = await _etapaAutorizacionRepository.UpdateInsertEtapaAutorizacion(oEtapaAutorizacionDTO, IdUsuario, baseDatos);
            oEtapaAutorizacionDTO.IdEtapaAutorizacion = resultado;
            for (int i = 0; i < oEtapaAutorizacionDTO.Detalle.Count; i++)
            {
                oEtapaAutorizacionDTO.Detalle[i].IdEtapaAutorizacion = resultado;
                int resultadoDetalle = await _etapaAutorizacionRepository.UpdateInsertEtapaAutorizacionDetalle(oEtapaAutorizacionDTO.Detalle[i], baseDatos);
            }

            scope.Complete();

            return resultado;
        }
        public async Task<EtapaAutorizacionDTO> ObtenerEtapaAutorizacion(int IdEtapaAutorizacion, string Base)
        {
            EtapaAutorizacionDTO oEtapaAutorizacionDTO = await _etapaAutorizacionRepository.ObtenerEtapaAutorizacion(IdEtapaAutorizacion, Base);
            oEtapaAutorizacionDTO.Detalle = await _etapaAutorizacionRepository.ObtenerDetallaEtapaAutorizacion(IdEtapaAutorizacion, Base);
            return oEtapaAutorizacionDTO;
        }
    }
}
