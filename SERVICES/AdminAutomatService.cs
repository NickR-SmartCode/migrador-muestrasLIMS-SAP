using DTO;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace SERVICES
{
    public class AdminAutomatizadorPedidoService
    {
        private readonly ConcurrentDictionary<string, CancellationTokenSource?> _activeProcesses = new();
        private readonly ConcurrentDictionary<string, AutomatizacionPedidoProgDTO> _progress = new();

        public bool IsRunning(string bd) => _activeProcesses.TryGetValue(bd, out var cts) && cts != null;

        public bool Start(string bd, Func<CancellationToken, Task> action)
        {
            if (IsRunning(bd)) return false;

            var cts = new CancellationTokenSource();
            _activeProcesses[bd] = cts;

            Task.Run(async () =>
            {
                try
                {
                    await action(cts.Token);
                }
                finally
                {
                    _activeProcesses.TryUpdate(bd, null, cts);
                    if (_progress.TryGetValue(bd, out var p)) p.IsRunning = false;
                }
            }, cts.Token);

            return true;
        }

        public void Stop(string bd)
        {
            if (_activeProcesses.TryRemove(bd, out var cts))
            {
                cts?.Cancel();
            }
        }

        public void AddAction(string bd, string mensaje)
        {
            var p = GetProgress(bd);
            if (p.UltimasAcciones == null) p.UltimasAcciones = new List<AccionConsolaDTO>();

            p.UltimasAcciones.Add(new AccionConsolaDTO
            {
                Mensaje = mensaje,
                Fecha = DateTime.Now
            });

            if (p.UltimasAcciones.Count > 15)
            {
                p.UltimasAcciones.RemoveAt(0);
            }
        }

        public AutomatizacionPedidoProgDTO GetProgress(string bd)
            => _progress.GetOrAdd(bd, new AutomatizacionPedidoProgDTO { IsRunning = false });

        public void UpdateProgress(string bd, Action<AutomatizacionPedidoProgDTO> updateAction)
        {
            var p = GetProgress(bd);
            updateAction(p);
        }

        public void TogglePause(string bd)
        {
            var p = GetProgress(bd);
            p.IsPaused = !p.IsPaused;
            AddAction(bd, p.IsPaused ? "⏸️ Proceso PAUSADO por el usuario." : "▶️ Proceso REANUDADO.");
        }

        public async Task WaitIfPaused(string bd, CancellationToken ct)
        {
            var p = GetProgress(bd);
            while (p.IsPaused && !ct.IsCancellationRequested)
            {
                await Task.Delay(1000, ct);
            }
        }
    }
}
