
document.addEventListener('DOMContentLoaded', () => {
    const menuToggle = document.getElementById('menu-toggle');
    const menu = document.querySelector('.menu');
    const overlay = document.querySelector('.menu-overlay');
    const body = document.body;

    if (!menuToggle || !menu || !overlay) {
        return;
    }

    function isMobileView() {
        return window.innerWidth <= 1023;
    }

    function abrirMenu() {        
        if (!isMobileView()) return;

        menu.classList.add('menu-mobile');
        overlay.style.display = 'block';
        body.classList.add('menu-open');
        menuToggle.setAttribute('aria-expanded', 'true');
    }

    function fecharMenu() {        
        if (!isMobileView()) return;

        menu.classList.remove('menu-mobile');
        overlay.style.display = 'none';
        body.classList.remove('menu-open');
        menuToggle.setAttribute('aria-expanded', 'false');
    }

    function isHomePage() {
        return window.location.pathname === '/' ||
            window.location.pathname.toLowerCase().startsWith('/home');
    }

    if (isHomePage() && isMobileView()) {
        abrirMenu();
    }

    menuToggle.addEventListener('click', () => {       
        if (menu.classList.contains('menu-mobile')) {
            fecharMenu();
        } else {
            abrirMenu();
        }
    });

    const menuLinks = menu.querySelectorAll('a');
    menuLinks.forEach(link => {
        link.addEventListener('click', () => {            
            fecharMenu();
        });
    });

    window.addEventListener('resize', () => {       
        if (!isMobileView() && menu.classList.contains('menu-mobile')) {
            fecharMenu();
        }
    });
});