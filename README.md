Gapsi Global - Plataforma de Gestão de Grupos de Apoio

📜 Visão Geral

Gapsi Global é uma aplicação web completa, desenvolvida como uma plataforma de Software como Serviço (SaaS), projetada para gerenciar de forma integrada e eficiente os serviços de uma clínica de psicologia focada em terapia de grupo.

O sistema foi construído do zero, abrangendo todo o ciclo de vida do desenvolvimento, desde a concepção da arquitetura e modelagem do banco de dados até o deploy final em um ambiente de produção. A plataforma oferece portais dedicados para três tipos de usuários distintos: Administradores, Psicólogos e Pacientes, cada um com funcionalidades e permissões específicas.

Este projeto representa um case completo de desenvolvimento Full-Stack, demonstrando competências em arquitetura de software, desenvolvimento back-end, design de interface, integração com serviços de nuvem e gerenciamento de infraestrutura.
✨ Funcionalidades Principais
Painel do Administrador

    Gerenciamento de Usuários: Cadastro, visualização e exclusão de psicólogos; visualização e gerenciamento de créditos de pacientes.

    Gerenciamento de Conteúdo: CRUD completo para Grupos de Apoio, permitindo a criação e edição de nome, descrição e imagem de capa, refletindo as alterações dinamicamente no site público.

    Gestão Operacional: Gerenciamento de horários dos grupos, visualização de comprovantes de pagamento e gerenciamento de consultas.

    Comunicação Centralizada: Ferramentas para envio de mensagens e links de reunião em massa para grupos ou psicólogos específicos.

Painel do Psicólogo

    Interface Dedicada: Área para gerenciar dados de perfil público (biografia, CRP, foto).

    Ferramentas de Comunicação: Envio de mensagens e links de consulta para os grupos.

    Visualização de Mensagens: Caixa de entrada para comunicados da administração.

Portal do Paciente

    Autocadastro: Fluxo de registro completo com seleção de grupos de interesse.

    Agendamento de Consultas: Visualização de horários disponíveis e agendamento de sessões.

    Gerenciamento Financeiro: Página dedicada para realizar pagamentos via PIX e enviar comprovantes de forma segura.

    Comunicação: Acesso a links de reunião e mensagens importantes.

🛠️ Arquitetura e Tecnologias Utilizadas

A plataforma foi construída com foco em boas práticas, escalabilidade e manutenibilidade.

    Back-end: C# e ASP.NET Core 8, utilizando a arquitetura MVC (Model-View-Controller).

        Acesso a Dados: Entity Framework Core com a abordagem Code-First para modelagem e migração do banco de dados.

        Autenticação e Autorização: ASP.NET Core Identity para um sistema robusto de login e gerenciamento de roles.

        Padrões de Projeto: Injeção de Dependência para desacoplamento e organização do código em uma camada de Serviços.

    Front-end:

        HTML5, CSS3 (Flexbox e Grid Layout) e JavaScript (ES6) para interfaces responsivas e dinâmicas.

        Bootstrap 5 para um sistema de componentes e layout consistente.

        jQuery/AJAX para funcionalidades assíncronas, como a atualização dinâmica de listas.

        Acessibilidade (WCAG): Implementação de widgets e práticas para garantir a usabilidade para todos.

    Banco de Dados:

        Microsoft SQL Server.

    Infraestrutura e Cloud (DevOps):

        Amazon S3 (AWS): Utilizado como solução de armazenamento de objetos para todos os uploads de arquivos (fotos de perfil, comprovantes), garantindo performance e escalabilidade. O AWS SDK for .NET foi utilizado para a integração.

        Deploy: Gerenciamento do processo completo de publicação e implantação em um servidor de produção IIS.

    Serviços Externos:

        Integração com serviço de SMTP para o envio de e-mails transacionais (notificações de comprovantes, criação de grupos, etc.).

🚀 Como Executar o Projeto

    Clone o repositório:
    Generated bash

          
    git clone https://github.com/seu-usuario/seu-repositorio.git

        

    IGNORE_WHEN_COPYING_START

Use code with caution. Bash
IGNORE_WHEN_COPYING_END

Configure o appsettings.json:

    Renomeie appsettings.Example.json para appsettings.json.

    Preencha as strings de conexão do banco de dados, as configurações de e-mail e as credenciais da AWS (Access Key, Secret Key e Bucket Name).

Aplique as Migrações:

    No Package Manager Console do Visual Studio, execute o comando:
    Generated powershell

          
    Update-Database

        

    IGNORE_WHEN_COPYING_START

        Use code with caution. Powershell
        IGNORE_WHEN_COPYING_END

    Execute a aplicação a partir do Visual Studio (F5).

Este projeto foi desenvolvido como um sistema completo para um cliente real, demonstrando a aplicação prática de tecnologias modernas para resolver problemas de negócio.
