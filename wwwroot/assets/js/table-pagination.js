/*
 * Paginación reutilizable para las tablas HTML planas.
 *
 * Uso:
 *   <table class="table-custom" id="mi-tabla" data-paginate data-page-size="10">
 *
 * El script toma las filas del <tbody>, se conecta con el buscador
 * (input.table-search-input) de la misma tarjeta y agrega el pie de
 * paginación automáticamente. No requiere configuración adicional.
 */
(function () {
    "use strict";

    var TAMANOS_PAGINA = [5, 10, 25, 50];
    var TAMANO_POR_DEFECTO = 10;

    function normalizar(texto) {
        return (texto || "")
            .toString()
            .normalize("NFD")
            .replace(/[\u0300-\u036f]/g, "")
            .toLowerCase()
            .trim();
    }

    function esFilaDeDatos(fila) {
        if (fila.hasAttribute("data-pagination-empty")) {
            return false;
        }

        // Las filas de "sin registros" ocupan todas las columnas.
        return !(fila.cells.length === 1 && fila.cells[0].hasAttribute("colspan"));
    }

    function construirPaginas(totalPaginas, paginaActual) {
        var paginas = [];

        if (totalPaginas <= 7) {
            for (var i = 1; i <= totalPaginas; i++) {
                paginas.push(i);
            }
            return paginas;
        }

        paginas.push(1);

        var inicio = Math.max(2, paginaActual - 1);
        var fin = Math.min(totalPaginas - 1, paginaActual + 1);

        if (inicio > 2) {
            paginas.push("...");
        }

        for (var j = inicio; j <= fin; j++) {
            paginas.push(j);
        }

        if (fin < totalPaginas - 1) {
            paginas.push("...");
        }

        paginas.push(totalPaginas);
        return paginas;
    }

    function crearItemNav(texto, deshabilitado, alHacerClic, etiqueta) {
        var item = document.createElement("li");
        item.className = "page-item" + (deshabilitado ? " disabled" : "");

        var boton = document.createElement("button");
        boton.type = "button";
        boton.className = "page-link";
        boton.textContent = texto;
        boton.setAttribute("aria-label", etiqueta);
        boton.disabled = deshabilitado;

        if (!deshabilitado) {
            boton.addEventListener("click", alHacerClic);
        }

        item.appendChild(boton);
        return item;
    }

    function crearItemNumero(numero, activo, alHacerClic) {
        var item = document.createElement("li");
        item.className = "page-item" + (activo ? " active" : "");

        var boton = document.createElement("button");
        boton.type = "button";
        boton.className = "page-link";
        boton.textContent = numero;
        boton.setAttribute("aria-label", "Página " + numero);

        if (activo) {
            boton.setAttribute("aria-current", "page");
        } else {
            boton.addEventListener("click", alHacerClic);
        }

        item.appendChild(boton);
        return item;
    }

    function crearPie(tabla, tbody, filas, columnas, buscador) {
        var contenedor = tabla.closest(".table-card-custom");
        if (!contenedor) {
            return;
        }

        var tamanoInicial = parseInt(tabla.getAttribute("data-page-size"), 10);
        if (!tamanoInicial || tamanoInicial < 1) {
            tamanoInicial = TAMANO_POR_DEFECTO;
        }

        var tamanoPagina = tamanoInicial;
        var paginaActual = 1;
        var filasFiltradas = filas.slice();

        // Fila que se muestra cuando la búsqueda no encuentra coincidencias.
        var filaSinResultados = document.createElement("tr");
        filaSinResultados.setAttribute("data-pagination-empty", "true");
        filaSinResultados.style.display = "none";

        var celdaSinResultados = document.createElement("td");
        celdaSinResultados.colSpan = columnas;
        celdaSinResultados.className = "text-center py-5 text-muted";

        var icono = document.createElement("i");
        icono.className = "bi bi-search d-block mb-2";
        icono.style.fontSize = "2rem";

        celdaSinResultados.appendChild(icono);
        celdaSinResultados.appendChild(document.createTextNode("No se encontraron coincidencias."));
        filaSinResultados.appendChild(celdaSinResultados);
        tbody.appendChild(filaSinResultados);

        // --- Pie de paginación ---
        var pie = document.createElement("div");
        pie.className = "table-pagination";

        var grupoTamano = document.createElement("div");
        grupoTamano.className = "table-pagination-size";

        var etiquetaTamano = document.createElement("label");
        etiquetaTamano.className = "table-pagination-label";
        etiquetaTamano.textContent = "Filas por página";

        var selector = document.createElement("select");
        selector.className = "form-select form-select-sm table-pagination-select";
        selector.setAttribute("aria-label", "Registros por página");

        TAMANOS_PAGINA.forEach(function (tamano) {
            var opcion = document.createElement("option");
            opcion.value = String(tamano);
            opcion.textContent = String(tamano);
            selector.appendChild(opcion);
        });

        selector.value = String(tamanoPagina);
        etiquetaTamano.appendChild(selector);
        grupoTamano.appendChild(etiquetaTamano);

        var info = document.createElement("div");
        info.className = "table-pagination-info";
        info.setAttribute("aria-live", "polite");

        var navegacion = document.createElement("nav");
        navegacion.className = "table-pagination-pages";
        navegacion.setAttribute("aria-label", "Paginación de la tabla");

        var lista = document.createElement("ul");
        lista.className = "pagination pagination-sm mb-0";
        navegacion.appendChild(lista);

        pie.appendChild(grupoTamano);
        pie.appendChild(info);
        pie.appendChild(navegacion);

        function render() {
            var total = filasFiltradas.length;
            var totalPaginas = Math.max(1, Math.ceil(total / tamanoPagina));

            if (paginaActual > totalPaginas) {
                paginaActual = totalPaginas;
            }
            if (paginaActual < 1) {
                paginaActual = 1;
            }

            var inicio = (paginaActual - 1) * tamanoPagina;
            var fin = Math.min(inicio + tamanoPagina, total);

            filas.forEach(function (fila) {
                fila.style.display = "none";
            });

            filasFiltradas.slice(inicio, fin).forEach(function (fila) {
                fila.style.display = "";
            });

            filaSinResultados.style.display = total === 0 ? "" : "none";

            if (total === 0) {
                info.textContent = "No se encontraron registros";
            } else {
                var texto = "Mostrando " + (inicio + 1) + "–" + fin +
                    " de " + total + " registro" + (total === 1 ? "" : "s");

                if (total !== filas.length) {
                    texto += " (filtrado de " + filas.length + ")";
                }

                info.textContent = texto;
            }

            lista.innerHTML = "";

            lista.appendChild(crearItemNav(
                "Anterior",
                paginaActual === 1,
                function () {
                    paginaActual -= 1;
                    render();
                },
                "Página anterior"
            ));

            construirPaginas(totalPaginas, paginaActual).forEach(function (pagina) {
                if (pagina === "...") {
                    var separador = document.createElement("li");
                    separador.className = "page-item disabled";
                    var span = document.createElement("span");
                    span.className = "page-link";
                    span.textContent = "…";
                    separador.appendChild(span);
                    lista.appendChild(separador);
                    return;
                }

                lista.appendChild(crearItemNumero(pagina, pagina === paginaActual, function () {
                    paginaActual = pagina;
                    render();
                }));
            });

            lista.appendChild(crearItemNav(
                "Siguiente",
                paginaActual === totalPaginas,
                function () {
                    paginaActual += 1;
                    render();
                },
                "Página siguiente"
            ));
        }

        selector.addEventListener("change", function () {
            var nuevoTamano = parseInt(this.value, 10);
            tamanoPagina = nuevoTamano > 0 ? nuevoTamano : TAMANO_POR_DEFECTO;
            paginaActual = 1;
            render();
        });

        if (buscador) {
            buscador.addEventListener("input", function () {
                var filtro = normalizar(this.value);

                filasFiltradas = filtro === ""
                    ? filas.slice()
                    : filas.filter(function (fila) {
                        return normalizar(fila.textContent).indexOf(filtro) !== -1;
                    });

                paginaActual = 1;
                render();
            });
        }

        var tablaResponsive = tabla.closest(".table-responsive");

        if (tablaResponsive && tablaResponsive.parentElement === contenedor) {
            contenedor.insertBefore(pie, tablaResponsive.nextSibling);
        } else {
            contenedor.appendChild(pie);
        }

        render();
    }

    function inicializarTabla(tabla) {
        if (tabla.getAttribute("data-pagination-ready") === "true") {
            return;
        }

        var tbody = tabla.tBodies[0];
        if (!tbody) {
            return;
        }

        var filas = Array.prototype.slice.call(tbody.rows).filter(esFilaDeDatos);

        // Si la tabla no tiene datos, no se agrega el pie de paginación.
        if (filas.length === 0) {
            return;
        }

        var columnas = tabla.tHead && tabla.tHead.rows[0]
            ? tabla.tHead.rows[0].cells.length
            : filas[0].cells.length;

        var contenedor = tabla.closest(".table-card-custom");
        var buscador = contenedor ? contenedor.querySelector(".table-search-input") : null;

        tabla.setAttribute("data-pagination-ready", "true");
        crearPie(tabla, tbody, filas, columnas, buscador);
    }

    function inicializar() {
        var tablas = document.querySelectorAll("table[data-paginate]");
        Array.prototype.forEach.call(tablas, inicializarTabla);
    }

    if (document.readyState === "loading") {
        document.addEventListener("DOMContentLoaded", inicializar);
    } else {
        inicializar();
    }
})();
