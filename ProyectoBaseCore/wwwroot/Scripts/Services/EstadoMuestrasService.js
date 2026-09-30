class EstadoMuestrasService  {
     static async Listar (dtParams, extraParams, options)  {

        return await ApiService.dataTablesAdapter('/EstadoMuestras/ListarPaginado', dtParams, extraParams, options);
    }

    static async  ProcesarMuestrasASap (data) {
        return await ApiService.ajax('/EstadoMuestras/ProcesarMuestrasASap', 'POST', data);
    }
}