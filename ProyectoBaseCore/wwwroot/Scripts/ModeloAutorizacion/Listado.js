
let table = '';
let contador = 0;
let contador1 = 0;
let Usuarios;
let Etapas;

window.onload = function () {
    ListarModulos()
    CargarUsuarios()
    CargarEtapas()
    ConsultaServidor();

    $(document).on('click', '.borrar', function (event) {
        event.preventDefault();
        $(this).closest('tr').remove();
    });

    TriggerTooltipB5()

};

function ModalNuevo() {
    $("#lblTituloModal").html("Nuevo Modelo de Autorizacion");
    AbrirModal("modal-form");
    $('#chkActivo').prop('checked', true);
    AgregarLinea(0);
    AgregarLineaEtapas(0);  
    AgregarLineaCondiciones('');
}

function CerrarModal() {
    $("#tabla").find('tbody').empty();
    $("#tablaEtapas").find('tbody').empty();
    $("#tablaDocumentos").find('tbody').empty();
    $("#tablaCondiciones").find('tbody').empty();
    $("#modal-form").modal('hide')

    limpiarDatos();
}
function limpiarDatos() {
    $("#txtId").val("");
    $("#txtNombreModelo").val("");
    $("#txtDescripcionModelo").val("");
    $("#chkActivo").prop('checked', true);
    $("#chkTodosUsuarios").prop('checked', false);
    $(".smcImputValidacion").removeClass("is-invalid")
    $(".smcLabelValidacion").addClass("d-none")

}

function ListarModulos() {

    $.ajax({
        url: "ObtenerModulos",
        type: "GET",
        success: function (data) {

            let Modulos = (data);

            let tr = '<br/>';

            for (var i = 0; i < Modulos.length; i++) {
                tr += `
                    <div class="row">
                        <div class='col-sm-12 mb-3'>
                            <div class="form-check">
                                <input class="form-check-input" name="chkModulo[]" IdModulo="`+ Modulos[i].Id +`" type="checkbox" id="chkModulo`+ Modulos[i].Id + `">
                                <label class="form-check-label" for="chkModulo`+ Modulos[i].Id + `">
                                    `+ Modulos[i].Descripcion + `
                                </label>
                            </div>
                        </div>
                    </div>
                    <br/>`;
            }
            console.log(tr)
            $("#divModulos").html(tr);

        },
        error: function (jqXHR, textStatus, errorThrown) {
            if (jqXHR.status === 401) {
                manejarTokenExpirado();
            } else {
                try {
                    var err = JSON.parse(jqXHR.responseText);
                    Swal.fire("Error", "Error " + jqXHR.status + ": " + err.message, "error")
                } catch (e) {
                    Swal.fire("Error", "Error " + jqXHR.status + ": " + jqXHR.responseText, "error")
                }
            }
        }
    });

}
function CargarUsuarios() {
    $.ajaxSetup({ async: false });

    $.ajax({
        url: "/Usuario/ObtenerUsuarios",
        type: "GET",
        data: { 'MostrarInactivos': false },
        success: function (data) {
            Usuarios = (data);
        },
        error: function (jqXHR, textStatus, errorThrown) {
            if (jqXHR.status === 401) {
                manejarTokenExpirado();
            } else {
                try {
                    var err = JSON.parse(jqXHR.responseText);
                    Swal.fire("Error", "Error " + jqXHR.status + ": " + err.message, "error")
                } catch (e) {
                    Swal.fire("Error", "Error " + jqXHR.status + ": " + jqXHR.responseText, "error")
                }
            }
        }
    });
}

function CargarEtapas() {
    $.ajaxSetup({ async: false });

    $.ajax({
        url: "/EtapaAutorizacion/ListarEtapaAutorizacion",
        type: "GET",
        data: { 'MostrarInactivos': false },
        success: function (data) {
            Etapas = (data);
        },
        error: function (jqXHR, textStatus, errorThrown) {
            if (jqXHR.status === 401) {
                manejarTokenExpirado();
            } else {
                try {
                    var err = JSON.parse(jqXHR.responseText);
                    Swal.fire("Error", "Error " + jqXHR.status + ": " + err.message, "error")
                } catch (e) {
                    Swal.fire("Error", "Error " + jqXHR.status + ": " + jqXHR.responseText, "error")
                }
            }
        }
    });
}

