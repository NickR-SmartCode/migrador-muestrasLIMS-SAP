using DTO;
using Helpers;
using INTERFACES.Repositories;
using INTERFACES.Services;
using System.Transactions;

namespace SERVICES
{
    public class AprobacionesService : IAprobacionesService
    {
        private readonly IAprobacionesRepository _aprobacionesRepository;
        private readonly IConfiguracionRepository _configuracionRepository;
        private readonly IChequeTestRepository _chequeTestRepository;
        private readonly Encrypter _encrypter;
        public AprobacionesService(IAprobacionesRepository aprobacionesRepository, IConfiguracionRepository configuracionRepository, Encrypter encrypter, IChequeTestRepository chequeTestRepository)
        {
            _aprobacionesRepository = aprobacionesRepository;
            _configuracionRepository = configuracionRepository;
            _encrypter = encrypter;
            _chequeTestRepository = chequeTestRepository;
        }

        public async Task<bool> RegistrarAprobacionDocumento(int IdTabla,int IdModulo, int IdAutor, string baseDatos)
        {
            ModuloDTO oModuloDTO = await ObtenerModuloValido(IdModulo, baseDatos);

            List<AprobacionesDTO> lstAprobacionesDAO = await _aprobacionesRepository.ObtenerModelosPorAutorDocumento(IdModulo, IdAutor, baseDatos);

            List<int> EtapasRegistrar = await DeterminarEtapasARegistrar(lstAprobacionesDAO, oModuloDTO, IdTabla, baseDatos);

            await ProcesarRegistroAprobaciones(EtapasRegistrar, IdModulo, IdTabla,  baseDatos);
            return true;
        }

        private async Task<ModuloDTO> ObtenerModuloValido(int IdModulo, string baseDatos)
        {
            ModuloDTO oModuloDTO = await _aprobacionesRepository.ObtenerDatosModulo(IdModulo, baseDatos);

            if (String.IsNullOrEmpty(oModuloDTO.Descripcion))
            {
                throw new Exception("No se encontró el Modulo con Id " + IdModulo);
            }
            return oModuloDTO;
        }

        private async Task<List<int>> DeterminarEtapasARegistrar(List<AprobacionesDTO> lstAprobacionesDAO,ModuloDTO oModuloDTO,int IdTabla,string baseDatos)
        {
            List<int> EtapasRegistrar = new List<int>();

            var diccionarioPorModelo = lstAprobacionesDAO
             .Select((a, index) => new { Item = a, Index = index })
             .GroupBy(x => x.Item.IdModelo)
             .ToDictionary(
                 g => g.Key,
                 g => g.OrderBy(x => x.Index).Select(x => x.Item).ToList()
             );

            foreach (var kvp in diccionarioPorModelo)
            {
                List<AprobacionesDTO> ListaPorModelo = kvp.Value;

                bool IngresarAprobacion = true;

                foreach (var aprobacion in kvp.Value)
                {
                    int Valido = await _aprobacionesRepository.ValidarCondicion(IdTabla, aprobacion.Condicion, baseDatos);
                    if (Valido == 0)
                    {
                        IngresarAprobacion = false;
                        break;
                    }
                }

                if (IngresarAprobacion)
                {
                    var etapas = ListaPorModelo
                     .Select(a => a.IdEtapa)
                     .Distinct()
                     .ToList();

                    EtapasRegistrar.AddRange(etapas);
                    EtapasRegistrar = EtapasRegistrar.Distinct().ToList();

                    //EtapasRegistrar = etapas
                    //.Union(etapas)
                    //.ToList();
                }
            }

            if (EtapasRegistrar.Count == 0 && oModuloDTO.DebeExistirModelo)
            {
                throw new Exception("No se encontró un Modelo de Aprobaciones Válido");
            }

            return EtapasRegistrar;
        }

