let table = '';


window.onload = function () {
    ConsultaServidor();
    CargarMenus()
};


function ConsultaServidor() {
    table = $("#table_id").DataTable({
        ajax: {
            url: 'ObtenerPerfiles', 
            type: 'GET',
            dataSrc: '',
            data:  { MostrarInactivos:true },
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
                data: "Descripcion",
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
                data: "IdPerfil",
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
    $("#lblTituloModal").html("Nuevo Perfil");
    AbrirModal("modal-form");
}

function cerrarModal() {
    $("#modal-form").modal('hide')
    limpiarDatos()
}


function GuardarPerfil() {

    let varIdPerfil = $("#txtId").val() || 0;
    let varPerfil = $("#txtPerfil").val();
    let varMenu = $("#cboMenu").val() || 0;
    let varEstado = false;

    if ($('#chkActivo')[0].checked) {
        varEstado = true;
    }


    const campos = [
        { id: "#txtPerfil", mensaje: "Complete el campo Perfil" },
        { id: "#cboMenu", mensaje: "Seleccione un Menu Inicial", adicional: v => v !== "0" },
    ];

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
    $("#btnGrabar").prop("disabled", true)

    $.ajax({
        url: "UpdateInsertPerfil",
        type: "POST",
        contentType: "application/json; charset=utf-8",
        data: 
            JSON.stringify({
                IdPerfil: varIdPerfil,
                Descripcion: varPerfil,
                Estado: varEstado,
                IdMenuInicial: varMenu
            }),
        success: function (data) {

            Swal.fire("Éxito!", "Proceso Realizado Correctamente", "success");
            table.ajax.reload(null, false);
            cerrarModal()
        },
        error: function (jqXHR, textStatus, errorThrown) {
            if (jqXHR.status === 401) {
                manejarTokenExpirado(function () { GuardarPerfil() });
            } else {
                try {

                    var err = JSON.parse(jqXHR.responseText);
                    console.log(err)
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

function ObtenerDatosxID(varIdPerfil) {
    $("#lblTituloModal").html("Editar Perfil");
    AbrirModal("modal-form");

    $.ajax({
        url: 'ObtenerDatosxID',
        type: 'GET',
        data: {
            IdPerfil: varIdPerfil
        },
        success: function (data, status) {
           
                let perfiles = (data);
                $("#txtId").val(perfiles.IdPerfil);
                $("#txtPerfil").val(perfiles.Descripcion);
            $("#cboMenu").val(perfiles.IdMenuInicial);
                if (perfiles.Estado) {
                    $("#chkActivo").prop('checked', true);
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


function limpiarDatos() {
    $("#txtId").val("");
    $("#txtPerfil").val("");
    $("#cboMenu").val("0");
    $("#chkActivo").prop('checked', true);
    $(".smcImputValidacion").removeClass("is-invalid")
    $(".smcLabelValidacion").addClass("d-none")
}


function CargarMenus() {

    $.ajax({
        url: "/Menu/ObtenerMenus",
        type: "GET",
        data: { 'MostrarInactivos': false },
        success: function (data) {
            let perfiles = (data);
            llenarComboMenu(perfiles, "cboMenu", "Seleccione");
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

function llenarComboMenu(lista, idCombo, primerItem) {
    var contenido = "";
    if (primerItem != null) contenido = "<option value='0'>" + primerItem + "</option>";
    var nRegistros = lista.length;
    var nCampos;
    var campos;
    for (var i = 0; i < nRegistros; i++) {
        console.log(lista[i])
        if (lista.length > 0) { contenido += "<option value='" + lista[i].IdMenu + "'>" + lista[i].MenuPadre.toUpperCase() + "-" + lista[i].Descripcion.toUpperCase() + "</option>"; }
        else { }
    }
    var cbo = document.getElementById(idCombo);
    if (cbo != null) cbo.innerHTML = contenido;
}