function AgregarLinea(Id) {

    let tr = '';

    tr += `<tr>
            <td><input style="display:none;" class="form-control" type="text" value="0" id="txtIdEtapaAutorizacionDetalle" name="txtIdEtapaAutorizacionDetalle[]"/></td>
           <td>
            <select class="form-select form-control-modern" name="cboUsuario[]" id="cboUsuario`+ contador + `" >`;
    tr += `  <option value="0">Seleccione</option>`;
    for (var i = 0; i < Usuarios.length; i++) {

        if (Usuarios[i].IdUsuario == Id) {
            tr += `  <option value="` + Usuarios[i].IdUsuario + `" selected>` + Usuarios[i].NombreUsuario + `</option>`;
        } else {
            tr += `  <option value="` + Usuarios[i].IdUsuario + `">` + Usuarios[i].NombreUsuario + `</option>`;

        }
    }
    tr += `</select>
            </td>
            <td><button class="btn btn-xs btn-danger fa fa-trash borrar" data-bs-toggle="tooltip" data-bs-placement="top" title="Eliminar"></button></td>
            </tr>`;

    $("#tabla").find('tbody').append(tr);
    TriggerTooltipB5();
    contador++;
}

function AgregarLineaEtapas(Id) {

    let tr = '';

    tr += `<tr>
            <td><input style="display:none;" class="form-control" type="text" value="0" id="txtIdModeloAutorizacionEtapa" name="txtIdModeloAutorizacionEtapa[]"/></td>
           <td>
            <select class="form-select form-control-modern" name="cboEtapa[]" id="cboEtapa`+ contador1 + `" onchange="ObtenerDescripcionEtapa(` + contador1 + `)">`;
    tr += `  <option value="0">Seleccione</option>`;
    for (var i = 0; i < Etapas.length; i++) {
        if (Etapas[i].IdEtapaAutorizacion == Id) {
            tr += `  <option value="` + Etapas[i].IdEtapaAutorizacion + `" selected>` + Etapas[i].NombreEtapa + `</option>`;
        }else{
            tr += `  <option value="` + Etapas[i].IdEtapaAutorizacion + `">` + Etapas[i].NombreEtapa + `</option>`;
        }

    }
    tr += `</select>
            </td>         
            <td><button class="btn btn-xs btn-danger fa fa-trash borrar" data-bs-toggle="tooltip" data-bs-placement="top" title="Eliminar"></button></td>
            </tr>`;

    $("#tablaEtapas").find('tbody').append(tr);
    TriggerTooltipB5();
    contador1++;
}


function AgregarLineaCondiciones(texto) {


    let tr = '';

    tr += `<tr>
            <td><input style="display:none;" class="form-control form-control-modern" type="text" value="0" id="txtIdModeloAutorizacionCondicion" name="txtIdModeloAutorizacionCondicion[]"/></td>
           <td>
            <input class="form-control form-control-modern" type="text" id="txtCondicion" name="txtCondicion[]" value="`+ texto +`"/>
            </td>
            <td><button class="btn btn-xs btn-danger borrar fa fa-trash" data-bs-toggle="tooltip" data-bs-placement="top" title="Eliminar"></button></td>
            </tr>`;

    $("#tablaCondiciones").find('tbody').append(tr);
    TriggerTooltipB5();
}

