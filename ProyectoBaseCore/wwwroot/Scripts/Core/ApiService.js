let searchAbortController = null;
class ApiService {
    static #getToken() {
        const token = localStorage.getItem("token");
        return token || "";
    }

    /**
     * Motor base para peticiones Fetch
     * @param {string} url - Ruta relativa (ej: '/Clientes/Listar')
     * @param {string} method - HTTP Verb (GET, POST, etc.)
     * @param {object} body - Objeto JS que se enviar� como JSON
     */
    static async ajax(url, method = 'GET', body = null, options = {}) {
        const innerOptions = {
            mostrarResumenErrores: options.mostrarResumenErrores ?? null,
            omitirErrHandling: options.omitirErrHandling ?? null,
            ...options
        }
        console.log(innerOptions);
        if (innerOptions.cancelPrevious && searchAbortController) {
            searchAbortController.abort();
        }

        searchAbortController = new AbortController();
        const { signal } = searchAbortController;


        const baseUrl = window.AppConfig.BaseURL || '';
        const fullUrl = `${baseUrl}${url}`.replace(/\/+/g, '/');

        const config = {
            method: method,
            signal,
            headers: {
                'Content-Type': 'application/json',
                'X-Requested-With': 'XMLHttpRequest'
            },
            credentials: 'include'
        };


        if (body && (method === 'POST' || method === 'PUT')) {
            config.body = JSON.stringify(body);
        }

        try {
            const response = await fetch(fullUrl, config);

            if (response.status === 401) {
                localStorage.removeItem("token");
                window.location.href = `${baseUrl}/? m = expired`;
                return null;
            }

            const result = await response.json();

            if (innerOptions.omitirErrHandling) return result;

            if (result && !result.Exito) {
                if (result.Error.Tipo === "INTERNAL SERVER ERROR") {
                    Swal.fire({
                        "title": "Error",
                        "icon": "error",
                        "html": `

                 <div> Error crítico en el servidor, por favor, dirígase al
                    <a href="https://smartcodetechnologyhubsac.freshdesk.com/support/tickets/new"
                    style="color: #2F4864;"
                    target="_blank">
                    soporte
                    </a>

                </div>
				`
                    });
                    throw new Error("ERROR CRITICO EN EL SERVIDOR");
                }
                console.warn("Advertencia de Negocio:", result.Error.Msg);

                if (window.Swal) {
                    Swal.fire("Atención", result.Error.Msg, "warning");
                }
                else {
                    alert(result.Error.Msg);
                }

                if (!result.Exito && result.Error.ErroresCampos) {

                    UI.showServerErrors(result.Error.ErroresCampos);
                    if (innerOptions.mostrarResumenErrores === true) {
                        Swal.fire({
                            title: "Errores de Validación",
                            html: `<div style="text-align:left">${resumenErrores.join("<br>")}</div>`,
                            icon: "error"
                        });
                    }
                }

            }

            return result;
        }
        catch (error) {
            if (error.name === "AbortError") return;
            Swal.fire({
                "title": "Error",
                "icon": "error",
                "html": `

                 <div> Error crítico en el servidor, por favor, dirígase al
                    <a href="https://smartcodetechnologyhubsac.freshdesk.com/support/tickets/new"
                    style="color: #2F4864;"
                    target="_blank">
                    soporte
                    </a>

                </div>
				`
            });
            throw error;
        }
    }

    /**
     * Adaptador para jQuery DataTables (Server-side)
     * @param {string} url - Endpoint del listado paginado
     * @param {object} dtParams 
     * @param {object} extraParams 
     */
    static async dataTablesAdapter(url, dtParams, extraParams = null, options = {}) {

        const peticion = {
            Pagina: (dtParams.start / dtParams.length) + 1,
            Cantidad: dtParams.length,
            Busqueda: dtParams.search.value,
            ...extraParams
        };


        const res = await this.ajax(url, 'POST', peticion, options);

        if (res && res.Exito) {
            return {
                draw: dtParams.draw,
                recordsTotal: res.Datos.TotalSinFiltrar,
                recordsFiltered: res.Datos.TotalFiltrado,
                data: res.Datos.Data
            }
                ;
        }
        else {
            return {
                draw: dtParams.draw,
                recordsTotal: 0,
                recordsFiltered: 0,
                data: []
            }
                ;
        }
    }
}

const Toast = Swal.mixin({
    toast: true,
    position: 'top-end',
    showConfirmButton: false,
    timer: 2000,
    timerProgressBar: true
});
window.Toast = Toast;