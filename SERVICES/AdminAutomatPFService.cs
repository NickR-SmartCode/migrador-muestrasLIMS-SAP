using DTO;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace SERVICES
{
    public class AdminAutomatPFService
    {
        private readonly ConcurrentDictionary<string, CancellationTokenSource?> _activeProcesses = new();
        private readonly ConcurrentDictionary<string, AutomatizacionPedidoProgDTO> _progress = new();

        private readonly ConcurrentDictionary<string, DateTime> _errorThrottle = new();

        public bool DeboNotificarError(string bd, string nroPF, string errorMsg)
        {
            // Creamos una llave única por Prefactura y Mensaje de Error
            // Usamos el HashCode del mensaje para que no sea una llave gigantesca
            string key = $"{bd}_{nroPF}_{errorMsg.GetHashCode()}";

            if (_errorThrottle.TryGetValue(key, out DateTime ultimaVez))
            {
                // Si han pasado menos de 45 minutos, no notificamos
                if ((DateTime.Now - ultimaVez).TotalMinutes < 45)
                {
                    return false;
                }
            }

            // Si es nuevo o ya pasó el tiempo, actualizamos la marca de tiempo
            _errorThrottle[key] = DateTime.Now;

            // Limpieza opcional: Eliminar llaves muy viejas para no llenar la RAM
            if (_errorThrottle.Count > 1000) { _errorThrottle.Clear(); }

            return true;
        }

        public bool IsRunning(string bd) => _activeProcesses.TryGetValue(bd, out var cts) && cts != null;

        public bool Start(string bd)
        {
            if (IsRunning(bd)) return false;

            var cts = new CancellationTokenSource();
            _activeProcesses[bd] = cts;

            UpdateProgress(bd, p =>
            {
                p.IsRunning = true;
                p.IsPaused = false;
                p.Procesadas = 0; // Resetear contador al iniciar
            });

            return true;
        }
        public void Stop(string bd)
        {
            if (_activeProcesses.TryRemove(bd, out var cts))
            {
                cts?.Cancel();
                cts?.Dispose();
            }
            UpdateProgress(bd, p => p.IsRunning = false);
        }

        public CancellationToken GetCancellationToken(string bd)
        {
            return _activeProcesses.TryGetValue(bd, out var cts) ? cts.Token : CancellationToken.None;
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

            if (p.UltimasAcciones.Count > 20)
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

        public bool RequestStart(string bd)
        {
            if (IsRunning(bd)) return false;

            var cts = new CancellationTokenSource();
            _activeProcesses[bd] = cts;

            UpdateProgress(bd, p =>
            {
                p.IsRunning = true;
                p.IsPaused = false;
            });

            return true;
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