function GuardarModeloAutorizacion() {

    let SubDominio = $("#txtSubDominio").val()
    let varIdModeloAutorizacion = $("#txtId").val() || "0";
    let varNombreModelo = $("#txtNombreModelo").val();
    let varDescripcionModelo = $("#txtDescripcionModelo").val();
    let varEstado = false;

    if ($('#chkActivo')[0].checked) {
        varEstado = true;
    }

    let varIncluirTodos = false;

    if ($('#chkTodosUsuarios')[0].checked) {
        varIncluirTodos = true;
    }

    let DetalleAutor = [];
    $("select[name='cboUsuario[]']").each(function (indice, elemento) {
        DetalleAutor.push({ 'IdAutor': $(elemento).val() });
    });

    let DetalleEtapa = [];
    $("select[name='cboEtapa[]']").each(function (indice, elemento) {
        DetalleEtapa.push({ 'IdEtapa': $(elemento).val() });
    });

    let DetalleCondicion = [];
    $("input[name='txtCondicion[]']").each(function (indice, elemento) {
        DetalleCondicion.push({ 'Condicion': $(elemento).val() });
    });

    let DetalleModulo = [];
    $("input[name='chkModulo[]']").each(function (indice, elemento) {
        if ($(elemento).prop("checked")) {
            DetalleModulo.push({ 'IdModulo': $(elemento).attr("IdModulo") });
        }
    });

    const campos = [
        { id: "#txtNombreModelo", mensaje: "Complete el campo Nombre" },
        { id: "#txtDescripcionModelo", mensaje: "Complete el campo Descripción" }
    ];

    let hayErrores = false;

    $(".smcImputValidacion").removeClass("is-invalid")
    $(".smcLabelValidacion").addClass("d-none")

    campos.forEach(c => {
        const valor = $(c.id).val()?.trim();

        if (!valor) {
            $(c.id).addClass("is-invalid");
            $(`#error-${c.id.replace("#", "")}`)
                .removeClass("d-none")
                .text(c.mensaje);

            hayErrores = true;
            return;
        }

        if (c.adicional && !c.adicional(valor)) {
            $(c.id).addClass("is-invalid");
            $(`#error-${c.id.replace("#", "")}`)
                .removeClass("d-none")
                .text(c.mensaje);

            hayErrores = true;
        }
    });


    if (DetalleAutor.length == 0 && $("#chkTodosUsuarios").prop("checked")==false) {
        $(`#error-tabla`)
            .removeClass("d-none")
            .text("Seleccione al Menos un Autor");

        hayErrores = true;
    }

    for (var i = 0; i < DetalleAutor.length; i++) {
        if (DetalleAutor[i].IdAutor == 0) {
            $(`#error-tabla`)
                .removeClass("d-none")
                .text("Seleccione el Autor para la Columna N° " + (i + 1));

            hayErrores = true;
        }
    }

    if (DetalleEtapa.length == 0) {
        $(`#error-tabla`)
            .removeClass("d-none")
            .text("Seleccione al Menos una Etapa");

        hayErrores = true;
    }

    for (var i = 0; i < DetalleEtapa.length; i++) {
        if (DetalleEtapa[i].IdEtapa == 0) {
            $(`#error-tabla`)
                .removeClass("d-none")
                .text("Seleccione la Etapa para la Columna N° " + (i + 1));

            hayErrores = true;
        }
    }

    if (DetalleModulo.length == 0) {
        $(`#error-tabla`)
            .removeClass("d-none")
            .text("Seleccione al Menos un Modulo");

        hayErrores = true;
    }


    if (DetalleCondicion.length == 0) {
        $(`#error-tabla`)
            .removeClass("d-none")
            .text("Ingrese al Menos una Condicion");

        hayErrores = true;
    }

    for (var i = 0; i < DetalleCondicion.length; i++) {
        if (DetalleCondicion[i].Condicion == '') {
            $(`#error-tabla`)
                .removeClass("d-none")
                .text("Ingrese la Condición para la Columna N° " + (i + 1));

            hayErrores = true;
        }
    }
  
    if (hayErrores) {
        return;
    }

    $("#btnGrabar").text("Guardando...")
    $("#btnGrabar").prop("disabled", true)



    $.ajax({
        url: 'UpdateInsertModeloAutorizacion',
        type: 'POST',
        contentType: "application/json; charset=utf-8",
        data: JSON.stringify({
            'IdModeloAutorizacion': varIdModeloAutorizacion,
            'NombreModelo': varNombreModelo,
            'DescripcionModelo': varDescripcionModelo,
            'Estado': varEstado,
            'IncluirTodosUsuarios': varIncluirTodos,
            DetalleAutor, 
            DetalleEtapa, 
            DetalleCondicion, 
            DetalleModulo, 

        }),
        success: function (data, status) {
           
            Swal.fire("Éxito!", "Proceso Realizado Correctamente", "success");
            table.ajax.reload(null, false);
            CerrarModal();          
               
        },
        error: function (jqXHR, textStatus, errorThrown) {
            if (jqXHR.status === 401) {
                manejarTokenExpirado();
            } else {
                try {
                    var err = JSON.parse(jqXHR.responseText);
                    Swal.fire("Error", "Error " + jqXHR.status + ": " + err.message, "error")
                } catch (e) {
                    Swal.fire("Error", "Error " + jqXHR.status + ": " + jqXHR.responseText, "error")
                }
            }
        }, complete: function () {
            $("#btnGrabar").text("Grabar")
            $("#btnGrabar").prop("disabled", false)
        }
    });

}

