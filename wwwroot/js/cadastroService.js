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
            return await response.text(); 
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
    window.location.href = "/Contrato"; 
}
