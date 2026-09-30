class EstadoMuestrasPreFTService  {
     static async Listar (dtParams, extraParams, options)  {

        return await ApiService.dataTablesAdapter('/EstadoPrefacturas/ListarPaginado', dtParams, extraParams, options);
    }

    static async  ProcesarMuestrasASap (data) {
        return await ApiService.ajax('/EstadoPrefacturas/ProcesarMuestrasASap', 'POST', data);
    }
}