function ConsultaServidor() {

    table = $("#table_id").DataTable({
        ajax: {
            url: 'ListarModeloAutorizacion',
            type: 'GET',
            dataSrc: '',
            data: { MostrarInactivos: true },
            error: function (jqXHR) {
                if (jqXHR.status === 401) {
                    manejarTokenExpirado(function () { ConsultaServidor() });
                } else {
                    try {
                        var err = JSON.parse(jqXHR.responseText);
                        Swal.fire("Error", "Error " + jqXHR.status + ": " + err.message, "error")
                    } catch (e) {
                        Swal.fire("Error", "Error " + jqXHR.status + ": " + jqXHR.responseText, "error")
                    }
                }
            }
        },
        columns: [
            {
                data: null,
                render: function (data, type, row, meta) {
                    return meta.row + 1;
                }
            },
            {
                data: "NombreModelo",
                render: function (data) {
                    return data.toUpperCase();
                }
            },
            {
                data: "DescripcionModelo",
                render: function (data) {
                    return data.toUpperCase();
                }
            },
            {
                data: "Estado",
                render: function (data) {
                    if (data) {
                        return '<span class="badge bg-success bg-opacity-10 text-success border border-success rounded-pill">ACTIVO</span>';
                    }
                    return '<span class="badge bg-danger bg-opacity-10 text-danger border border-danger rounded-pill">INACTIVO</span>';
                }
            },
            {
                data: "IdModeloAutorizacion",
                render: function (data) {
                    return '<button class="btn btn-primary fa fa-pencil btn-xs me-1" onclick="ObtenerDatosxID(' + data + ')" data-bs-toggle="tooltip" title="Editar"></button>';
                }
            }
        ],

        language: lenguaje_data,

        initComplete: function () {
            TriggerTooltipB5();
        }
    });

}


function ObtenerDatosxID(varIdModeloAutorizacion) {
    $("#lblTituloModal").html("Editar Modelo de Autorizacion");
    AbrirModal("modal-form");

    $.ajax({
        url: 'ObtenerModeloAutorizacion',
        type: 'GET',
        data: {
            'IdModeloAutorizacion': varIdModeloAutorizacion
        },
        success: function (data, status) {

            let modeloautorizacion = (data);

            $("#txtId").val(modeloautorizacion.IdModeloAutorizacion);
            $("#txtNombreModelo").val(modeloautorizacion.NombreModelo);
            $("#txtDescripcionModelo").val(modeloautorizacion.DescripcionModelo);
            $("#chkActivo").prop('checked', false);
            if (modeloautorizacion.Estado) {
                $("#chkActivo").prop('checked', true);
            }
            $("#chkTodosUsuarios").prop('checked', false);
            if (modeloautorizacion.IncluirTodosUsuarios) {
                $("#chkTodosUsuarios").prop('checked', true);
            }

            let DetallesAutor = modeloautorizacion.DetalleAutor;
            let DetallesEtapa = modeloautorizacion.DetalleEtapa;
            let DetallesDocumento = modeloautorizacion.DetalleModulo;
            let DetallesCondicion = modeloautorizacion.DetalleCondicion;

            for (var i = 0; i < DetallesAutor.length; i++) {
                AgregarLinea(DetallesAutor[i].IdAutor);
            }

            for (var i = 0; i < DetallesEtapa.length; i++) {
                AgregarLineaEtapas(DetallesEtapa[i].IdEtapa);
            }

            for (var i = 0; i < DetallesDocumento.length; i++) {
                $("#chkModulo" + DetallesDocumento[i].IdModulo).prop("checked",true)
            }

            for (var i = 0; i < DetallesCondicion.length; i++) {
                AgregarLineaCondiciones(DetallesCondicion[i].Condicion);
            }

        },
        error: function (jqXHR, textStatus, errorThrown) {
            if (jqXHR.status === 401) {
                manejarTokenExpirado();
                throw new Error("Token expirado");
                // window.location.href = "/login";
            } else {
                console.error("Error:", textStatus, errorThrown);
                console.error("Código de estado:", jqXHR.status);
                console.error("Respuesta del servidor:", jqXHR.responseText);
            }
        }
    });




}


//--------------------------------------------------------------



//let contador2 = 0;