        private async Task ProcesarRegistroAprobaciones(List<int> EtapasRegistrar,int IdModulo,int IdTabla,string baseDatos)
        {
            if (EtapasRegistrar.Count == 0)
            {
               int resultado = await _aprobacionesRepository.UpdateModuloAprobarDirecto(IdModulo, IdTabla, baseDatos);
                if (resultado <= 0)
                    throw new Exception("No se pudo aprobar el módulo.");
            }
            else
            {
                for (int i = 0; i < EtapasRegistrar.Count; i++)
                {
                    int IdDocumentoAprobacionEtapa = await _aprobacionesRepository.UpdateInsertDocumentoAprobacionEtapas(EtapasRegistrar[i], IdModulo, IdTabla, baseDatos);
                    int resultado = await _aprobacionesRepository.UpdateInsertModuloAprobacionModelo(EtapasRegistrar[i], IdDocumentoAprobacionEtapa, baseDatos);
                    if (resultado <= 0)
                        throw new Exception("No se pudo aprobar el módulo.");
                }

            }
        }

        public async Task<bool> EnviarCorreoAprobadores(int IdModulo, int IdTablaOriginal, string baseUrl, string BaseDatos)
        {
            try
            {
                ConfiguracionDTO oConfiguracionDTO = await _configuracionRepository.ObtenerConfiguracion(BaseDatos);
                oConfiguracionDTO.PasswordSMTP = _encrypter.Decrypt(oConfiguracionDTO.PasswordSMTP);
                List<TurnoAprobarDTO> lstTurnoAprobarDTO = await _aprobacionesRepository.ObtenerTurnoAprobadores(IdModulo, IdTablaOriginal, BaseDatos);

                for (int i = 0; i < lstTurnoAprobarDTO.Count; i++)
                {
                    if (!lstTurnoAprobarDTO[i].correoEnviado)
                    {

                        string IdEncrypted = _encrypter.Encrypt(lstTurnoAprobarDTO[i].IdModuloAprobacionModelo.ToString());
                        string BDEncrypted = _encrypter.Encrypt(BaseDatos.ToString());
                        string AccionAprobarEncrypted = _encrypter.Encrypt("1");
                        string AccionRechazarEncrypted = _encrypter.Encrypt("2");

                        baseUrl += "/Aprobacion/AprobarDesdeEmail?Param1=" + IdEncrypted + "&Param2=" + BDEncrypted + "&Param3=";

                        string Body = PlantillaAprobacion();
                        Body = Body.Replace("{{UrlAprobar}}", baseUrl + AccionAprobarEncrypted);
                        Body = Body.Replace("{{UrlRechazar}}", baseUrl + AccionRechazarEncrypted);

                        string BodyModulo = "";
                        string EstilosModulo = EstilosGeneral();
                        if (IdModulo == 1)
                        {
                            BodyModulo = await PlantillaCheque(IdTablaOriginal, BaseDatos);
                            EstilosModulo += EstilosCheque();
                        }
                        Body = Body.Replace("{{DatosAprobar}}", BodyModulo);


                        MailHelper oMailHelper = new MailHelper();
                        List<string> correos = new List<string>();
                        correos.Add(lstTurnoAprobarDTO[i].Email);

                        oMailHelper.AgregarEstilosAdicionales(EstilosModulo);
                        bool CorreoEnviado = await oMailHelper.EnviarEmail(oConfiguracionDTO, "APROBACION DE DOCUMENTOS", correos, Body);

                        if (CorreoEnviado)
                        {
                            await _aprobacionesRepository.MarcarCorreoAprobadorEnviado(lstTurnoAprobarDTO[i].IdModuloAprobacionModelo, 1, BaseDatos);
                        }

                    }
                }
                return true;

            }
            catch
            {
                throw;
            }

        }
        private string PlantillaAprobacion()
        {
            return @" 
            {{DatosAprobar}}
            <!-- Acciones -->
            <div class=""actions"">
                <h3>¿Desea autorizar estos documentos?</h3>
                <p style=""margin-bottom: 25px; color: #6c757d;"">
                    Haga clic en una de las siguientes opciones para procesar la solicitud:
                </p>
                
                <a href=""{{UrlAprobar}}"" class=""btn btn-approve"">
                    ✅ Aprobar
                </a>
                
                <a href=""{{UrlRechazar}}"" class=""btn btn-reject"">
                    ❌ Rechazar
                </a>
            </div>";
        }

