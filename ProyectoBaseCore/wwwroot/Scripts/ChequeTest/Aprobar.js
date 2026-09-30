let table = '';
let contartr = 0;

window.onload = function () {
    ConsultaServidor();
    TriggerTooltipB5();
};


function ConsultaServidor() {
    if (table) {
        table.destroy()
    }
    $.ajax({
        url: "ObtenerChequesParaAprobacion",
        method: "GET",
        success: function (data, status) { 
            
            let usuarios = (data.data);
            let total_usuarios = usuarios.length;
            let tr = '';
            for (let i = 0; i < usuarios.length; i++) {
                const fecha = new Date(parseInt(usuarios[i].Fecha.match(/\d+/)[0]));

                tr += '<tr>' +
                    '<td>' + (i + 1) + '</td>' +
                    '<td>' + usuarios[i].Nombre + '</td>' +
                    '<td>' + fecha.toLocaleDateString("es-PE") + '</td>' +
                    '<td>' + usuarios[i].Moneda.toUpperCase() + '</td>' +
                    '<td>' + usuarios[i].Monto.toLocaleString("en-US", { minimumFractionDigits: 2, maximumFractionDigits: 2 }) + '</td>' +
                    '<td><textarea class="form-control form-control-modern" id="txtComentario' + usuarios[i].IdModuloAprobacionModelo+'" ></textarea></td>' +
                    '<td>' +
                        '<button class="btn btn-success" onclick="Aprobar(' + usuarios[i].IdModuloAprobacionModelo + ',1)">APROBAR</button> ' +
                        '<button class="btn btn-danger" onclick="Aprobar(' + usuarios[i].IdModuloAprobacionModelo + ',2)">RECHAZAR</button>' +
                    '</td>' +
                    '</tr>';
            }

            $("#tbody_Usuarios").html(tr);
            $("#spnTotalRegistros").html(total_usuarios);

            table = $("#table_id").DataTable({
                language: lenguaje,
                initComplete: function () {
                    TriggerTooltipB5();
                }
            });
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
function Aprobar(IdModuloAprobacionModelo, Estado) {

    let Comentario = $("#txtComentario" + IdModuloAprobacionModelo).val();

    if (Estado == 1) {
        varEstado = "Aprobar";
        varEstadoCorreo = "REGISTRADOS";
    } else {
        varEstado = "Desaprobar";
        varEstadoCorreo = "OBSERVADOS";
    }

    if (Comentario == null || Comentario == undefined) {
        Comentario = "";
    }

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
                url: "/Aprobacion/AprobarRegistro",
                type: "POST",
                async: true,
                data: {
                    "IdModuloAprobacionModelo": IdModuloAprobacionModelo, "Estado": Estado, "Comentario": Comentario
                },
                beforeSend: function () {

                    Swal.fire({
                        title: "Cargando...",
                        text: "Por favor espere",
                        showConfirmButton: false,
                        allowOutsideClick: false
                    });
                },
                success: function (data) {


                    
                        Swal.fire('Correcto', 'Proceso Realizado Correctamente', 'success')
                        ConsultaServidor("ObtenerAprobaciones");
             

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

            }).fail(function () {
                Swal.fire(
                    'Error!',
                    'Comunicarse con el Area Soporte: smarcode@smartcode.pe !',
                    'error'
                )
            });

        }
    })

}
