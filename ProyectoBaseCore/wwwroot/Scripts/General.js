
let GDecimalesCantidades;
let GDecimalesImportes;
let GDecimalesPrecios;
let GDecimalesPorcentajes;
let token = localStorage.getItem("token");

function eventDefault(event) {
    event.preventDefault();
}


let lenguaje = {
    "language": {
        "decimal": ",",
        "thousands": ".",
        "info": "Mostrando registros del _START_ al _END_ de un total de _TOTAL_ registros",
        "infoEmpty": "Mostrando registros del 0 al 0 de un total de 0 registros",
        "infoPostFix": "",
        "infoFiltered": "(filtrado de un total de _MAX_ registros)",
        "loadingRecords": "Cargando...",
        "lengthMenu": "Mostrar _MENU_ registros",
        "paginate": {
            "first": "Primero",
            "last": "Último",
            "next": "Siguiente",
            "previous": "Anterior"
        },
        "processing": "Procesando...",
        "search": "Buscar:",
        "searchPlaceholder": "",
        "zeroRecords": "No se encontraron resultados",
        "emptyTable": "Ningún dato disponible en esta tabla",
        "aria": {
            "sortAscending": ": Activar para ordenar la columna de manera ascendente",
            "sortDescending": ": Activar para ordenar la columna de manera descendente"
        },

    },
    scrollX: true,
    //responsive: true

};

let lenguaje_data = {
    "decimal": ",",
    "thousands": ".",
    "info": "Mostrando registros del _START_ al _END_ de un total de _TOTAL_ registros",
    "infoEmpty": "Mostrando registros del 0 al 0 de un total de 0 registros",
    "infoPostFix": "",
    "infoFiltered": "(filtrado de un total de _MAX_ registros)",
    "loadingRecords": "Cargando...",
    "lengthMenu": "Mostrar _MENU_ registros",
    "paginate": {
        "first": "Primero",
        "last": "Último",
        "next": "Siguiente",
        "previous": "Anterior"
    },
    "processing": "Procesando...",
    "search": "Buscar:",
    "searchPlaceholder": "",
    "zeroRecords": "No se encontraron resultados",
    "emptyTable": "Ningún dato disponible en esta tabla",
    "aria": {
        "sortAscending": ": Activar para ordenar la columna de manera ascendente",
        "sortDescending": ": Activar para ordenar la columna de manera descendente"
    },


};


function AbrirModal(idModal) {

    $("#" + idModal).modal('show');

    //$.magnificPopup.open({
    //    //removalDelay: 100,
    //    items: {
    //        src: $('#' + idModal)
    //    },
    //    callbacks: {
    //        beforeOpen: function (e) {
    //            var Animation = 'mfp-slideDown';
    //            this.st.mainClass = Animation;

    //        }
    //    },
    //    midClick: true,
    //});
    //setTimeout(function () {
    //    var elems = document.getElementsByClassName('input-sm');
    //    for (var i = elems.length; i--;) {
    //        var tidx2 = elems[i].getAttribute('tabindex');
    //        if (tidx2 == 1) elems[i].focus();
    //    }
    //    $(".mfp-wrap").eq(0).removeAttr("tabindex");
    //}, 100);
}

function closePopup(idModal) {
    $("#" + idModal).modal('hide');
    limpiarDatos()
    //console.log("hola11");
    //if (scrollHeight > 0) {
    //    setTimeout(function () {
    //        var html = document.getElementsByTagName("HTML")[0];
    //        html.style.height = scrollHeight + "px";
    //    }, 500);
    //}
}

function formatNumberDecimales(numero, decimales) {
    try {
        numero = parseFloat(numero)

        if (isNaN(numero)) {
            return '-'
        }
        // Redondear el número a la cantidad deseada de decimales
        const numeroRedondeado = Number(numero.toFixed(decimales));

        // Convertir el número redondeado en una cadena con comas para la parte entera
        const partes = numeroRedondeado.toString().split('.');
        partes[0] = partes[0].replace(/\B(?=(\d{3})+(?!\d))/g, ',');

        // Agrega Ceros si no hay decimales
        if (partes.length === 1) {
            return partes[0] + '.' + '0'.repeat(decimales);
        }

        // Unir las partes con el punto decimal y agrega los ceros necesarios para completar la cantidad de decimales
        return partes[0] + '.' + partes[1].padEnd(decimales, '0');
    }
    catch (e) {
        return '-'
    }



    

}




function quitarFormato(number) {
    return Number(StringReplace(number, ',', ''));
}


function SinFormato(number) {
    //alert(number);
    return number.replace(",", "");
}


function StringReplace(text, oldText, newText, flags) {
    var r = text;
    if (oldText instanceof Array) for (var i = 0; i < oldText.length; i++) r = r.replace(new RegExp(oldText[i], flags || 'g'), newText);
    else r = r.replace(new RegExp(oldText, flags || 'g'), newText);
    return r;
}
function FormatMiles(num) {
    var a = num.toString().split('.');
    a[0] = a[0].replace(/\B(?=(\d{3})+(?!\d))/g, ',');
    return a.join(".");
}
function KeyPressNumber(control) {
    var a = control;

    a.keydown(function (e) {
        var p = +e.which,
            q = (p > 47 && p < 58)
                || (p > 95 && p < 106)
                || (p > 36 && p < 41)
                || ((p == 110 || p == 190) && a.val().toString().indexOf('.') == -1)
                || p == 8 || p == 46;

        if (!q) e.preventDefault();
    });

    a.keyup(function (e) {

        var p = StringReplace(a.val(), ',', '');
        p = FormatMiles(p)

        a.val(p);
        //CalcularTotales();
    });
}

