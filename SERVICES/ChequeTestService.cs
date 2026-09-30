using DTO;
using INTERFACES.Repositories;
using INTERFACES.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Text;
using System.Transactions;

namespace SERVICES
{
    public class ChequeTestService : IChequeTestService
    {
        private readonly IChequeTestRepository _chequeTestRepository;
        private readonly IAprobacionesService _aprobacionesService;
        private readonly BackgroundTaskQueue _backgroundQueue;
        private readonly IServiceScopeFactory _scopeFactory;
        public ChequeTestService(IChequeTestRepository chequeTestRepository,IAprobacionesService aprobacionesService, BackgroundTaskQueue backgroundTaskQueue, IServiceScopeFactory serviceScopeFactory) { 
            _chequeTestRepository = chequeTestRepository;
            _aprobacionesService = aprobacionesService;
            _backgroundQueue = backgroundTaskQueue;
            _scopeFactory = serviceScopeFactory;
        }
        public async Task<int> UpdateInsertCheque(ChequeTestDTO oChequeTestDTO, int IdUsuario,string BaseUrl, string baseDatos)
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

            int IdCheque = await _chequeTestRepository.UpdateInsertCheque(oChequeTestDTO, IdUsuario, baseDatos);
            bool AprobacionRegistrada = await _aprobacionesService.RegistrarAprobacionDocumento(IdCheque, 1, IdUsuario, baseDatos);
            scope.Complete();
            
            if (IdCheque > 0)
            {
                _backgroundQueue.QueueBackgroundWorkItem(async token =>
                {
                    using var scope = _scopeFactory.CreateScope();

                    var aprobacionesService = scope.ServiceProvider
                        .GetRequiredService<IAprobacionesService>();

                    await aprobacionesService
                        .EnviarCorreoAprobadores(1, IdCheque, BaseUrl, baseDatos);
                });
            }
                
    

            return IdCheque;
        }
        public async Task<List<ChequeTestDTO>> ObtenerCheques(string baseDatos) => await _chequeTestRepository.ObtenerCheques(baseDatos);
        public async Task<List<ChequeTestDTO>> ObtenerChequesParaAprobacion(int IdUsuario, string baseDatos) => await _chequeTestRepository.ObtenerChequesParaAprobacion(IdUsuario,baseDatos);
    }
}
