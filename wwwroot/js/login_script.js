document.addEventListener('DOMContentLoaded', function () {
    const senhaInput = document.getElementById('Senha');
    const mostrarSenhaCheckbox = document.getElementById('mostrarSenhaCheckbox');

    if (senhaInput && mostrarSenhaCheckbox) {
        mostrarSenhaCheckbox.addEventListener('change', function () {
            senhaInput.type = this.checked ? 'text' : 'password';
        });
    }
})