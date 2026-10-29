// wwwroot/js/usuarios.js
document.addEventListener("DOMContentLoaded", function () {
    // 1. Inicializar Tabulator
    var table = new Tabulator("#usuarios-table", {
        ajaxURL: "/Usuarios/GetUsuariosJson",
        layout: "fitColumns",
        responsiveLayout: "collapse",
        pagination: "local",
        paginationSize: 10,
        paginationSizeSelector: [10, 25, 50, 100],
        movableColumns: true,
        columns: [
            { title: "ID", field: "idUsuarios", visible: false },
            { title: "Nombre", field: "nombre", editor: "input", width: "25%" },
            { title: "Apellido", field: "apellido", editor: "input", width: "25%" },
            { title: "Correo Electrónico", field: "email", editor: "input", width: "30%" },
            {
                title: "Fecha Creación",
                field: "fecha_creacion",
                width: "20%",
                formatter: function (cell) {
                    var val = cell.getValue();
                    return val ? val.substring(0, 10) : "";
                }
            },
        ],
    });

    // 2. Inicializar el manejador reutilizable para el modal de creación
    // Le pasamos: ID del formulario, ID del modal, Endpoint POST y qué hacer al tener éxito
    const userModalHandler = new ModalFormHandler(
        "formCreateUser",       // ID del form en _CreateModal.cshtml
        "createUserModal",      // ID del modal en Bootstrap
        "/Usuarios/CreateAjax", // Endpoint en C#
        function(result) {
            console.log(result.message);
            // Recargar los datos de la tabla Tabulator automáticamente
            table.setData("/Usuarios/GetUsuariosJson");
        }
    );

    // 3. Evento automático de edición en celda (Estilo Excel)
    table.on("cellEdited", function (cell) {
        var rowData = cell.getRow().getData();

        fetch('/Usuarios/ActualizarCelda', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify(rowData)
        })
        .then(response => response.json())
        .then(data => {
            if (!data.success) {
                alert("Error al actualizar: " + data.message);
            }
        })
        .catch(error => {
            console.error("Error de red:", error);
            alert("Ocurrió un error al intentar guardar los cambios.");
        });
    });
});