function ObtenerDescripcionEtapa(contador) {

    let IdEtapa = $("#cboEtapa" + contador).val();
    $.ajaxSetup({ async: false });
    $.ajax({
        url: "/EtapaAutorizacion/ObtenerDatosxID",
        type: "POST",
        data: {
            IdEtapaAutorizacion: IdEtapa
        },
        success: function (data) {
            var errorEmpresa = validarEmpresa(data);
            if (errorEmpresa) {
                return;
            }

            let datos = JSON.parse(data);
            $("#txtDescripcionEtapa" + contador).val(datos[0].DescripcionEtapa);
        },
        error: function (jqXHR, textStatus, errorThrown) {
            if (jqXHR.status === 401) {
                manejarTokenExpirado();
                throw new Error("Token expirado");
                // window.location.href = "/login";
            } else {
                console.error("Error:", textStatus, errorThrown);
                console.error("Código de estado:", jqXHR.status);
                console.error("Respuesta del servidor:", jqXHR.responseText);
            }
        }
    });


}




function ObtenerDepartamentoUsuario(contador) {


}



function openContenido(evt, Name) {
    var i, tabcontent, tablinks;
    tabcontent = document.getElementsByClassName("tabcontent");
    for (i = 0; i < tabcontent.length; i++) {
        tabcontent[i].style.display = "none";
    }
    tablinks = document.getElementsByClassName("tablinks");
    for (i = 0; i < tablinks.length; i++) {
        tablinks[i].className = tablinks[i].className.replace(" active", "");
    }
    document.getElementById(Name).style.display = "block";
    evt.currentTarget.className += " active";
}


function AgregarLineaDetalleDocumentos(contador, IdModeloAutorizacionDocumento, IdDocumento) {

    let tr = '';

    tr += `<tr>
            <td><input style="display:none;" class="form-control" type="text" value="`+ IdModeloAutorizacionDocumento + `" id="txtIdModeloAutorizacionDocumento" name="txtIdModeloAutorizacionDocumento[]"/></td>
            <td>
             <div class="checkbox-custom col-xs-6">
                  <input type="checkbox" id="chkSolicitudCompra" />
                  <label for="chkSolicitudCompra">Solicitud de Compra</label>
              </div>`

    let SubDominio = $("#txtSubDominio").val()
    if (SubDominio != "merieux") {
        tr += `<div class="checkbox-custom col-xs-6" >
                  <input type="checkbox" id="chkOrdenCompra" />
                  <label for="chkOrdenCompra">Orden de Compra (Pedido)</label>
              </div >
               <div class="checkbox-custom col-xs-6">
                  <input type="checkbox" id="chkFactura" />
                  <label for="chkFactura">Factura</label>
              </div>
               <div class="checkbox-custom col-xs-6">
                  <input type="checkbox" id="chkDAM" />
                  <label for="chkDAM">DAM</label>
              </div>
              `
    }
    tr += ` <div class="checkbox-custom col-xs-6">
                <input type="checkbox" id="chkInvitacionProveedor" />
                <label for="chkInvitacionProveedor">Invitaciones a Proveedor</label>
            </div>`;

    tr += ` </td >
            </tr >`;



    $("#tablaDocumentos").find('tbody').append(tr);

    if (IdDocumento == 1) {
        $('#chkSolicitudCompra').prop('checked', true);
    }

    if (IdDocumento == 4) {
        $('#chkOrdenCompra').prop('checked', true);
    }

    if (IdDocumento == 5) {
        $('#chkFactura').prop('checked', true);
    }

    if (IdDocumento == 6) {
        $('#chkDAM').prop('checked', true);
    }

    if (IdDocumento == 7) {
        $('#chkInvitacionProveedor').prop('checked', true);
    }

}


function AgregarLineaDetalleCondicion(contador1, IdModeloAutorizacionCondicion, Condicion) {

    let tr = '';

    tr += `<tr>
            <td><input style="display:none;" class="form-control" type="text" value="`+ IdModeloAutorizacionCondicion + `" id="txtIdModeloAutorizacionCondicion" name="txtIdModeloAutorizacionCondicion[]"/></td>
            <td>
                <input  class="form-control" type="text" value="`+ Condicion + `"  id="txtCondicion` + contador1 + `" name="txtCondicion[]"/>
            </td>
            <td><button class="btn btn-xs btn-danger fa fa-trash" onclick="EliminarModeloAutorizacionDetalleCondicion(`+ IdModeloAutorizacionCondicion + `,this)" data-bs-toggle="tooltip" data-bs-placement="top" title="Eliminar"></button></td>
            </tr>`;

    $("#tablaCondiciones").find('tbody').append(tr);
    TriggerTooltipB5();
}



