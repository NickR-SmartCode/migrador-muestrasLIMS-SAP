let table = '';


window.onload = function () {
    ConsultaServidor();
    CargarMenus()
};


function ConsultaServidor() {

    $.ajax({
        url: "ObtenerConfiguracion",
        type: "GET",
        data: {'MostrarInactivos':true},
        success: function (data, status) {

            let datos = (data);

            $("#txtRuc").val(datos.RUC);
            $("#txtRazonSocial").val(datos.RazonSocial);
            $("#txtServerSMTP").val(datos.ServidorSMTP);
            $("#txtPortSMTP").val(datos.PuertoSMTP);
            $("#txtEmailSMTP").val(datos.EmailSMTP );
            $("#txtPasswordSMTP").val(datos.PasswordSMTP);
            if (datos.SSLSMTP) {
                $("#cboSSLSMTP").val(1)
            } else {
                $("#cboSSLSMTP").val(0)
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

function Guardar() {

    let RUC = $("#txtRuc").val();
    let RazonSocial = $("#txtRazonSocial").val();
    let ServidorSMTP = $("#txtServerSMTP").val();
    let PuertoSMTP = $("#txtPortSMTP").val();
    let SSLSMTP = $("#cboSSLSMTP").val() == 1 ? true : false;
    let EmailSMTP = $("#txtEmailSMTP").val();
    let PasswordSMTP = $("#txtPasswordSMTP").val();


    const campos = [
        { id: "#txtRuc", mensaje: "Complete el campo RUC" },
        { id: "#txtRazonSocial", mensaje: "Complete el campo Razón Social" },
        { id: "#txtServerSMTP", mensaje: "Complete el campo Servidor SMTP" },
        { id: "#txtPortSMTP", mensaje: "Complete el campo Puerto SMTP" },
        { id: "#cboSSLSMTP", mensaje: "Complete el campo SSL SMTP" },
        { id: "#txtEmailSMTP", mensaje: "Complete el campo Email SMTP" },
        { id: "#txtPasswordSMTP", mensaje: "Complete el campo Contraseña SMTP" },
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


    $("#btnSave").prop("disabled",true)
    $("#btnSave").html(`<i class="fa-solid fa-floppy-disk"></i>  Guardando...`)

    $.ajax({
        url: "UpdateConfiguracion",
        contentType: "application/json; charset=utf-8",
        type: "POST",
        data: JSON.stringify({
            RUC, 
            RazonSocial,
            ServidorSMTP,
            PuertoSMTP,
            SSLSMTP,
            EmailSMTP,
            PasswordSMTP,
        }),
        success: function (data) {

                Swal.fire("Éxito!", "Proceso Realizado Correctamente", "success");
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
        },complete: function () {
            $("#btnSave").prop("disabled", false)
            $("#btnSave").html(`<i class="fa-solid fa-floppy-disk"></i>  Guardar`)
            $(".smcImputValidacion").removeClass("is-invalid")
            $(".smcLabelValidacion").addClass("d-none")
        }
    });

}

function EnviarTestEmail() {

    $("#btnTestEmail").prop("disabled", true)
    $("#btnTestEmail").html(`<i class="fa-solid fa-envelope"></i>  Enviando...`)

    $.ajax({
        url: "EnviarEmailPrueba",
        type: "POST",     
        success: function (data) {

            Swal.fire("Éxito!", "Proceso Realizado Correctamente", "success");
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
            $("#btnTestEmail").prop("disabled", false)
            $("#btnTestEmail").html(`<i class="fa-solid fa-envelope"></i>   Enviar Email de Prueba`)
        }
    });


}