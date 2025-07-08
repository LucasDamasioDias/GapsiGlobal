export function habilitarBotaoCadastro() {
    const checkbox = document.getElementById("checkboxConcordo");
    const botaoCadastro = document.getElementById("botaoCadastro");

    botaoCadastro.disabled = !checkbox.checked;
}

export async function enviarCadastro(idCadastro) {
    try {
        let response = await fetch(`/Cadastro/ConfirmarCadastro?idCadastro=${idCadastro}`, {
            method: "POST"
        });
        alert("Cadastro realizado com sucesso!");
        window.location.href = "/Login/Index";

    } catch (error) {
        console.error("Erro ao chamar ConfirmarCadastro:", error);
    }
}


window.enviarCadastro = enviarCadastro;
export function voltarParaCadastro() {   
    window.location.href = "/Cadastro/Index";
}