function AgregarLineaDetalleEtapa(contador1, IdModeloAutorizacionEtapa, IdEtapa, DescripcionEtapa) {

    let Etapas;
    $.ajaxSetup({ async: false });
    $.ajax({
        url: "/EtapaAutorizacion/ObtenerEtapaAutorizacion",
        type: "POST",
        success: function (data, status) {
            var errorEmpresa = validarEmpresa(data);
            if (errorEmpresa) {
                return;
            }
            Etapas = JSON.parse(data);
        },
        error: function (jqXHR, textStatus, errorThrown) {
            if (jqXHR.status === 401) {
                manejarTokenExpirado();
                throw new Error("Token expirado");
                // window.location.href = "/login";
            } else {
                console.error("Error:", textStatus, errorThrown);
                console.error("Código de estado:", jqXHR.status);
                console.error("Respuesta del servidor:", jqXHR.responseText);
            }
        }
    });


    let tr = '';

    tr += `<tr>
            <td><input style="display:none;" class="form-control" type="text" value="`+ IdModeloAutorizacionEtapa + `" id="txtIdModeloAutorizacionEtapa" name="txtIdModeloAutorizacionEtapa[]"/></td>
           <td>
            <select class="form-select" name="cboEtapa[]" id="cboEtapa`+ contador1 + `" onchange="ObtenerDescripcionEtapa(` + contador1 + `)">`;
    tr += `  <option value="0">Seleccione</option>`;
    for (var i = 0; i < Etapas.length; i++) {
        if (Etapas[i].IdEtapaAutorizacion == IdEtapa) {
            tr += `  <option value="` + Etapas[i].IdEtapaAutorizacion + `" selected>` + Etapas[i].NombreEtapa + `</option>`;
        } else {
            tr += `  <option value="` + Etapas[i].IdEtapaAutorizacion + `">` + Etapas[i].NombreEtapa + `</option>`;
        }

    }
    tr += `</select>
            </td>
            <td>
                <input  class="form-control" type="text" value="`+ DescripcionEtapa + `"  id="txtDescripcionEtapa` + contador1 + `" name="txtDescripcionEtapa[]"/>
            </td>
            <td><button class="btn btn-xs btn-danger fa fa-trash" onclick="EliminarModeloAutorizacionDetalleEtapa(`+ IdModeloAutorizacionEtapa + `,this)" data-bs-toggle="tooltip" data-bs-placement="top" title="Eliminar"></button></td>
            </tr>`;

    $("#tablaEtapas").find('tbody').append(tr);
    TriggerTooltipB5();
}

function AgregarLineaDetalleAutor(contador, IdModeloAutorizacionAutor, IdAutor, IdDepartamento) {
    let Departamentos;
    let Usuarios;

    $.ajaxSetup({ async: false });
    //$.post("/Departamento/ObtenerDepartamentos", function (data, status) {
    //    var errorEmpresa = validarEmpresa(data);
    //    if (errorEmpresa) {
    //        return;
    //    }
    //    Departamentos = JSON.parse(data);
    //});

    $.ajax({
        url: "/Usuario/ObtenerUsuarios",
        type: "POST",
        success: function (data) {
            var errorEmpresa = validarEmpresa(data);
            if (errorEmpresa) {
                return;
            }
            Usuarios = JSON.parse(data);
        },
        error: function (jqXHR, textStatus, errorThrown) {
            if (jqXHR.status === 401) {
                manejarTokenExpirado();
                throw new Error("Token expirado");
                // window.location.href = "/login";
            } else {
                console.error("Error:", textStatus, errorThrown);
                console.error("Código de estado:", jqXHR.status);
                console.error("Respuesta del servidor:", jqXHR.responseText);
            }
        }
    });


    let tr = '';

    tr += `<tr>
            <td><input style="display:none;" class="form-control" type="text" value="`+ IdModeloAutorizacionAutor + `" id="txtIdModeloAutorizacionAutor" name="txtIdModeloAutorizacionAutor[]"/></td>
           <td>
            <select class="form-select" name="cboUsuario[]" id="cboUsuario`+ contador + `" >`;
    tr += `  <option value="0">Seleccione</option>`;
    for (var i = 0; i < Usuarios.length; i++) {
        if (Usuarios[i].IdUsuario == IdAutor) {
            tr += `  <option value="` + Usuarios[i].IdUsuario + `" selected>` + Usuarios[i].NombreUsuario + `</option>`;
        } else {
            tr += `  <option value="` + Usuarios[i].IdUsuario + `">` + Usuarios[i].NombreUsuario + `</option>`;
        }

    }
    tr += `</select>
            </td>
            <td><button class="btn btn-xs btn-danger fa fa-trash" onclick="EliminarModeloAutorizacionDetalleAutor(`+ IdModeloAutorizacionAutor + `,this)" data-bs-toggle="tooltip" data-bs-placement="top" title="Eliminar"></button></td>
            </tr>`;

    $("#tabla").find('tbody').append(tr);
    TriggerTooltipB5();
    //contador++;
}





