document.addEventListener("DOMContentLoaded", function () {
    const body = document.getElementById("preguntas-body");
    const addBtn = document.getElementById("add-pregunta");

    if (!body || !addBtn) {
        return;
    }

    function reindexar() {
        body.querySelectorAll("tr.pregunta-row").forEach(function (row, index) {
            const ordenInput = row.querySelector('input[name*=".Orden"]');
            if (ordenInput && (!ordenInput.value || ordenInput.value === "0")) {
                ordenInput.value = (index + 1).toString();
            }

            row.querySelectorAll("input, textarea").forEach(function (input) {
                if (input.name) {
                    input.name = input.name.replace(/Preguntas\[\d+\]/g, "Preguntas[" + index + "]");
                }
                if (input.id) {
                    input.id = input.id.replace(/Preguntas_\d+__/g, "Preguntas_" + index + "__");
                }
            });

            row.querySelectorAll("[data-valmsg-for]").forEach(function (span) {
                const actual = span.getAttribute("data-valmsg-for");
                if (actual) {
                    span.setAttribute(
                        "data-valmsg-for",
                        actual.replace(/Preguntas\[\d+\]/g, "Preguntas[" + index + "]")
                    );
                }
            });
        });
    }

    function limpiarFila(row) {
        row.querySelectorAll("input, textarea").forEach(function (input) {
            if (input.type === "hidden" && input.name && input.name.indexOf("IdPregunta") !== -1) {
                input.value = "0";
            } else if (input.name && input.name.indexOf(".Orden") !== -1) {
                input.value = "";
            } else if (input.type !== "hidden") {
                input.value = "";
            }
        });
        row.querySelectorAll("span.text-danger").forEach(function (span) {
            span.textContent = "";
        });
    }

    addBtn.addEventListener("click", function () {
        const rows = body.querySelectorAll("tr.pregunta-row");
        if (rows.length === 0) return;
        const clone = rows[rows.length - 1].cloneNode(true);
        limpiarFila(clone);
        body.appendChild(clone);
        reindexar();
    });

    body.addEventListener("click", function (event) {
        const boton = event.target.closest(".remove-pregunta");
        if (!boton) return;
        const rows = body.querySelectorAll("tr.pregunta-row");
        if (rows.length === 1) {
            limpiarFila(rows[0]);
            reindexar();
            return;
        }
        boton.closest("tr.pregunta-row").remove();
        reindexar();
    });

    reindexar();
});
