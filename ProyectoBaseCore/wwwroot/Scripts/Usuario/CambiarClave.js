window.onload = function () {

};

function MostrarClaves() {

    if ($("#chkVerClave").prop("checked")) {
        $(".clave").each(function (indice, elemento) {
            $(elemento).attr('type', 'text');
        });

    } else {
        $(".clave").each(function (indice, elemento) {
            $(elemento).attr('type', 'password');
        });
    }

}

function CambiarClave() {
    let ClaveActual = $("#txtClaveActual").val()
    let ClaveNueva = $("#txtClaveNueva").val()
    let ClaveNueva2 = $("#txtClaveNueva2").val()

    if (ClaveActual == "" || ClaveNueva == "" || ClaveNueva2 == "") {
        Swal.fire("Advertencia", "Complete todos los campos", "info");
        return;
    }

    if (ClaveNueva != ClaveNueva2) {
        Swal.fire("Error", "La Nueva Clave y la Confirmación no coinciden", "error");
        return;
    }

    $.post("CambiarPassword", { ClaveActual, ClaveNueva }, function (data, status) {
        let datos = (data);

        if (datos.success) {
            Swal.fire("Exito", "Clave Cambiada Correctamente", "success")
        } else {
            Swal.fire("Error", datos.message, "error")
        }

    });

    $.ajax({
        url: "CambiarPassword",
        type: "POST",
        data: {
            ClaveActual, ClaveNueva
        },
        success: function (data) {
           
                Swal.fire("Éxito!", "Proceso Realizado Correctamente", "success");
                table.destroy();
                ConsultaServidor("ObtenerPerfiles");
                limpiarDatos();     
        },
        error: function (jqXHR, textStatus, errorThrown) {
            if (jqXHR.status === 401) {
                manejarTokenExpirado();
            } else {
                try {
                    var err = JSON.parse(jqXHR.responseText);
                    Swal.fire("Error", "Error " + jqXHR.status +": " +err.message, "error")
                } catch (e) {
                    Swal.fire("Error", "Error " + jqXHR.status + ": " +jqXHR.responseText, "error")
                }
            }
        }
    });

}