function EliminarModeloAutorizacionDetalleAutor(IdModeloAutorizacionAutor, dato) {


    Swal.fire({
        title: 'Desea Eliminar este Registro?',
        text: 'No podras revertir esta opcion',
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        cancelButtonText: 'No, cancelar',
        confirmButtonText: 'Si, Eliminar'
    }).then((result) => {
        if (result.isConfirmed) {

            $.ajax({
                url: "EliminarModeloAutorizacionDetalleAutor",
                type: "POST",
                data: { 'IdModeloAutorizacionAutor': IdModeloAutorizacionAutor },
                success: function (data, status) {
                    var errorEmpresa = validarEmpresaUpdateInsert(data);
                    if (errorEmpresa) {
                        return;
                    }

                    if (data == 0) {
                        Swal.fire("Error!", "Ocurrió un error");
                    } else {
                        Swal.fire("Éxito!", "Item eliminado", "success");
                        $(dato).closest('tr').remove();
                    }
                },
                error: function (jqXHR, textStatus, errorThrown) {
                    if (jqXHR.status === 401) {
                        manejarTokenExpirado();
                        throw new Error("Token expirado");
                        // window.location.href = "/login";
                    } else {
                        console.error("Error:", textStatus, errorThrown);
                        console.error("Código de estado:", jqXHR.status);
                        console.error("Respuesta del servidor:", jqXHR.responseText);
                    }
                }
            });


        }
    })
}

function EliminarModeloAutorizacionDetalleEtapa(IdModeloAutorizacionEtapa, dato) {

    Swal.fire({
        title: 'Desea Eliminar este Registro?',
        text: 'No podras revertir esta opcion',
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        cancelButtonText: 'No, cancelar',
        confirmButtonText: 'Si, Eliminar'
    }).then((result) => {
        if (result.isConfirmed) {


            $.ajax({
                url: "EliminarModeloAutorizacionDetalleEtapa",
                type: "POST",
                data: {
                    'IdModeloAutorizacionEtapa': IdModeloAutorizacionEtapa
                },
                success: function (data) {
                    var errorEmpresa = validarEmpresaUpdateInsert(data);
                    if (errorEmpresa) {
                        return;
                    }

                    if (data == 0) {
                        Swal.fire("Error!", "Ocurrio un Error");
                    } else {
                        Swal.fire("Exito!", "Item Eliminado", "success");
                        $(dato).closest('tr').remove();
                    }
                },
                error: function (jqXHR, textStatus, errorThrown) {
                    if (jqXHR.status === 401) {
                        manejarTokenExpirado();
                        throw new Error("Token expirado");
                        // window.location.href = "/login";
                    } else {
                        console.error("Error:", textStatus, errorThrown);
                        console.error("Código de estado:", jqXHR.status);
                        console.error("Respuesta del servidor:", jqXHR.responseText);
                    }
                }
            });


        }
    })
}

