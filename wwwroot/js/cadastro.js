document.addEventListener("DOMContentLoaded", function () {
    document.getElementById("botaoRegistro").addEventListener("click", function (event) {
        event.preventDefault(); // Evita envio antes da validação

        let isValid = true; // Flag para validação

        // Captura os campos
        const nome = document.getElementById("nome");
        const email = document.getElementById("email");
        const telefone = document.getElementById("telefone");
        const senha = document.getElementById("senha");
        const gruposSelecionados = Array.from(document.querySelectorAll("input[name='GruposSelecionados']:checked"));
        const gruposContainer = document.querySelector(".grupos-cadastro-container");

        // Remove mensagens de erro anteriores
        document.querySelectorAll(".erro-msg, .erro-msg-grupos").forEach(el => el.remove());

        // Função para exibir erro abaixo do campo
        function exibirErro(campo, mensagem, isTabela = false) {
            const erro = document.createElement("span");
            erro.classList.add(isTabela ? "erro-msg-grupos" : "erro-msg");
            erro.innerText = mensagem;

            if (isTabela) {
                gruposContainer.insertAdjacentElement("afterend", erro); // Mensagem fica abaixo da tabela
            } else {
                campo.insertAdjacentElement("afterend", erro);
            }

            isValid = false;
        }

        // Valida cada campo
        if (!nome.value.trim()) exibirErro(nome, "O nome é obrigatório.");
        if (!email.value.trim()) exibirErro(email, "O e-mail é obrigatório.");
        if (!telefone.value.trim()) exibirErro(telefone, "O telefone é obrigatório.");
        if (!senha.value.trim()) exibirErro(senha, "A senha é obrigatória.");
        if (gruposSelecionados.length === 0) exibirErro(gruposContainer, "Selecione pelo menos um grupo.", true);

        // Se tudo estiver válido, salva os dados e avança
        if (isValid) {
            const dadosCadastro = {
                nome: nome.value.trim(),
                email: email.value.trim(),
                telefone: telefone.value.trim(),
                senha: senha.value.trim(),
                gruposSelecionados: gruposSelecionados.map(el => el.value)
            };

            idStorage.setItem("dadosCadastro", JSON.stringify(dadosCadastro)); // Alteração para idStorage
            window.location.href = "/Contrato/Index";
        }
    });
});
