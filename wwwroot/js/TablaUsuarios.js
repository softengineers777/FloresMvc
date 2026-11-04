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
            {
                title: "Nombre", field: "nombre", editor: "input", width: "22%",
                editorParams: { selectContents: true },
                validator: ["required"]
            },
            {
                title: "Apellido", field: "apellido", editor: "input", width: "22%",
                editorParams: { selectContents: true },
                validator: ["required"]
            },
            {
                title: "Correo Electrónico", field: "email", editor: "input", width: "30%",
                editorParams: { selectContents: true },
                validator: ["required", "email"]
            },
            {
                title: "Fecha Creación",
                field: "fecha_creacion",
                width: "18%",
                hozAlign: "center",
                formatter: function (cell) {
                    var val = cell.getValue();
                    if (!val) return "—";
                    var fecha = new Date(val);
                    if (isNaN(fecha.getTime())) return val;
                    var pad = function (n) { return String(n).padStart(2, "0"); };
                    return pad(fecha.getDate()) + "/" + pad(fecha.getMonth() + 1) + "/" + fecha.getFullYear();
                }
            },
        ],
    });

    // 2. Cargar los roles reales en el selector del modal de creación
    var selectRol = document.getElementById("IdRol");
    var modalUsuario = document.getElementById("createUserModal");

    function cargarRoles() {
        if (!selectRol) return;

        fetch("/Roles/GetRolesJson")
            .then(function (response) { return response.json(); })
            .then(function (roles) {
                selectRol.innerHTML = "";

                if (!roles || roles.length === 0) {
                    var sinRoles = document.createElement("option");
                    sinRoles.value = "";
                    sinRoles.textContent = "No hay roles creados. Cree uno primero.";
                    sinRoles.disabled = true;
                    sinRoles.selected = true;
                    selectRol.appendChild(sinRoles);
                    return;
                }

                var placeholder = document.createElement("option");
                placeholder.value = "";
                placeholder.textContent = "Seleccione un rol...";
                placeholder.disabled = true;
                placeholder.selected = true;
                selectRol.appendChild(placeholder);

                roles.forEach(function (rol) {
                    var opcion = document.createElement("option");
                    opcion.value = rol.idRol;
                    opcion.textContent = rol.nombre;
                    selectRol.appendChild(opcion);
                });
            })
            .catch(function (error) {
                console.error("Error al cargar los roles:", error);
            });
    }

    cargarRoles();
    if (modalUsuario) {
        modalUsuario.addEventListener("show.bs.modal", cargarRoles);
    }

    // 3. Inicializar el manejador reutilizable para el modal de creación
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

    // 4. Evento automático de edición en celda (Estilo Excel)
    // Solo se envían los campos que el servidor puede actualizar.
    table.on("cellEdited", function (cell) {
        var row = cell.getRow();
        var rowData = row.getData();

        var payload = {
            idUsuarios: rowData.idUsuarios,
            nombre: rowData.nombre,
            apellido: rowData.apellido,
            email: rowData.email
        };

        fetch("/Usuarios/ActualizarCelda", {
            method: "POST",
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify(payload)
        })
            .then(async function (response) {
                // El servidor podría devolver HTML (por ejemplo, si la sesión
                // expiró); se lee como texto para no romper el JSON.parse.
                var texto = await response.text();
                var result = {};
                try {
                    result = texto ? JSON.parse(texto) : {};
                } catch {
                    result = { success: false, message: "Respuesta no válida del servidor." };
                }

                if (!response.ok || result.success === false) {
                    throw new Error(result.message || ("Error HTTP " + response.status));
                }
                return result;
            })
            .then(function (result) {
                // Reflejar en la fila los valores ya normalizados por el servidor.
                row.update({
                    nombre: result.nombre,
                    apellido: result.apellido,
                    email: result.email
                });
            })
            .catch(function (error) {
                console.error("Error al actualizar:", error);
                // Si no se guardó, se devuelve la celda a su valor anterior.
                cell.restoreOldValue();
                alert("No se pudo guardar el cambio: " + error.message);
            });
    });
});