using DTO;
using INTERFACES.Repositories;
using INTERFACES.Services;
using System.Transactions;

namespace SERVICES
{
    public class PerfilService : IPerfilService
    {
        private readonly IPerfilRepository _perfilRepository;

        public PerfilService(IPerfilRepository perfilRepository)
        {
            _perfilRepository = perfilRepository;
        }

        public async Task<List<PerfilDTO>> ObtenerPerfiles(bool MostrarInactivos, string Base) => await _perfilRepository.ObtenerPerfiles(MostrarInactivos, Base);
        public async Task<PerfilDTO> ObtenerDatosxID(int IdPerfil, string Base) => await _perfilRepository.ObtenerDatosxID(IdPerfil, Base);

        public async Task<int> UpdateInsertPerfil(PerfilDTO oPerfilDTO, int idUsuario, string baseDatos)
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

            int resultado = await _perfilRepository.UpdateInsertPerfil(oPerfilDTO, idUsuario, baseDatos);

            scope.Complete();

            return resultado;
        }
    }
}
