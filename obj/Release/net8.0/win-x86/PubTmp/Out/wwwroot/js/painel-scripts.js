function setupToggleVisibility(checkboxId, elementoId) {
    const checkbox = document.getElementById(checkboxId);
    const elemento = document.getElementById(elementoId);
    
    if (!checkbox || !elemento) {
        return;
    }

    function toggleElement() {
        elemento.style.display = checkbox.checked ? 'none' : 'block';
    }

    checkbox.addEventListener('change', toggleElement);
 
    toggleElement();
}