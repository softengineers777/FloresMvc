class ModalFormHandler {
    constructor(formId, modalId, submitUrl, onSuccessCallback) {
        this.form = document.getElementById(formId);
        this.modalElement = document.getElementById(modalId);
        this.submitUrl = submitUrl;
        this.onSuccessCallback = onSuccessCallback;

        this.init();
    }

    init() {
        if (!this.form || !this.modalElement) return;

        this.form.addEventListener("submit", (e) => {
            e.preventDefault();

            // Convertir los campos del formulario en un objeto JSON plano
            const formData = new FormData(this.form);
            const data = Object.fromEntries(formData.entries());

            // Convertir a número los campos numéricos (ej. IdRol), sin importar
            // si el atributo name viene en mayúsculas o minúsculas
            Object.keys(data).forEach((key) => {
                const field = this.form.elements[key];

                // Una fecha vacía ("") no se puede convertir a DateTime en el
                // servidor, así que simplemente no se envía.
                if (field && field.type === "date" && data[key] === "") {
                    delete data[key];
                    return;
                }

                const esNumerico = (field && field.type === "number") || key.toLowerCase() === "idrol";
                if (esNumerico && data[key] !== "") {
                    const numero = Number(data[key]);
                    if (!Number.isNaN(numero)) data[key] = numero;
                }
            });

            this.submitData(data);
        });
    }

    submitData(data) {
        fetch(this.submitUrl, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify(data)
        })
            .then(async (response) => {
                // El servidor puede devolver HTML en un error 500; evitamos que
                // response.json() falle y así mostramos el mensaje real.
                const texto = await response.text();
                let result = {};
                try {
                    result = texto ? JSON.parse(texto) : {};
                } catch {
                    result = { success: response.ok, message: texto };
                }

                if (!response.ok || result.success === false) {
                    throw new Error(result.message || ("Error HTTP " + response.status));
                }
                return result;
            })
            .then(result => {
                // Cerrar modal usando Bootstrap 5
                const modalInstance = bootstrap.Modal.getInstance(this.modalElement) || new bootstrap.Modal(this.modalElement);
                modalInstance.hide();

                // Limpiar formulario
                this.form.reset();

                // Ejecutar callback (por ejemplo, recargar la tabla)
                if (typeof this.onSuccessCallback === 'function') {
                    this.onSuccessCallback(result);
                }
            })
            .catch(error => {
                console.error("Error al procesar la solicitud:", error);
                alert("Error: " + error.message);
            });
    }
}