function AlertConfirm() {
    return new Promise(function (resolve, reject) {
        var alert = Swal.fire({
            title: "Confirmar?",
            text: "¿Desea eliminar este item?",
            icon: "question",
            showCancelButton: true,
            confirmButtonColor: "#3085d6",
            cancelButtonColor: "#ccc",
            confirmButtonText: "Si, Continuar",
            cancelButtonText: "No, Cancelar"
        });

        resolve(alert);
    });
}


function TriggerTooltipB5() {
      $('body').tooltip({
            selector: '[data-bs-toggle="tooltip"]',
            container: 'body',
            trigger: 'hover'
      });

    var tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'))
    tooltipTriggerList.forEach(function (tooltipTriggerEl) {
            tooltipTriggerEl.addEventListener('click', function (event) {
                var tooltipInstance = bootstrap.Tooltip.getInstance(tooltipTriggerEl);
                if (tooltipInstance) {
                    tooltipInstance.hide(); // cerrar al hacer click
                }
            });
    });

  
}


function autocompleteInput(inp, arr, callback) {
    var currentFocus;

    inp.addEventListener("input", function (e) {
        var a, b, i, val = this.value;
        closeAllLists();
        if (!val) { return false; }

        callback(val).then(function (result) {
            arr = result;

            currentFocus = -1;
            a = document.createElement("DIV");
            a.setAttribute("id", inp.id + "autocomplete-list");
            a.setAttribute("class", "autocomplete-items");
            inp.parentNode.appendChild(a);

            var regex = new RegExp(val, 'gi');

            for (i = 0; i < arr.length; i++) {
                if (regex.test(arr[i])) {
                    b = document.createElement("DIV");

                    var highlightedText = arr[i].replace(regex, function (match) {
                        return "<strong>" + match + "</strong>";
                    });

                    b.innerHTML = highlightedText;
                    b.innerHTML += "<input type='hidden' value='" + arr[i] + "'>";

                    b.addEventListener("click", function (e) {
                        inp.value = this.getElementsByTagName("input")[0].value;
                        closeAllLists();
                    });

                    a.appendChild(b);
                }
            }
        });
    });

    inp.addEventListener("keydown", function (e) {
        var x = document.getElementById(this.id + "autocomplete-list");
        if (x) x = x.getElementsByTagName("div");
        if (e.keyCode == 40) {
            currentFocus++;
            addActive(x);
        } else if (e.keyCode == 38) {
            currentFocus--;
            addActive(x);
        } else if (e.keyCode == 13) {
            e.preventDefault();
            if (currentFocus > -1) {
                if (x) x[currentFocus].click();
            }
        }
    });

    function addActive(x) {
        if (!x) return false;
        removeActive(x);
        if (currentFocus >= x.length) currentFocus = 0;
        if (currentFocus < 0) currentFocus = (x.length - 1);
        x[currentFocus].classList.add("autocomplete-active");
    }

    function removeActive(x) {
        for (var i = 0; i < x.length; i++) {
            x[i].classList.remove("autocomplete-active");
        }
    }

    function closeAllLists(elmnt) {
        var x = document.getElementsByClassName("autocomplete-items");
        for (var i = 0; i < x.length; i++) {
            if (elmnt != x[i] && elmnt != inp) {
                x[i].parentNode.removeChild(x[i]);
            }
        }
    }

    document.addEventListener("click", function (e) {
        closeAllLists(e.target);
    });
}



function fetchSegura(url, opciones = {}) {
    return fetch(url, opciones)
        .then(response => {
            if (response.status === 401) {
                manejarTokenExpirado();
                throw new Error("Token expirado");
            }

            const contentType = response.headers.get("content-type");

            if (contentType && contentType.includes("application/json")) {
                return response.json();
            } else {
                return response.text();
            }
        });
}
function manejarTokenExpirado(onSuccessCallback) {

    $.ajax({
        url: '/Home/PostRefreshToken',
        type: 'POST',
        success: function (response) {
            if (typeof onSuccessCallback === "function") {
                // Reintenta la llamada original
                onSuccessCallback();
            }

        },
        error: function (jqXHR) {
            Swal.fire({
                title: 'La sesión ha expirado',
                text: 'Tu sesión ha finalizado. Serás redirigido al login.',
                icon: 'warning',
                showConfirmButton: false,
                allowOutsideClick: false,
                allowEscapeKey: false,
                timer: 4000,
                timerProgressBar: true,
                didClose: () => {
                    window.location.href = '/';
                }
            });

            // Log para depuración
            console.error("Error al refrescar token");
            console.error("Código de estado:", jqXHR.status);
            console.error("Respuesta del servidor:", jqXHR.responseText);
        }
    });
}
