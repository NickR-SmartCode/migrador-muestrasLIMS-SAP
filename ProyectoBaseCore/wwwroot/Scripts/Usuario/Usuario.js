let table = '';
let contartr = 0;

window.onload = function () {
    ConsultaServidor();
    CargarPerfiles();
    TriggerTooltipB5();
};

function ConsultaServidor() {
    table = $("#table_id").DataTable({
        ajax: {
            url: 'ObtenerUsuarios',
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
                data: "NombreUsuario",
                render: function (data) {
                    return data.toUpperCase();
                }
            },
            {
                data: "Usuario",
                render: function (data) {
                    return data.toUpperCase();
                }
            },
            {
                data: "NombrePerfil",
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
                data: "IdUsuario",
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


function ModalNuevo() {
    $("#lblTituloModal").html("Nuevo Usuario");
    $("#modal-form").modal("show")

}
function CargarPerfiles() {

    $.ajax({
        url: "/Perfil/ObtenerPerfiles",
        type: "GET",
        data: {'MostrarInactivos':false},
        success: function (data) {
            llenarComboPerfil(data, "cboPerfil", "Seleccione");
        },
        error: function (jqXHR, textStatus, errorThrown) {
            if (jqXHR.status === 401) {
                manejarTokenExpirado(function () { CargarPerfiles() });
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

function llenarComboPerfil(lista, idCombo, primerItem) {
    var contenido = "";
    if (primerItem != null) contenido = "<option value=''>" + primerItem + "</option>";
    var nRegistros = lista.length;
    var nCampos;
    var campos;
    for (var i = 0; i < nRegistros; i++) {

        if (lista.length > 0) { contenido += "<option value='" + lista[i].IdPerfil + "'>" + lista[i].Descripcion.toUpperCase() + "</option>"; }
        else { }
    }
    var cbo = document.getElementById(idCombo);
    if (cbo != null) cbo.innerHTML = contenido;
}

function GuardarUsuario() {

    let varIdUsuario = $("#txtId").val() || 0;
    let varNombre = $("#txtNombre").val();
    let varUsuario = $("#txtUsuario").val();
    let varContraseña = $("#txtContraseña").val();
    let varPerfil = $("#cboPerfil").val() || 0;
    let varCorreo = $("#txtCorreo").val();
    let varEstado = $('#chkActivo').is(':checked');

    const campos = [
        { id : "#txtNombre", mensaje: "Complete el campo Nombre" },
        { id : "#txtUsuario", mensaje: "Complete el campo Usuario" },
        { id : "#cboPerfil", mensaje: "Seleccione un Perfil", adicional: v => v !== "0" },
        { id : "#txtCorreo", mensaje: "Complete el campo Correo" },
    ];

    if (varIdUsuario == 0) {
        campos.push({ id : "#txtContraseña", mensaje: "Complete el campo Contraseña" },)
    }

    let hayErrores = false;

    campos.forEach(c => {
        $(c.id).removeClass("is-invalid");
        $(`#error-${c.id.replace("#", "")}`).addClass("d-none").text("");
    });

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

    if (hayErrores) {
        return;
    }

    $("#btnGrabar").text("Guardando...")
    $("#btnGrabar").prop("disabled",true)

    $.ajax({
        url: 'UpdateInsertUsuario',
        type: 'POST',
        contentType: "application/json; charset=utf-8",
        data:JSON.stringify({
                IdUsuario: varIdUsuario,
                NombreUsuario: varNombre,
                Usuario: varUsuario,
                Password: varContraseña,
                IdPerfil: varPerfil,
                Correo: varCorreo,
                Estado: varEstado
            }),
        success: function (data, status) {
            
                Swal.fire("Éxito!", "Proceso Realizado Correctamente", "success");
                table.ajax.reload(null, false);
                CerrarModal()
          
        },
        error: function (jqXHR, textStatus, errorThrown) {
            if (jqXHR.status === 401) {
                manejarTokenExpirado(function () { GuardarUsuario() });
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

function ObtenerDatosxID(varIdUsuario) {
    $("#lblTituloModal").html("Editar Usuario");
    $("#txtContraseña").prop("disabled", true)
    $("#modal-form").modal("show")
    $.ajax({
        url: 'ObtenerDatosxID',
        type: 'GET',
        data: {
            'IdUsuario': varIdUsuario
        },
        success: function (data) {

                let usuarios = (data);

                $("#txtId").val(usuarios.IdUsuario);
                $("#txtNombre").val(usuarios.NombreUsuario);
                $("#txtUsuario").val(usuarios.Usuario);
                $("#txtCorreo").val(usuarios.Correo);
                $("#cboPerfil").val(usuarios.IdPerfil);
                $("#chkActivo").prop('checked', usuarios.Estado || false);

            
        },
        error: function (jqXHR, textStatus, errorThrown) {
            if (jqXHR.status === 401) {
                manejarTokenExpirado(function () { ObtenerDatosxID(varIdUsuario) });
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

function CerrarModal() {
    $("#modal-form").modal("hide")
    limpiarDatos();
}

function limpiarDatos() {
    $("#txtId").val("");
    $("#txtNombre").val("");
    $("#txtUsuario").val("");
    $("#txtContraseña").val("");
    $("#txtUsuarioSap").val("");
    $("#cboPerfil").val("0");
    $("#txtContrasenaSap").val("");
    $("#txtCorreo").val("");
    $("#cboProveedor").val("");
    $("#chkActivo").prop('checked', true);
    $("#txtContraseña").prop("disabled", false)

    $(".smcImputValidacion").removeClass("is-invalid")
    $(".smcLabelValidacion").addClass("d-none")

}