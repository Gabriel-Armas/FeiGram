function validarContrasenias() {
    const pass = document.getElementById("contrasena").value;
    const confirm = document.getElementById("confirmarContrasena").value;
    const error = document.getElementById("mensajeError");

    if (pass !== confirm) {
        error.classList.remove("d-none");
        return false;
    }

    error.classList.add("d-none");
    return true;
}