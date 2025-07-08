document.addEventListener('DOMContentLoaded', function () {
    const accessibilityToggleButton = document.getElementById('accessibility-toggle-btn');
    const accessibilityPanel = document.getElementById('accessibility-panel');

    if (accessibilityToggleButton && accessibilityPanel) {
        accessibilityToggleButton.addEventListener('click', function () {

            accessibilityPanel.classList.toggle('visible');

            const isExpanded = accessibilityPanel.classList.contains('visible');
            accessibilityToggleButton.setAttribute('aria-expanded', isExpanded.toString());

            if (isExpanded) {
                const firstButtonInPanel = accessibilityPanel.querySelector('ul li button');
                if (firstButtonInPanel) {
                }
            }
        });

        document.addEventListener('click', function (event) {
            if (accessibilityPanel.classList.contains('visible') &&
                !accessibilityToggleButton.contains(event.target) &&
                !accessibilityPanel.contains(event.target)) {
                accessibilityPanel.classList.remove('visible');
                accessibilityToggleButton.setAttribute('aria-expanded', 'false');
            }
        });

        const btnAumentarFonte = document.getElementById('btn-aumentar-fonte');
        const btnDiminuirFonte = document.getElementById('btn-diminuir-fonte');
        const btnAltoContraste = document.getElementById('btn-alto-contraste');
        const corpoDaPagina = document.body; 

        let tamanhoFonteAtual = 1; 

        if (btnAumentarFonte) {
            btnAumentarFonte.addEventListener('click', function () {
                tamanhoFonteAtual += 0.1;
                corpoDaPagina.style.fontSize = tamanhoFonteAtual + 'em';
                console.log('Aumentar fonte para: ' + tamanhoFonteAtual + 'em');
            });
        }

        if (btnDiminuirFonte) {
            btnDiminuirFonte.addEventListener('click', function () {
                if (tamanhoFonteAtual > 0.5) { 
                    tamanhoFonteAtual -= 0.1;
                    corpoDaPagina.style.fontSize = tamanhoFonteAtual + 'em';
                    console.log('Diminuir fonte para: ' + tamanhoFonteAtual + 'em');
                }
            });
        }

        if (btnAltoContraste) {
            btnAltoContraste.addEventListener('click', function () {
                corpoDaPagina.classList.toggle('alto-contraste');
                console.log('Alto contraste: ' + corpoDaPagina.classList.contains('alto-contraste'));
            });
        }

    } else {
        console.warn('Botão de toggle de acessibilidade ou painel não encontrado.');
    }
});