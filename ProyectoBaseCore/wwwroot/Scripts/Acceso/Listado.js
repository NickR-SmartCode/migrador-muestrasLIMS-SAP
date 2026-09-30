var tipoUsuarioLogin;
const C_ROL_PRINCIPAL = 6;

window.onload = function () {
    CargarPerfiles();
    CargarMenu();
    //var url = "obtenerListaMenurol";
    //enviarServidor(url, "get", mostrarLista, null);
    //configurarBoton();
    ////pl();
}

function CargarPerfiles() {

    $.ajax({
        url: "/Perfil/ObtenerPerfiles",
        type: "GET",
        data: { 'MostrarInactivos': false },
        success: function (data) {
            let perfiles = (data);
            console.log(perfiles);
            llenarComboPerfil(perfiles, "cboRol", "Seleccione");
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

function CargarMenu() {

    $.ajax({
        url: "/Menu/ObtenerMenuCompleto",
        type: "GET",
        success: function (data) {
            const menuJerarquico = construirMenuJerarquico(data);
            renderMenuTabla(menuJerarquico, $("#tbRol"));

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

// Construir jerarquía
function construirMenuJerarquico(data) {
    const map = {};
    const raiz = [];

    data.forEach(item => {
        map[item.IdMenu] = { ...item, itemsSubMenu: [] };
    });

    data.forEach(item => {
        const parentId = item.IdMenuPadre;
        if (parentId && parentId !== item.IdMenu && map[parentId]) {
            map[parentId].itemsSubMenu.push(map[item.IdMenu]);
            map[item.IdMenu].EsSubMenu = true;
        } else {
            raiz.push(map[item.IdMenu]);
        }
    });

    return raiz;
}

// Renderizar menú en tabla con checkbox, desplegable y flechas
function renderMenuTabla(menu, container, nivel = 0, parentId = null) {
    menu.forEach(item => {
        const tr = $("<tr></tr>")
            .attr("data-id", item.IdMenu)
            .attr("data-parent", parentId)
            .attr("data-nivel", nivel);

        // Columna descripción con flecha
        const tdDescripcion = $("<td></td>")
            .css("padding-left", (nivel * 20) + "px")
            .css("cursor", item.itemsSubMenu.length ? "pointer" : "default");

        if (item.itemsSubMenu && item.itemsSubMenu.length > 0) {
            const flecha = $("<span>▸ </span>").css("font-weight", "bold");
            tdDescripcion.append(flecha).append(item.Descripcion);
            // Click para desplegar/ocultar hijos
            tdDescripcion.click(function (e) {
                e.stopPropagation();
                toggleHijos(item.IdMenu, flecha);
            });
        } else {
            tdDescripcion.text(item.Descripcion);
        }

        // Columna checkbox
        const tdCheckbox = $("<td></td>");
        const checkbox = $("<input type='checkbox' idmenu=" + item.IdMenu +" />");
        tdCheckbox.append(checkbox);

        tr.append(tdDescripcion, tdCheckbox);
        container.append(tr);

        item.checkbox = checkbox; // Guardar referencia

        // Si tiene hijos, renderizar recursivamente y ocultarlos
        if (item.itemsSubMenu && item.itemsSubMenu.length > 0) {
            renderMenuTabla(item.itemsSubMenu, container, nivel + 1, item.IdMenu);
            item.itemsSubMenu.forEach(child => {
                $(`tr[data-id=${child.IdMenu}]`).hide();
            });
        }

        // Checkbox: marcar hijos y actualizar padres
        checkbox.change(function () {
            marcarHijos(item, $(this).prop("checked"));
            actualizarPadres($(this));
        });
    });
}

// Mostrar/ocultar hijos recursivamente con flecha
function toggleHijos(parentId, flecha) {
    $(`tr[data-parent=${parentId}]`).each(function () {
        const childId = $(this).data("id");
        if ($(this).is(":visible")) {
            $(this).slideUp();
            // Ocultar también los sub-hijos
            toggleHijos(childId, null);
        } else {
            $(this).slideDown();
        }
    });
    // Cambiar flecha
    if (flecha) {
        flecha.text(flecha.text() === "▸ " ? "▾ " : "▸ ");
    }
}

// Marcar/desmarcar todos los hijos
function marcarHijos(item, estado) {
    if (item.itemsSubMenu && item.itemsSubMenu.length > 0) {
        item.itemsSubMenu.forEach(child => {
            child.checkbox.prop("checked", estado);
            marcarHijos(child, estado);
        });
    }
}

// Actualizar padres según hijos
function actualizarPadres(checkbox) {
    const tr = checkbox.closest("tr");
    let nivel = parseInt(tr.attr("data-nivel"));
    if (nivel === 0) return; // Es raíz, no hay padre

    let currentTr = tr.prevAll("tr");
    while (currentTr.length > 0) {
        const currentNivel = parseInt(currentTr.attr("data-nivel"));
        if (currentNivel === nivel - 1) {
            const parentCheckbox = currentTr.find("input[type=checkbox]");

            // Solo marcar el padre si al menos un hijo está marcado
            const siblings = [];
            const parentLevel = currentNivel;
            currentTr.nextAll("tr").each(function () {
                const siblingLevel = parseInt($(this).attr("data-nivel"));
                if (siblingLevel === parentLevel + 1) siblings.push($(this).find("input[type=checkbox]"));
                if (siblingLevel <= parentLevel) return false;
            });

            const anyChecked = siblings.some(cb => cb.prop("checked"));
            if (anyChecked) parentCheckbox.prop("checked", true); // Solo marcar, no desmarcar

            checkbox = parentCheckbox;
            nivel = parentLevel;
        }
        currentTr = currentTr.prevAll("tr");
    }
}

function CargarAccesos() {

    $('input[type="checkbox"]').each(function () {
        // Obtiene el atributo data-id del tr padre
        $(this).prop("checked",false)
    });

    let IdPerfil = $("#cboRol").val();

    $.ajax({
        url: "ObtenerAccesos",
        type: "GET",
        data: { IdPerfil },
        success: function (data) {
            let Accesos = (data);
            for (var i = 0; i < Accesos.length; i++) {
        
                    $('input[type="checkbox"][idmenu="' + Accesos[i].IdMenu +'"]').prop('checked', true);
                
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

function obtenerIdsCheckeados() {
   
    return ids;
}
function Guardar() {
    let IdPerfil = $("#cboRol").val();
    const Accesos = [];

    // Selecciona todos los checkboxes marcados dentro de la tabla
    $('input[type="checkbox"]:checked').each(function () {
        // Obtiene el atributo data-id del tr padre
        const idMenu = $(this).attr('idmenu');
        if (idMenu) Accesos.push(parseInt(idMenu)); // Convertir a número si quieres
    });


    $("#btnAplicar").text("Guardando...")
    $("#btnAplicar").prop("disabled", true)

    $.ajax({
        url: 'grabarAcceso',
        type: 'POST',
        data: {
            IdPerfil,
            Accesos
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
        }, complete: function () {
            $("#btnAplicar").text("Aplicar")
            $("#btnAplicar").prop("disabled", false)
        }
    });

}