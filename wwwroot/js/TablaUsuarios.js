// wwwroot/js/TablaUsuarios.js
document.addEventListener("DOMContentLoaded", function () {
    var rolesMap = {};
    var rolesValues = {};
    var table;

    function cargarRoles(callback) {
        fetch("/Roles/GetRolesJson")
            .then(function (response) { return response.json(); })
            .then(function (roles) {
                roles = roles || [];
                rolesMap = {};
                rolesValues = {};
                roles.forEach(function (rol) {
                    var id = String(rol.idRol);
                    rolesMap[id] = rol.nombre;
                    rolesValues[id] = rol.nombre;
                });

                // Selector de rol del modal de creación
                var selectRol = document.getElementById("IdRol");
                if (selectRol) {
                    selectRol.innerHTML = "";
                    if (roles.length === 0) {
                        var sinRoles = document.createElement("option");
                        sinRoles.value = "";
                        sinRoles.textContent = "No hay roles creados. Cree uno primero.";
                        sinRoles.disabled = true;
                        sinRoles.selected = true;
                        selectRol.appendChild(sinRoles);
                    } else {
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
                    }
                }

                if (typeof callback === "function") callback();
            })
            .catch(function (error) {
                console.error("Error al cargar los roles:", error);
                if (typeof callback === "function") callback();
            });
    }

    function inicializarTabla() {
        table = new Tabulator("#usuarios-table", {
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
                    title: "Nombre", field: "nombre", editor: "input", widthGrow: 2,
                    editorParams: { selectContents: true },
                    validator: ["required"]
                },
                {
                    title: "Apellido", field: "apellido", editor: "input", widthGrow: 2,
                    editorParams: { selectContents: true },
                    validator: ["required"]
                },
                {
                    title: "Correo Electrónico", field: "email", editor: "input", widthGrow: 3,
                    editorParams: { selectContents: true },
                    validator: ["required", "email"]
                },
                {
                    title: "Rol",
                    field: "idRol",
                    widthGrow: 2,
                    editor: "list",
                    editorParams: {
                        values: rolesValues,
                        clearable: false,
                        autocomplete: true,
                        listOnEmpty: true,
                        freetext: false
                    },
                    formatter: function (cell) {
                        var id = String(cell.getValue() ?? "");
                        return rolesMap[id] || cell.getRow().getData().rol || "—";
                    },
                    headerFilter: "list",
                    headerFilterParams: { values: rolesValues, clearable: true }
                },
                {
                    title: "Estado",
                    field: "activo",
                    widthGrow: 1,
                    hozAlign: "center",
                    formatter: "tickCross",
                    editor: "tickCross",
                    headerFilter: "tickCross",
                    headerFilterParams: { tristate: true }
                },
                {
                    title: "Fecha Creación",
                    field: "fecha_creacion",
                    widthGrow: 1,
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
                {
                    title: "Modificado por",
                    field: "lastModify",
                    widthGrow: 1,
                    formatter: function (cell) {
                        var val = cell.getValue();
                        return (val && String(val).trim()) ? val : "—";
                    }
                },
                {
                    title: "Última modificación",
                    field: "lastDate",
                    widthGrow: 1,
                    hozAlign: "center",
                    formatter: function (cell) {
                        var val = cell.getValue();
                        if (!val) return "—";
                        var fecha = new Date(val);
                        if (isNaN(fecha.getTime())) return val;
                        var pad = function (n) { return String(n).padStart(2, "0"); };
                        return pad(fecha.getDate()) + "/" + pad(fecha.getMonth() + 1) + "/" + fecha.getFullYear() +
                            " " + pad(fecha.getHours()) + ":" + pad(fecha.getMinutes());
                    }
                },
            ],
        });

        // Inicializar el manejador reutilizable para el modal de creación
        new ModalFormHandler(
            "formCreateUser",
            "createUserModal",
            "/Usuarios/CreateAjax",
            function (result) {
                console.log(result.message);
                table.setData("/Usuarios/GetUsuariosJson");
            }
        );

        // Evento automático de edición en celda (Estilo Excel)
        table.on("cellEdited", function (cell) {
            var row = cell.getRow();
            var rowData = row.getData();

            var payload = {
                idUsuarios: rowData.idUsuarios,
                idRol: parseInt(rowData.idRol, 10) || 0,
                nombre: rowData.nombre,
                apellido: rowData.apellido,
                email: rowData.email,
                activo: !!rowData.activo
            };

            fetch("/Usuarios/ActualizarCelda", {
                method: "POST",
                headers: {
                    "Content-Type": "application/json"
                },
                body: JSON.stringify(payload)
            })
                .then(async function (response) {
                    var texto = await response.text();
                    var result = {};
                    try {
                        result = texto ? JSON.parse(texto) : {};
                    } catch {
                        result = { success: false, message: "Respuesta no válida del servidor." };
                    }

                    if (!response.ok || result.success === false) {
                        var err = new Error(result.message || ("Error HTTP " + response.status));
                        err.detail = result.detail || "";
                        throw err;
                    }
                    return result;
                })
                .then(function (result) {
                    row.update({
                        idRol: result.idRol,
                        rol: result.rol,
                        nombre: result.nombre,
                        apellido: result.apellido,
                        email: result.email,
                        activo: result.activo,
                        lastModify: result.lastModify,
                        lastDate: result.lastDate
                    });
                })
                .catch(function (error) {
                    console.error("Error al actualizar:", error, error.detail || "");
                    cell.restoreOldValue();
                    var msg = "No se pudo guardar el cambio: " + error.message;
                    if (error.detail) msg += "\nDetalle: " + error.detail;
                    alert(msg);
                });
        });
    }

    cargarRoles(function () {
        inicializarTabla();

        var modalUsuario = document.getElementById("createUserModal");
        if (modalUsuario) {
            modalUsuario.addEventListener("show.bs.modal", function () { cargarRoles(); });
        }
    });
});
