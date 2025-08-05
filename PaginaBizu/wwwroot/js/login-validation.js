document.addEventListener('DOMContentLoaded', () => {
    const emailInput = document.getElementById('email');
    const passwordInput = document.getElementById('password');
    const form = emailInput.closest('form');
    const submitBtn = form.querySelector('button[type="submit"]');

    // Crear mensajes de error dinámicos debajo de cada input
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

    function validateEmail(email) {
        return /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email.toLowerCase());
    }

    function toggleSubmit(enabled) {
        submitBtn.disabled = !enabled;
        submitBtn.style.opacity = enabled ? '1' : '0.6';
        submitBtn.style.cursor = enabled ? 'pointer' : 'not-allowed';
    }

    emailInput.addEventListener('input', () => {
        if (emailInput.value.length === 0) {
            emailMsg.textContent = '';
            toggleSubmit(false);
        } else if (!validateEmail(emailInput.value)) {
            emailMsg.textContent = 'Por favor, ingresa un correo válido.';
            toggleSubmit(false);
        } else {
            emailMsg.textContent = '';
            if (passwordInput.value.length > 0) toggleSubmit(true);
        }
    });

    passwordInput.addEventListener('input', () => {
        if (passwordInput.value.length === 0) {
            passwordMsg.textContent = '';
            toggleSubmit(false);
        } else if (passwordInput.value.length < 8) {
            passwordMsg.textContent = 'La contraseña debe tener al menos 8 caracteres.';
            toggleSubmit(false);
        } else {
            passwordMsg.textContent = '';
            if (validateEmail(emailInput.value)) toggleSubmit(true);
        }
    });

    // Inicializa botón deshabilitado
    toggleSubmit(false);
});
