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

            // Convertir campos numéricos si existen (ej. idRol)
            if (data.idRol) data.idRol = parseInt(data.idRol);

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
            .then(response => response.json())
            .then(result => {
                if (result.success) {
                    // Cerrar modal usando Bootstrap 5
                    const modalInstance = bootstrap.Modal.getInstance(this.modalElement) || new bootstrap.Modal(this.modalElement);
                    modalInstance.hide();

                    // Limpiar formulario
                    this.form.reset();

                    // Ejecutar callback (por ejemplo, recargar la tabla)
                    if (typeof this.onSuccessCallback === 'function') {
                        this.onSuccessCallback(result);
                    }
                } else {
                    alert("Error: " + result.message);
                }
            })
            .catch(error => {
                console.error("Error de red:", error);
                alert("Ocurrió un error al procesar la solicitud.");
            });
    }
}