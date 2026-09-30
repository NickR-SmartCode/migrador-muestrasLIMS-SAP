class PedidosService  {
    static async Listar(d, extraParams, options) {
        return await ApiService.dataTablesAdapter('/Pedidos/ListarPaginado', d, extraParams, options)
    }
    static async ObtenerDetalle(id) { return await ApiService.ajax(`/Pedidos/ObtenerDetalle?id=${id}`) }
};