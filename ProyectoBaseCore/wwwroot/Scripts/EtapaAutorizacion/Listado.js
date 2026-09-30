let table = '';
let contador = 0;
let Usuarios;


window.onload = function () {
    CargarUsuarios()
    ConsultaServidor();
    $(document).on('click', '.borrar', function (event) {
        event.preventDefault();
        $(this).closest('tr').remove();
    });

    TriggerTooltipB5();

};

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
function ModalNuevo() {
    $("#lblTituloModal").html("Nueva Etapa de Autorizacion");
    AbrirModal("modal-form");
    $('#chkActivo').prop('checked', true);
    AgregarLinea(0);
}


function AgregarLinea(Id) {

    let tr = '';

    tr += `<tr>
            <td><input style="display:none;" class="form-control" type="text" value="0" id="txtIdEtapaAutorizacionDetalle" name="txtIdEtapaAutorizacionDetalle[]"/></td>
           <td>
            <select class="form-select" name="cboUsuario[]" id="cboUsuario`+ contador + `" >`;
    tr += `  <option value="0">Seleccione</option>`;
    for (var i = 0; i < Usuarios.length; i++) {

        if (Usuarios[i].IdUsuario == Id) {
            tr += `  <option value="` + Usuarios[i].IdUsuario + `" selected>` + Usuarios[i].NombreUsuario + `</option>`;
        }else{
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

function GuardarEtapaAutorizacion() {

    let varIdEtapaAutorizacion = $("#txtId").val() || "0";
    let varNombreEtapa = $("#txtNombreEtapa").val();
    let varDescripcionEtapa = $("#txtDescripcionEtapa").val();
    let varAutorizacionesRequeridas = $("#txtAutorizacionesRequeridas").val();
    let varRechazosRequeridos = $("#txtRechazosRequeridos").val();
    let varEstado = false;

    if ($('#chkActivo')[0].checked) {
        varEstado = true;
    }

    let arrayGeneral = new Array();
    let arrayIdEtapaAutorizacionDetalle = new Array();
    let arrayIdUsuario = new Array();

    $("input[name='txtIdEtapaAutorizacionDetalle[]']").each(function (indice, elemento) {
        arrayIdEtapaAutorizacionDetalle.push($(elemento).val());
    });
    $("select[name='cboUsuario[]']").each(function (indice, elemento) {
        arrayIdUsuario.push($(elemento).val());
    });

    for (var i = 0; i < arrayIdUsuario.length; i++) {
        arrayGeneral.push({ 'IdEtapaAutorizacionDetalle': arrayIdEtapaAutorizacionDetalle[i], 'IdUsuario': arrayIdUsuario[i]})
    }

    const campos = [
        { id: "#txtNombreEtapa", mensaje: "Complete el campo Nombre" },
        { id: "#txtDescripcionEtapa", mensaje: "Complete el campo Descripción" },
        { id: "#txtAutorizacionesRequeridas", mensaje: "Ingrese una Cantidad de Autorizaciones Válida", adicional: v => v > 0 },
        { id: "#txtRechazosRequeridos", mensaje: "Ingrese una Cantidad de Rechazos Válida", adicional: v => v > 0 },
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

    if (varAutorizacionesRequeridas > arrayGeneral.length ) {
        $(`#error-tabla`)
            .removeClass("d-none")
            .text("La Cantidad de Autorizaciones Requeridas supera a los Autorizadores listados");
            $("#txtAutorizacionesRequeridas").addClass("is-invalid");

        hayErrores = true;
    }

    if (varRechazosRequeridos > arrayGeneral.length) {
        $(`#error-tabla`)
            .removeClass("d-none")
            .text("La Cantidad de Rechazos Requeridas supera a los Autorizadores listados");
            $("#txtRechazosRequeridos").addClass("is-invalid");
        hayErrores = true;
    }

    $("select[name='cboUsuario[]']").each(function (indice, elemento) {
        if ($(elemento).val() == 0) {
            $(`#error-tabla`)
                .removeClass("d-none")
                .text("Seleccione el Autorizador para la Columna N° " +(indice+1));

            hayErrores = true;
        }
    });

    if (hayErrores) {
        return;
    }

    $("#btnGrabar").text("Guardando...")
    $("#btnGrabar").prop("disabled", true)

    $.ajax({
        url: 'UpdateInsertEtapaAutorizacion',
        type: 'POST',
        contentType: "application/json; charset=utf-8",
        data: JSON.stringify({
            'IdEtapaAutorizacion': varIdEtapaAutorizacion,
            'NombreEtapa': varNombreEtapa,
            'DescripcionEtapa': varDescripcionEtapa,
            'AutorizacionesRequeridas': varAutorizacionesRequeridas,
            'RechazosRequeridos': varRechazosRequeridos,
            'Estado': varEstado,
            'Detalle': arrayGeneral // Asegura que se envíe correctamente
        }),
        success: function (data) {

                Swal.fire("Éxito!", "Proceso Realizado Correctamente", "success");
            table.ajax.reload(null, false);
                limpiarDatos();
                $("#modal-form").modal("hide");
                $("#tabla").find('tbody').empty();
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

function limpiarDatos() {
    $("#txtId").val("");
    $("#txtNombreEtapa").val("");
    $("#txtDescripcionEtapa").val("");
    $("#txtAutorizacionesRequeridas").val("");
    $("#txtRechazosRequeridos").val("");
    $("#chkActivo").prop('checked', true);
    $(".smcImputValidacion").removeClass("is-invalid")
    $(".smcLabelValidacion").addClass("d-none")
}



function CerrarModal() {
    $("#tabla").find('tbody').empty();
    $("#modal-form").modal('hide')
    limpiarDatos();
}


function ConsultaServidor() {

    table = $("#table_id").DataTable({
        ajax: {
            url: 'ListarEtapaAutorizacion',
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
                data: "NombreEtapa",
                render: function (data) {
                    return data.toUpperCase();
                }
            },
            {
                data: "AutorizacionesRequeridas",
                render: function (data) {
                    return data;
                }
            },
            {
                data: "RechazosRequeridos",
                render: function (data) {
                    return data;
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
                data: "IdEtapaAutorizacion",
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

function ObtenerDatosxID(varIdEtapaAutorizacion) {
    $("#lblTituloModal").html("Editar Etapa de Autorizacion");
    AbrirModal("modal-form");

    //console.log(varIdUsuario);

    $.ajax({
        url: 'ObtenerEtapaAutorizacion',
        type: 'GET',
        data: {
            'IdEtapaAutorizacion': varIdEtapaAutorizacion
        },
        success: function (data) {
           
                let etapaautorizacion = (data);

                $("#txtId").val(etapaautorizacion.IdEtapaAutorizacion);
                $("#txtNombreEtapa").val(etapaautorizacion.NombreEtapa);
                $("#txtDescripcionEtapa").val(etapaautorizacion.DescripcionEtapa);
                $("#txtAutorizacionesRequeridas").val(etapaautorizacion.AutorizacionesRequeridas);
                $("#txtRechazosRequeridos").val(etapaautorizacion.RechazosRequeridos);
                $("#chkActivo").prop('checked', false);
                if (etapaautorizacion.Estado) {
                    $("#chkActivo").prop('checked', true);
                }

            let Detalles = etapaautorizacion.Detalle;
                console.log(Detalles);

            for (var i = 0; i < Detalles.length; i++) {
                AgregarLinea(Detalles[i].IdUsuario)
                    //AgregarLineaDetalle(i, Detalles[i].IdEtapaAutorizacionDetalle, Detalles[i].IdUsuario, Detalles[i].IdDepartamento);
            }
            
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


//---------------------------------------------------------
