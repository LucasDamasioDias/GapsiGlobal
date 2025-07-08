document.addEventListener('DOMContentLoaded', () => {
    const menuToggle = document.getElementById('menu-toggle');
    const menu = document.querySelector('.menu');
    const overlay = document.querySelector('.menu-overlay');
    const logo = document.getElementById('logo-mobile');
    const welcome = document.querySelector('.welcome');
    const appRoot = document.getElementById('appRoot');

    menuToggle.addEventListener('click', () => {
        const isMenuOpen = menu.classList.toggle('menu-mobile'); // Alterna o menu
        appRoot.classList.toggle('menu-open');
        

        if (isMenuOpen) {
            overlay.classList.add('menu-overlay'); // Adiciona classe para exibir
            overlay.style.display = 'block';
            logo.style.display = 'block';
            welcome.style.opacity = '0'; // Torna invisível sem afetar a estrutura
            welcome.style.pointerEvents = 'none'; // Impede interações
        } else {
            overlay.classList.remove('menu-overlay'); // Remove a classe
            overlay.style.display = 'none';
            logo.style.display = 'none';
            welcome.style.opacity = '1'; // Restaura a visibilidade
            welcome.style.pointerEvents = 'auto'; // Permite interações novamente
        }
    });
});
