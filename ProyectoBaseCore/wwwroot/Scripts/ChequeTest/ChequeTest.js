let table = '';
let contartr = 0;

window.onload = function () {
    ConsultaServidor();
    TriggerTooltipB5();
};


function ConsultaServidor() {

    table = $("#table_id").DataTable({
        ajax: {
            url: 'ObtenerCheques',
            type: 'GET',
            dataSrc: '',
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
                data: "Nombre",
                render: function (data) {
                    return data.toUpperCase();
                }
            },
            {
                data: "Fecha",
                render: function (data) {
                    return new Date(parseInt(data.match(/\d+/)[0])).toLocaleDateString("es-PE");
                }
            },
            {
                data: "Moneda",
                render: function (data) {
                    return data.toUpperCase();
                }
            },
            {
                data: "Monto",
                render: function (data) {
                    return data.toLocaleString("en-US", { minimumFractionDigits: 2, maximumFractionDigits: 2 });
                }
            },
            {
                data: "EstadoAprobacion",
                render: function (data) {
                    if (data == 1) {
                        return '<span class="badge bg-success bg-opacity-10 text-success border border-success rounded-pill">APROBADO</span>';
                    } else if (data == 2) {
                        return '<span class="badge bg-danger bg-opacity-10 text-danger border border-danger rounded-pill">RECHAZADO</span>';
                    } else {

                    }
                    return '<span class="badge bg-primary bg-opacity-10 text-white border border-primary rounded-pill">PENDIENTE</span>';
                }
            },
            {
                data: "IdCheque",
                render: function (data) {
                    return '<button class="btn btn-xs btn-primary fa-solid fa-list-check" onclick="VerAprobaciones(1,' + data +')"></button>';
                }
            },
            {
                data: "IdCheque",
                render: function (data) {
                    return '<button class="btn btn-xs btn-primary fa-solid fa-envelope" onclick="EnviarCorreoAprobacion(1,' + data +')"></button>';
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
    $("#lblTituloModal").html("Nuevo Cheque");
    $("#modal-form").modal("show")

}

function Guardar() {

    let IdCheque = $("#txtId").val() || "0";
    let Nombre = $("#txtNombre").val();
    let Fecha = $("#txtFecha").val();
    let Moneda = $("#cboMoneda").val();
    let Monto = $("#txtMonto").val();

    const campos = [
        { id : "#txtNombre", mensaje: "Complete el campo Nombre" },
        { id: "#txtFecha", mensaje: "Complete el campo Fecha" },
        { id: "#cboMoneda", mensaje: "Seleccione una Moneda", adicional: v => v !== "0" },
        { id: "#txtMonto", mensaje: "Complete el campo Monto", adicional: v => v > "0" },
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
    $("#btnGrabar").prop("disabled",true)

    $.ajax({
        url: 'UpdateInsertCheque',
        type: 'POST',
        contentType: "application/json; charset=utf-8",
        data: JSON.stringify({
            IdCheque, 
            Nombre, 
            Fecha, 
            Moneda, 
            Monto, 
        }),
        success: function (data, status) {
            
            Swal.fire("Éxito!", "Proceso Realizado Correctamente", "success");
            table.ajax.reload(null, false);
            $("#modal-form").modal("hide");
            limpiarDatos();
          
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
            if (!data.success) {
                Swal.fire("Error!",data.message, "error");
                limpiarDatos();
            } else {
                let usuarios = (data.data);

                $("#txtId").val(usuarios.IdUsuario);
                $("#txtNombre").val(usuarios.NombreUsuario);
                $("#txtUsuario").val(usuarios.Usuario);
                $("#txtCorreo").val(usuarios.Correo);
                $("#cboPerfil").val(usuarios.IdPerfil);
                $("#chkActivo").prop('checked', usuarios.Estado || false);

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
    $("#txtContrasenaSap").val("");
    $("#txtCorreo").val("");
    $("#cboProveedor").val("");
    $("#chkActivo").prop('checked', true);
    $("#txtContraseña").prop("disabled", false)

    $(".smcImputValidacion").removeClass("is-invalid")
    $(".smcLabelValidacion").addClass("d-none")

}


function VerAprobaciones(IdModulo,IdTablaOriginal) {
    $("#modal-aprobaciones").modal("show")
    $.ajax({
        url: "/Aprobacion/ObtenerAprobaciones",
        data: {IdModulo,IdTablaOriginal},
        method: "GET",
        success: function (data, status) {

            let registros = (data.data); 
            let tr = '';
            for (let i = 0; i < registros.length; i++) {

                let LabelEstado = '<span class="badge bg-primary bg-opacity-10 text-primary border border-primary rounded-pill">PENDIENTE</span>'
                if (registros[i].EstadoAprobacion == 2) {
                    LabelEstado = '<span class="badge bg-danger bg-opacity-10 text-danger border border-danger rounded-pill">RECHAZADO</span>'
                }
                if (registros[i].EstadoAprobacion == 1) {
                    LabelEstado = '<span class="badge bg-success bg-opacity-10 text-success border border-success rounded-pill">APROBADO</span>'
                }


                tr += '<tr>' +
                    '<td>' + (i + 1) + '</td>' +
                    '<td>' + registros[i].NombreEtapa + '</td>' +
                    '<td>' + registros[i].NombreUsuario + '</td>' +
                    '<td>' + LabelEstado + '</td>' +
                    '<td>' + registros[i].Comentario + '</td>' +
                    '<td>' + registros[i].FechaAprobacion + '</td>' +              
                    '</tr>';
            }

            $("#tbody_aprobaciones").html(tr);
            
        }, error: function (jqXHR) {
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
function CerrarModalAprobar() {
    $("#modal-aprobaciones").modal("hide")
    $("#tbody_aprobaciones").html("");
}

function EnviarCorreoAprobacion(IdModulo, IdTablaOriginal) {

    Swal.fire({
        title: 'Procesando...',
        text: 'Por favor espere',
        allowOutsideClick: false,
        allowEscapeKey: false,
        didOpen: () => {
            Swal.showLoading();
        }
    });

    setTimeout(() => {
        $.ajax({
            url: '/Aprobacion/ReenviarCorreoAprobacion',
            type: 'POST',
            data: {
                IdModulo,
                IdTablaOriginal
            },
            success: function (data, status) {

                Swal.fire("Éxito!", "Proceso Realizado Correctamente", "success");
                $("#modal-form").modal("hide");

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
    }, 200)

    
}