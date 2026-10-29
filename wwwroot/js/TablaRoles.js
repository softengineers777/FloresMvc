// Inicialización de Tabulator para Roles
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
        { title: "", field: "idRol", width: 80, hozAlign: "center" },
        {
            title: "Nombre del Rol",
            field: "nombre",
            editor: "input", // Permite edición directa al hacer doble clic estilo Excel
            validator: ["required"]
        },
        { title: "Fecha Creación", field: "fecha_Creacion", hozAlign: "center", formatter: "datetime", formatterParams: { inputFormat: "iso", outputFormat: "YYYY-MM-DD HH:mm" } },
        { title: "Creado por", field: "owner", hozAlign: "center" },
        { title: "Última Modificación", field: "lastModify", hozAlign: "center" },
        { title: "Fecha Modificación", field: "lastDate", hozAlign: "center" }
    ],
});

// Evento para guardar cambios en tiempo real al editar una celda (Estilo Excel)
table.on("cellEdited", function (cell) {
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
            }
        })
        .catch(error => {
            console.error('Error:', error);
            cell.restoreOldValue();
        });
});