document.addEventListener('DOMContentLoaded', () => {
    const emailInput = document.getElementById('icon-email').nextElementSibling;
    const passwordInput = document.getElementById('icon-password').nextElementSibling;
    const confirmInput = document.getElementById('icon-confirm-password').nextElementSibling;

    const iconEmail = document.getElementById('icon-email');
    const iconPassword = document.getElementById('icon-password');
    const iconConfirm = document.getElementById('icon-confirm-password');

    const form = document.getElementById('registerForm');
    const submitBtn = form.querySelector('button[type="submit"]');

    // Crea elementos para mensajes debajo de inputs
    function createMessageElement(input) {
        let msg = document.createElement('div');
        msg.classList.add('input-message');
        msg.style.color = '#dc3545';
        msg.style.fontSize = '0.9rem';
        msg.style.marginTop = '4px';
        input.parentNode.appendChild(msg);
        return msg;
    }

    const emailMsg = createMessageElement(emailInput);
    const passwordMsg = createMessageElement(passwordInput);
    const confirmMsg = createMessageElement(confirmInput);

    function setIconState(icon, valid, message = '') {
        icon.style.transition = 'color 0.3s ease';
        if (valid) {
            icon.style.color = '#28a745'; // verde
            icon.title = "Campo válido";
        } else {
            icon.style.color = '#dc3545'; // rojo
            icon.title = message || "Campo inválido";
        }
    }

    function validateEmail(email) {
        return /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email.toLowerCase());
    }

    // Validación y mensajes para cada campo
    emailInput.addEventListener('input', () => {
        if (emailInput.value.length === 0) {
            emailMsg.textContent = '';
            setIconState(iconEmail, false, "El correo es obligatorio");
            toggleSubmit(false);
        } else if (!validateEmail(emailInput.value)) {
            emailMsg.textContent = 'Por favor, ingresa un correo válido.';
            setIconState(iconEmail, false, 'Correo inválido');
            toggleSubmit(false);
        } else {
            emailMsg.textContent = '';
            setIconState(iconEmail, true);
            checkFormValidity();
        }
    });

    passwordInput.addEventListener('input', () => {
        if (passwordInput.value.length === 0) {
            passwordMsg.textContent = '';
            setIconState(iconPassword, false, "La contraseña es obligatoria");
            toggleSubmit(false);
        } else if (passwordInput.value.length < 8) {
            passwordMsg.textContent = 'La contraseña debe tener al menos 8 caracteres.';
            setIconState(iconPassword, false, 'Contraseña muy corta');
            toggleSubmit(false);
        } else {
            passwordMsg.textContent = '';
            setIconState(iconPassword, true);
            checkFormValidity();
        }
    });

    confirmInput.addEventListener('input', () => {
        if (confirmInput.value.length === 0) {
            confirmMsg.textContent = '';
            setIconState(iconConfirm, false, "Confirma la contraseña");
            toggleSubmit(false);
        } else if (confirmInput.value !== passwordInput.value) {
            confirmMsg.textContent = 'Las contraseñas no coinciden.';
            setIconState(iconConfirm, false, 'Contraseñas diferentes');
            toggleSubmit(false);
        } else {
            confirmMsg.textContent = '';
            setIconState(iconConfirm, true);
            checkFormValidity();
        }
    });

    // Verifica si todos los campos son válidos para habilitar el botón
    function checkFormValidity() {
        const emailValid = validateEmail(emailInput.value);
        const passwordValid = passwordInput.value.length >= 8;
        const confirmValid = confirmInput.value === passwordInput.value && confirmInput.value.length >= 8;

        toggleSubmit(emailValid && passwordValid && confirmValid);
    }

    function toggleSubmit(enabled) {
        submitBtn.disabled = !enabled;
        if (enabled) {
            submitBtn.style.opacity = '1';
            submitBtn.style.cursor = 'pointer';
        } else {
            submitBtn.style.opacity = '0.6';
            submitBtn.style.cursor = 'not-allowed';
        }
    }

    // Inicializa estado botón deshabilitado
    toggleSubmit(false);
});
