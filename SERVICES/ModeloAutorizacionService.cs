using DTO;
using INTERFACES.Repositories;
using INTERFACES.Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Transactions;

namespace SERVICES
{
    public class ModeloAutorizacionService : IModeloAutorizacionService
    {
        private readonly IModeloAutorizacionRepository _modeloAutorizacionRepository;
        public ModeloAutorizacionService(IModeloAutorizacionRepository modeloAutorizacionRepository)
        {
            _modeloAutorizacionRepository = modeloAutorizacionRepository;
        }

        public async Task<List<ModuloDTO>> ObtenerModulos(string Base) =>await _modeloAutorizacionRepository.ObtenerModulos(Base);
        public async Task<List<ModeloAutorizacionDTO>> ListarModeloAutorizacion(bool MostrarInactivos, string Base) => await _modeloAutorizacionRepository.ListarModeloAutorizacion(MostrarInactivos, Base);

        public async Task<int> UpdateInsertModeloAutorizacion(ModeloAutorizacionDTO oModeloAutorizacionDTO, int IdUsuario, string baseDatos)
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

            int resultado = await _modeloAutorizacionRepository.UpdateInsertModeloAutorizacion(oModeloAutorizacionDTO, IdUsuario, baseDatos);
            oModeloAutorizacionDTO.IdModeloAutorizacion = resultado;
            for (int i = 0; i < oModeloAutorizacionDTO.DetalleAutor.Count; i++)
            {
                oModeloAutorizacionDTO.DetalleAutor[i].IdModeloAutorizacion = resultado;
                int resultadoDetalle = await _modeloAutorizacionRepository.InsertModeloAutorizacionAutor(oModeloAutorizacionDTO.DetalleAutor[i], baseDatos);
            }
            for (int i = 0; i < oModeloAutorizacionDTO.DetalleEtapa.Count; i++)
            {
                oModeloAutorizacionDTO.DetalleEtapa[i].IdModeloAutorizacion = resultado;
                int resultadoDetalle = await _modeloAutorizacionRepository.InsertModeloAutorizacionEtapa(oModeloAutorizacionDTO.DetalleEtapa[i], baseDatos);
            }
            for (int i = 0; i < oModeloAutorizacionDTO.DetalleModulo.Count; i++)
            {
                oModeloAutorizacionDTO.DetalleModulo[i].IdModeloAutorizacion = resultado;
                int resultadoDetalle = await _modeloAutorizacionRepository.InsertModeloAutorizacionModulo(oModeloAutorizacionDTO.DetalleModulo[i], baseDatos);
            }
            for (int i = 0; i < oModeloAutorizacionDTO.DetalleCondicion.Count; i++)
            {
                oModeloAutorizacionDTO.DetalleCondicion[i].IdModeloAutorizacion = resultado;
                int resultadoDetalle = await _modeloAutorizacionRepository.InsertModeloAutorizacionCondicion(oModeloAutorizacionDTO.DetalleCondicion[i], baseDatos);
            }


            scope.Complete();

            return resultado;
        }

        public async Task<ModeloAutorizacionDTO> ObtenerModeloAutorizacion(int IdModeloAutorizacion, string Base)
        {
            ModeloAutorizacionDTO oModeloAutorizacionDTO = await _modeloAutorizacionRepository.ObtenerModeloAutorizacion(IdModeloAutorizacion, Base);
            oModeloAutorizacionDTO.DetalleAutor = await _modeloAutorizacionRepository.ObtenerAutorModeloAutorizacion(IdModeloAutorizacion, Base);
            oModeloAutorizacionDTO.DetalleModulo = await _modeloAutorizacionRepository.ObtenerModuloModeloAutorizacion(IdModeloAutorizacion, Base);
            oModeloAutorizacionDTO.DetalleEtapa = await _modeloAutorizacionRepository.ObtenerEtapaModeloAutorizacion(IdModeloAutorizacion, Base);
            oModeloAutorizacionDTO.DetalleCondicion = await _modeloAutorizacionRepository.ObtenerCondicionModeloAutorizacion(IdModeloAutorizacion, Base);
            return oModeloAutorizacionDTO;
        }
    }
}
