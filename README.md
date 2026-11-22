# Forja Team App

Sistema web de gestão para a **Forja Team — Escola de Lutas**, desenvolvido em **ASP.NET Core Razor Pages** com **SQLite**, focado no controle de alunos, turmas, débitos, check-in/presenças e áreas de acesso separadas por perfil.

## Funcionalidades

- **Autenticação por Cookies e Perfis (`Admin` e `Aluno`):**
  - **Administrador:** acessa via credenciais configuradas (`AdminPerfil`) com acesso completo ao painel de gestão, alunos, turmas, débitos e check-in.
  - **Aluno:** o administrador cadastra o CPF do aluno, e o próprio aluno define sua senha no primeiro acesso (`/Cadastrar`) antes de entrar na sua área (`/AreaAluno`).
- **Controle de Presenças e Integração:** registro de presenças e endpoint protegido por API Key (`POST /api/presencas/registrar`) via header `X-Api-Key`.
- **Interface PWA e Tema Personalizado:** visual escuro com identidade vermelho e preto, suporte a `manifest.json` e `service-worker.js`.

## Tecnologias Utilizadas

- **Backend & Web:** ASP.NET Core Razor Pages (.NET)
- **Banco de Dados:** Entity Framework Core + SQLite
- **Segurança & Configuração:** Hashing de senhas, autenticação via Cookies, `User Secrets` e variáveis de ambiente
- **Frontend:** HTML5, CSS3, JavaScript, Lucide Icons

## Configuração Local

### 1. Clonar o repositório
```bash
git clone [https://github.com/Jjoaobeco/Forja-team-gerenciamento.git](https://github.com/Jjoaobeco/Forja-team-gerenciamento.git)
cd Forja-team-gerenciamento
```

### 2. Configurar segredos com `dotnet user-secrets`
Evite colocar senhas ou chaves reais no `appsettings.json`. Configure localmente com:

```bash
dotnet user-secrets init
dotnet user-secrets set "AdminPerfil:Senha" "sua-senha-admin"
dotnet user-secrets set "IntegracaoPresenca:ApiKey" "sua-chave-api"
```

*(Opcional)* Você também pode definir o usuário e nome do admin via `AdminPerfil:Usuario` e `AdminPerfil:Nome`, ou usar variáveis de ambiente (`AdminPerfil__Senha`, `IntegracaoPresenca__ApiKey`).

### 3. Executar a aplicação
```bash
dotnet run
```

O banco SQLite local (`*.db`) e os arquivos de build (`bin/`, `obj/`) são ignorados pelo `.gitignore`.

## Subindo o `README.md` para o GitHub

Depois de salvar o arquivo **`README.md`** na pasta do projeto, rode no terminal:

```powershell
git add README.md
git commit -m "docs: adiciona README com visão geral e instruções de setup"
git push
```

---

Se quiser adicionar alguma seção extra — como prints das telas, detalhes das regras de graduação/débitos ou como rodar o endpoint de presença via `curl`/PowerShell — me avisa que já encaixamos!

<FollowUp label="Quer incluir exemplos prontos de requisição para testar o endpoint de presenças no README?" query="Adicione no README um exemplo de requisição HTTP para o endpoint /api/presencas/registrar com o header X-Api-Key."/>