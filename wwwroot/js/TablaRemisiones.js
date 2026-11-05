// Inicialización de Tabulator para el listado de Remisiones.
document.addEventListener("DOMContentLoaded", function () {
    var contador = document.getElementById("remisiones-count");
    var buscador = document.getElementById("remisiones-search");

    var table = new Tabulator("#remisiones-table", {
        ajaxURL: "/Remisiones/GetRemisionesJson",
        layout: "fitColumns",
        responsiveLayout: "collapse",
        pagination: "local",
        paginationSize: 10,
        paginationSizeSelector: [5, 10, 25, 50],
        paginationCounter: "rows",
        locale: "es",
        langs: {
            "es": {
                "groups": {
                    "item": "elemento",
                    "items": "elementos"
                },
                "columns": {},
                "data": {
                    "loading": "Cargando...",
                    "error": "Error al cargar los datos"
                },
                "pagination": {
                    "page_size": "Registros por página",
                    "page_title": "Ver página",
                    "first": "Primero",
                    "first_title": "Primera página",
                    "last": "Último",
                    "last_title": "Última página",
                    "prev": "Anterior",
                    "prev_title": "Página anterior",
                    "next": "Siguiente",
                    "next_title": "Página siguiente",
                    "all": "Todos",
                    "counter": {
                        "showing": "Mostrando",
                        "of": "de",
                        "rows": "registros",
                        "pages": "páginas"
                    }
                },
                "headerFilters": {
                    "default": "Buscar...",
                    "columns": {}
                }
            }
        },
        placeholder: "No hay remisiones registradas todavía.",
        columnDefaults: {
            resizable: true
        },
        columns: [
            {
                title: "N.º",
                field: "idRemision",
                width: 70,
                hozAlign: "center"
            },
            {
                title: "N.º Remisión",
                field: "numeroRemision",
                minWidth: 120
            },
            {
                title: "Solicitante",
                field: "solicitante",
                minWidth: 160
            },
            {
                title: "Área",
                field: "area",
                minWidth: 100,
                formatter: textoODefecto
            },
            {
                title: "Finca",
                field: "finca",
                minWidth: 100,
                formatter: textoODefecto
            },
            {
                title: "Fecha",
                field: "fecha",
                minWidth: 110,
                hozAlign: "center",
                formatter: formatearFecha
            },
            {
                title: "Placa",
                field: "placa",
                minWidth: 90,
                hozAlign: "center"
            },
            {
                title: "Insumos",
                field: "insumos",
                minWidth: 80,
                hozAlign: "center"
            },
            {
                title: "Acciones",
                field: "acciones",
                width: 130,
                hozAlign: "center",
                headerSort: false,
                formatter: formatearAcciones
            }
        ]
    });

    function textoODefecto(cell) {
        var valor = cell.getValue();
        return valor ? valor : "—";
    }

    function formatearFecha(cell) {
        var valor = cell.getValue();
        if (!valor) return "—";

        var fecha = new Date(valor);
        if (isNaN(fecha.getTime()) || fecha.getFullYear() <= 1) return "—";

        var pad = function (n) { return String(n).padStart(2, "0"); };
        return pad(fecha.getDate()) + "/" + pad(fecha.getMonth() + 1) + "/" + fecha.getFullYear();
    }

    function crearAccion(url, titulo, icono, claseExtra) {
        var enlace = document.createElement("a");
        enlace.href = url;
        enlace.className = "table-btn-action" + (claseExtra ? " " + claseExtra : "");
        enlace.title = titulo;

        var i = document.createElement("i");
        i.className = "bi " + icono;
        enlace.appendChild(i);

        return enlace;
    }

    function formatearAcciones(cell) {
        var id = cell.getRow().getData().idRemision;
        var grupo = document.createElement("div");
        grupo.className = "d-flex justify-content-center gap-1";

        grupo.appendChild(crearAccion("/Remisiones/Details/" + id, "Ver detalle", "bi-eye", ""));
        grupo.appendChild(crearAccion("/Remisiones/Edit/" + id, "Editar", "bi-pencil", ""));
        grupo.appendChild(crearAccion("/Remisiones/Delete/" + id, "Eliminar", "bi-trash", "delete"));

        return grupo;
    }

    function normalizar(texto) {
        return (texto || "")
            .toString()
            .normalize("NFD")
            .replace(/[\u0300-\u036f]/g, "")
            .toLowerCase();
    }

    if (buscador) {
        buscador.addEventListener("input", function () {
            var filtro = normalizar(this.value.trim());

            if (!filtro) {
                table.clearFilter();
            } else {
                table.setFilter(function (data) {
                    var texto = normalizar([
                        data.numeroRemision,
                        data.solicitante,
                        data.area,
                        data.finca,
                        data.placa
                    ].join(" "));

                    return texto.indexOf(filtro) !== -1;
                });
            }

            table.setPage(1);
        });
    }

    table.on("dataLoaded", function (data) {
        if (contador) {
            contador.textContent = data.length;
        }
    });

    // Mantiene el badge del encabezado sincronizado con la búsqueda.
    table.on("dataFiltered", function (filtros, filas) {
        if (contador) {
            contador.textContent = filas.length;
        }
    });
});
