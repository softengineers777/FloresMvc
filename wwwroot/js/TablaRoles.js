// Inicialización de Tabulator para Roles
document.addEventListener("DOMContentLoaded", function () {
    var table = new Tabulator("#roles-table", {
        ajaxURL: "/Roles/GetRolesJson", // Endpoint que devolverá la lista de roles en JSON
        layout: "fitColumns",
        responsiveLayout: "hideColumns",
        pagination: "local",
        paginationSize: 10,
        paginationSizeSelector: [5, 10, 25, 50],
        locale: true,
        langs: {
            "es": {
                "ajax": {
                    "loading": "Cargando...",
                    "error": "Error al cargar los datos"
                },
                "pagination": {
                    "page_size": "Registros por página",
                    "first": "Primero",
                    "first_title": "Primera página",
                    "last": "Último",
                    "last_title": "Última página",
                    "prev": "Anterior",
                    "prev_title": "Página anterior",
                    "next": "Siguiente",
                    "next_title": "Página siguiente",
                }
            }
        },
        columnDefaults: {
            resizable: true,
        },
        columns: [
            { formatter: "rownum", width: 60, hozAlign: "center", headerSort: false },
            { title: "ID", field: "idRol", width: 80, hozAlign: "center" },
            {
                title: "Nombre del Rol",
                field: "nombre",
                editor: "input", // Permite edición directa al hacer doble clic estilo Excel
                validator: ["required"]
            },
            {
                title: "Fecha Creación",
                field: "fecha_Creacion",
                hozAlign: "center",
                formatter: function (cell) {
                    return formatearFecha(cell.getValue());
                }
            },
            { title: "Creado por", field: "owner", hozAlign: "center" },
            { title: "Última Modificación", field: "lastModify", hozAlign: "center" },
            {
                title: "Fecha Modificación",
                field: "lastDate",
                hozAlign: "center",
                formatter: function (cell) {
                    return formatearFecha(cell.getValue());
                }
            },
            {
                title: "Acciones",
                field: "idRol",
                hozAlign: "center",
                headerSort: false,
                width: 140,
                formatter: function (cell) {
                    var id = cell.getValue();
                    return '<a class="btn btn-sm btn-outline-primary" href="/Roles/Permisos/' + id + '" title="Gestionar permisos"><i class="bi bi-shield-lock"></i> Permisos</a>';
                }
            }
        ],
    });

    function formatearFecha(valor) {
        if (!valor) return "—";
        var fecha = new Date(valor);
        if (isNaN(fecha.getTime())) return valor;

        var pad = function (n) { return String(n).padStart(2, "0"); };
        return fecha.getFullYear() + "-" + pad(fecha.getMonth() + 1) + "-" + pad(fecha.getDate()) +
            " " + pad(fecha.getHours()) + ":" + pad(fecha.getMinutes());
    }

    // Evento para guardar cambios en tiempo real al editar una celda (Estilo Excel)
    table.on("cellEdited", function (cell) {
        // Solo se edita el nombre del rol
        if (cell.getField() !== "nombre") return;

        var data = cell.getRow().getData();

        fetch('/Roles/ActualizarCelda', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify(data)
        })
            .then(response => response.json())
            .then(result => {
                if (!result.success) {
                    alert("Error al actualizar: " + result.message);
                    cell.restoreOldValue(); // Revierte el cambio si falla el servidor
                    return;
                }

                // Refresca la trazabilidad de la última modificación en la fila
                cell.getRow().update({
                    lastModify: result.lastModify,
                    lastDate: result.lastDate
                });
            })
            .catch(error => {
                console.error('Error:', error);
                cell.restoreOldValue();
            });
    });

    // Reutiliza el manejador genérico de modales para crear roles con auditoría
    if (typeof ModalFormHandler !== "undefined") {
        new ModalFormHandler("formCreateRol", "createRolModal", "/Roles/CreateAjax", function () {
            table.replaceData();
        });
    }
});