function EliminarModeloAutorizacionDetalleCondicion(IdModeloAutorizacionCondicion, dato) {
    Swal.fire({
        title: 'Desea Eliminar este Registro?',
        text: 'No podras revertir esta opcion',
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        cancelButtonText: 'No, cancelar',
        confirmButtonText: 'Si, Eliminar'
    }).then((result) => {
        if (result.isConfirmed) {


            $.ajax({
                url: "EliminarModeloAutorizacionDetalleCondicion",
                type: "POST",
                data: {
                    'IdModeloAutorizacionCondicion': IdModeloAutorizacionCondicion
                },
                success: function (data) {
                    var errorEmpresa = validarEmpresaUpdateInsert(data);
                    if (errorEmpresa) {
                        return;
                    }

                    if (data == 0) {
                        Swal.fire("Error!", "Ocurrio un Error");
                    } else {
                        Swal.fire("Exito!", "Item Eliminado", "success");
                        $(dato).closest('tr').remove();
                    }
                },
                error: function (jqXHR, textStatus, errorThrown) {
                    if (jqXHR.status === 401) {
                        manejarTokenExpirado();
                        throw new Error("Token expirado");
                        // window.location.href = "/login";
                    } else {
                        console.error("Error:", textStatus, errorThrown);
                        console.error("Código de estado:", jqXHR.status);
                        console.error("Respuesta del servidor:", jqXHR.responseText);
                    }
                }
            });


        }
    })
}




function validarEmpresa(rpta) {
    if (rpta == "SinBD") {   //Sin Session
        window.location.href = "/";
        return true;
    }
    return false;
}

function validarEmpresaUpdateInsert(rpta) {
    if (rpta == -999) {   //Sin Session
        window.location.href = "/";
        return true;
    }
    return false;
}


function eliminar(IdModelo) {
    Swal.fire({
        title: 'Desea Eliminar este Registro?',
        text: 'No podras revertir esta opcion',
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        cancelButtonText: 'No, cancelar',
        confirmButtonText: 'Si, Eliminar'
    }).then((result) => {
        if (result.isConfirmed) {

            $.ajax({
                url: "EliminarModeloAutorizacion",
                type: "POST",
                data: {
                    'IdModeloAutorizacion': IdModelo
                },
                success: function (data) {
                    if (data == "0") {
                        Swal.fire("Error!", "No se puede eliminar modelo de autorización");
                    } else {
                        Swal.fire("Éxito!", "Se eliminó correctamente");
                        table.ajax.reload(null, false);
                        limpiarDatos();
                    }
                },
                error: function (jqXHR, textStatus, errorThrown) {
                    if (jqXHR.status === 401) {
                        manejarTokenExpirado();
                        throw new Error("Token expirado");
                        // window.location.href = "/login";
                    } else {
                        console.error("Error:", textStatus, errorThrown);
                        console.error("Código de estado:", jqXHR.status);
                        console.error("Respuesta del servidor:", jqXHR.responseText);
                    }
                }
            });


        }
    })
}
function validacionesAutor(elemento) {


    let chkRQ = $("#chkSolicitudCompra").prop("checked")
    let chkOrdenCompra = $("#chkOrdenCompra").prop("checked")

    let chkFactura = $("#chkFactura").prop("checked")
    let chkDAM = $("#chkDAM").prop("checked")

    let chkInvitacion = $("#chkInvitacionProveedor").prop("checked")

    if (chkRQ == false && chkOrdenCompra == false && chkFactura == false && chkDAM == false && chkInvitacion == false) {
        Swal.fire("Espere", "Debe seleccionar primero una opción en la Pestaña de Documentos", "info");
        $(elemento).val(0)
        return
    }

    let IdAutor = $(elemento).val()

    let IdTipoDoc;

    if (chkRQ) IdTipoDoc = 1
    if (chkOrdenCompra) IdTipoDoc = 4
    if (chkFactura) IdTipoDoc = 5
    if (chkDAM) IdTipoDoc = 6
    if (chkInvitacion) IdTipoDoc = 7

    $.ajax({
        url: "ValidarAutoresxTipoDocumento",
        type: "POST",
        data: {
            IdAutor: IdAutor,
            IdTipoDocumento: IdTipoDoc
        },
        success: function (data) {
            if (data != "ok") {
                Swal.fire("Error", data, "info");
                $(elemento).val(0);
            }
        },
        error: function (jqXHR, textStatus, errorThrown) {
            if (jqXHR.status === 401) {
                manejarTokenExpirado();
                throw new Error("Token expirado");
                // window.location.href = "/login";
            } else {
                console.error("Error:", textStatus, errorThrown);
                console.error("Código de estado:", jqXHR.status);
                console.error("Respuesta del servidor:", jqXHR.responseText);
            }
        }
    });


}
