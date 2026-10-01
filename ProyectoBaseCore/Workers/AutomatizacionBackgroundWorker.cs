using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using SERVICES;
using INTERFACES.Services;

namespace ProyectoBaseCore.BackgroundServices
{
    public class AutomatizacionBackgroundWorker : BackgroundService
    {
        private readonly AdminAutomatPFService _stateManager;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IConfiguration _configuration;

        public AutomatizacionBackgroundWorker(AdminAutomatPFService stateManager, IServiceScopeFactory scopeFactory,
                IConfiguration configuration)
        {
            _stateManager = stateManager;
            _scopeFactory = scopeFactory;
            _configuration = configuration;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            string baseDeDatosInicial = "FusionSAPLinkerFTS";

            try
            {
                await Task.Delay(TimeSpan.FromMinutes(2), stoppingToken);
            }
            catch (OperationCanceledException)
            {
                return;
            }

            if (!_stateManager.IsRunning(baseDeDatosInicial) && _configuration.GetValue<bool>("ServiceAutoStart"))
            {
                _stateManager.Start(baseDeDatosInicial);
                _stateManager.AddAction(baseDeDatosInicial, "🚀 Servicio iniciado automáticamente (2 min después de encender el servidor).");
            }

            while (!stoppingToken.IsCancellationRequested)
            {
                var activeDatabases = GetActiveDatabasesToProcess();


                var dbsARunear = activeDatabases.Where(db => _stateManager.IsRunning(db)).ToList();

          
                foreach (var bd in dbsARunear)
                {
                    using (var scope = _scopeFactory.CreateScope())
                    {
                        var pfService = scope.ServiceProvider.GetRequiredService<IEstadoMuestrasPreFTService>();
                        var userCts = _stateManager.GetCancellationToken(bd);

                        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken, userCts);

                        try
                        {
                            await pfService.EjecutarProcesamientoAutomaticoAsync(bd, 1, linkedCts.Token);

                            _stateManager.AddAction(bd, "💤 Ciclo completado. Esperando 5 minutos...");
                        }
                        catch (OperationCanceledException) { /* Stop manual */ }
                        catch (Exception ex)
                        {
                            _stateManager.AddAction(bd, $"❌ Error critico en worker: {ex.Message}");
                        }
                    }
                }

                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
        }


        private List<string> GetActiveDatabasesToProcess()
        {
            return new List<string>() { "FusionSAPLinkerFTS" };
        }
    }
}