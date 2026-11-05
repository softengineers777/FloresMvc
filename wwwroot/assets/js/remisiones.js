document.addEventListener("DOMContentLoaded", function () {
    const body = document.getElementById("detalles-body");
    const addBtn = document.getElementById("add-detalle");

    if (!body || !addBtn) {
        return;
    }

    // Vuelve a numerar los campos para que el model binder los reciba
    // como Detalles[0], Detalles[1], etc. después de agregar o quitar filas.
    function reindexar() {
        body.querySelectorAll("tr.detalle-row").forEach(function (row, index) {
            row.querySelectorAll("input").forEach(function (input) {
                if (input.name) {
                    input.name = input.name.replace(/Detalles\[\d+\]/g, "Detalles[" + index + "]");
                }
                if (input.id) {
                    input.id = input.id.replace(/Detalles_\d+__/g, "Detalles_" + index + "__");
                }
            });

            row.querySelectorAll("[data-valmsg-for]").forEach(function (span) {
                const actual = span.getAttribute("data-valmsg-for");
                if (actual) {
                    span.setAttribute(
                        "data-valmsg-for",
                        actual.replace(/Detalles\[\d+\]/g, "Detalles[" + index + "]")
                    );
                }
            });
        });
    }

    function limpiarFila(row) {
        row.querySelectorAll("input").forEach(function (input) {
            input.value = input.type === "hidden" ? "0" : "";
        });
        row.querySelectorAll("span.text-danger").forEach(function (span) {
            span.textContent = "";
        });
    }

    addBtn.addEventListener("click", function () {
        const rows = body.querySelectorAll("tr.detalle-row");
        if (rows.length === 0) {
            return;
        }

        const clone = rows[rows.length - 1].cloneNode(true);
        limpiarFila(clone);
        body.appendChild(clone);
        reindexar();
    });

    body.addEventListener("click", function (event) {
        const boton = event.target.closest(".remove-detalle");
        if (!boton) {
            return;
        }

        const rows = body.querySelectorAll("tr.detalle-row");
        if (rows.length === 1) {
            limpiarFila(rows[0]);
            return;
        }

        boton.closest("tr.detalle-row").remove();
        reindexar();
    });
});
