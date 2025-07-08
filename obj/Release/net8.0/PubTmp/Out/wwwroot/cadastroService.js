export async function enviarDadosParaServidor(dados) {
    try {
        let response = await fetch("/Cadastro/ArmazenarTemporario", {
            method: "POST",
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify(dados)
        });

        if (response.ok) {
            return await response.text(); // Retorna o ID gerado pelo backend
        } else {
            console.error("Erro ao armazenar temporariamente:", response.status);
            return null;
        }
    } catch (error) {
        console.error("Erro na requisição:", error);
        return null;
    }
}

export function abrirContrato() {
    // Aqui você pode carregar a nova view via JavaScript (se for SPA) ou apenas exibir um pop-up
    window.location.href = "/Contrato"; // Alternativa: trocar o conteúdo da página sem recarregar
}
