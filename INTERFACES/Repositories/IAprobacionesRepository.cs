using DTO;
using System;
using System.Collections.Generic;
using System.Text;

namespace INTERFACES.Repositories
{
    public interface IAprobacionesRepository
    {
        Task<ModuloDTO> ObtenerDatosModulo(int IdModulo, string baseDatos);
        Task<List<AprobacionesDTO>> ObtenerModelosPorAutorDocumento(int IdModulo, int IdAutor, string baseDatos);
        Task<int> ValidarCondicion(int Id, string Condicion, string baseDatos);
        Task<int> UpdateModuloAprobarDirecto(int IdModulo, int IdTablaOriginal, string baseDatos);
        Task<int> UpdateInsertDocumentoAprobacionEtapas(int IdEtapa, int IdModulo, int IdTablaOriginal, string baseDatos);
        Task<int> UpdateInsertModuloAprobacionModelo(int IdEtapa, int IdDocumentoAprobacionEtapa, string baseDatos);
        Task<List<TurnoAprobarDTO>> ObtenerTurnoAprobadores(int IdModulo, int IdTablaOriginal, string baseDatos);
        Task<int> MarcarCorreoAprobadorEnviado(int IdModuloAprobacionModelo, int Enviado, string baseDatos);
    }
}
