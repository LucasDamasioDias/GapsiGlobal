document.addEventListener('DOMContentLoaded', () => {
    const menuToggle = document.getElementById('menu-toggle');
    const menu = document.querySelector('.menu');
    const overlay = document.querySelector('.menu-overlay');
    const logo = document.getElementById('logo-mobile');
    const welcome = document.querySelector('.welcome');
    const appRoot = document.getElementById('appRoot');

    menuToggle.addEventListener('click', () => {
        const isMenuOpen = menu.classList.toggle('menu-mobile');
        appRoot.classList.toggle('menu-open');
        

        if (isMenuOpen) {
            overlay.classList.add('menu-overlay');
            overlay.style.display = 'block';
            logo.style.display = 'block';
            welcome.style.opacity = '0'; 
            welcome.style.pointerEvents = 'none';
        } else {
            overlay.classList.remove('menu-overlay'); 
            overlay.style.display = 'none';
            logo.style.display = 'none';
            welcome.style.opacity = '1'; 
            welcome.style.pointerEvents = 'auto'; 
        }
    });
});