        private string EstilosGeneral()
        {
            return @" .actions {
            text-align: center;
            margin-top: 40px;
            padding: 30px;
            background-color: #f8f9fa;
            border-radius: 8px;
            }
            .actions h3 {
                margin-bottom: 20px;
                color: #495057;
                font-size: 18px;
            }
            .btn {
                display: inline-block;
                padding: 12px 30px;
                margin: 0 10px;
                text-decoration: none;
                border-radius: 6px;
                font-weight: bold;
                font-size: 14px;
                text-transform: uppercase;
                letter-spacing: 0.5px;
                transition: all 0.3s ease;
                cursor: pointer;
                border: none;
            }
            .btn-approve {
                background-color: #28a745;
                color: white;
            }
            .btn-approve:hover {
                background-color: #218838;
                transform: translateY(-2px);
                box-shadow: 0 4px 8px rgba(40, 167, 69, 0.3);
            }
            .btn-reject {
                background-color: #dc3545;
                color: white;
            }
            .btn-reject:hover {
                background-color: #c82333;
                transform: translateY(-2px);
                box-shadow: 0 4px 8px rgba(220, 53, 69, 0.3);
            }
            @media(max - width: 600px) {
                .actions.btn {
                    display: block;
                    margin: 10px 0;
                    width: 100 %;
                    }
                .info - grid {
                    grid - template - columns: 1fr;
                }
            }";
        }

        public async Task<string> PlantillaCheque(int IdTablaOriginal, string BaseDatos)
        {
            //aqui deberia traer por id, pero como es concepto no hice un metodo para traer por id asi que lo busco con linq;
            List<ChequeTestDTO> lstChequeTestDTO = await _chequeTestRepository.ObtenerCheques(BaseDatos);

            ChequeTestDTO oChequeTestDTO = lstChequeTestDTO.FirstOrDefault(o => o.IdCheque == IdTablaOriginal);

            return @"
            <div class=""section"">
                <h2>📋 Información General</h2>
                <div class=""info-grid"">               
                    <div class=""info-item"">
                        <div>
                            <label>Nombre:</label>
                            <span>" + oChequeTestDTO.Nombre + @"</span>
                        </div>
                    </div>
                    <br/>
                      <div class=""info-item"">
                        <div>
                            <label>Fecha:</label>
                            <span>" + oChequeTestDTO.Fecha.ToString("dd/MM/yyyy") + @"</span>
                        </div>
                    </div>
                    <br/>
                     <div class=""info-item"">
                        <div>
                            <label>Monto:</label>
                            <span>" + oChequeTestDTO.Moneda + ": " + oChequeTestDTO.Monto.ToString("N2") + @"</span>
                        </div>
                    </div>
                </div>
            </div>     
            ";


        }

        private string EstilosCheque()
        {
            return @" 
            .section {
                margin-bottom: 30px;
            }
            .section h2 {
                color: #667eea;
                font-size: 18px;
                margin-bottom: 15px;
                border-bottom: 2px solid #e9ecef;
                padding-bottom: 8px;
            }
            .info-grid {
                display: grid;
                grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
                gap: 15px;
                margin-bottom: 20px;
            }
            .info-item {
                background-color: #f8f9fa;
                padding: 12px;
                border-radius: 6px;
                border-left: 4px solid #667eea;
            }
            .info-item label {
                font-weight: bold;
                color: #495057;
                display: block;
                margin-bottom: 4px;
                font-size: 12px;
                text-transform: uppercase;
                letter-spacing: 0.5px;
            }
            .info-item span {
                color: #212529;
                font-size: 14px;
            }";
        }

    }
}
