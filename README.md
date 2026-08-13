# SuporteRemoto

Plataforma de suporte remoto para a equipe de TI interna, com possibilidade de evoluir para
produto no futuro. Três módulos combinados: **acesso remoto** (tipo TeamViewer), **sistema de
tickets** e **chat ao vivo**. Stack: C# / .NET 8.

O módulo de **tickets** está completo (atribuição, comentários, anexos) com **chat embutido em
cada ticket** (não um módulo solto — cada ticket tem sua própria conversa em tempo real entre
solicitante e técnico). O módulo de **acesso remoto** ainda é só o hub de sinalização e os
projetos de agente/viewer conectando (sem captura/streaming de tela, que é o item de maior risco
técnico e será tratado em uma etapa própria).

## Arquitetura

```
SuporteRemoto.sln
/src
  SuporteRemoto.Domain          -> Entidades e enums (Ticket, ChatThread, RemoteSession, ...)
  SuporteRemoto.Application     -> Interfaces de repositório (casos de uso)
  SuporteRemoto.Infrastructure  -> EF Core (MySQL/Pomelo), ASP.NET Core Identity, repositórios
  SuporteRemoto.Shared          -> DTOs/contratos usados pela Api, Web, Agent e Viewer
  SuporteRemoto.Api             -> ASP.NET Core Web API + SignalR (auth JWT, tickets, hubs)
  SuporteRemoto.Web             -> Blazor Server (login, tickets, detalhe do ticket com chat embutido)
  SuporteRemoto.RemoteAgent     -> Worker Service instalado na máquina do usuário final
  SuporteRemoto.RemoteViewer    -> App WPF do técnico (visualização/controle remoto)
/tests
  SuporteRemoto.Domain.Tests
  SuporteRemoto.Application.Tests
```

Api, Agent e Viewer se conectam a dois hubs SignalR:
- `/hubs/chat` — sinalização de chat (vinculável a um ticket)
- `/hubs/remote-session` — sinalização de sessões de acesso remoto

Entidades principais (`Ticket`, `ChatThread`, `RemoteSession`) já têm `TenantId` (nullable) para
não travar uma futura oferta multi-tenant, sem implementar isso agora.

## Ambiente de produção (Render + Aiven)

A Api e a Web estão publicadas e acessíveis pela internet:
- Api: https://suporte-remoto.onrender.com
- Web: https://suporte-remoto-web.onrender.com

Deploy via [Render](https://render.com) (Web Services em Docker, build a partir de
[`src/SuporteRemoto.Api/Dockerfile`](src/SuporteRemoto.Api/Dockerfile) e
[`src/SuporteRemoto.Web/Dockerfile`](src/SuporteRemoto.Web/Dockerfile)) + banco
[Aiven](https://aiven.io) MySQL gerenciado. Segredos (connection string, `Jwt:Key`) ficam só nas
variáveis de ambiente do Render, nunca em arquivo versionado. Testado ponta a ponta: registro,
login, criar ticket, e chat em tempo real — tudo confirmado direto nas URLs públicas.

Planos free do Render dormem depois de um tempo sem uso — a primeira requisição depois disso pode
demorar ~30s pra "acordar" o serviço.

**Pegadinha resolvida**: containers com limite baixo de `inotify` (comum em planos free de PaaS)
derrubavam o app na inicialização, porque o .NET tenta observar `appsettings.json` pra hot-reload
por padrão. Corrigido desativando isso via `DOTNET_hostBuilder__reloadConfigOnChange=false` nos
Dockerfiles — vale a pena saber se for reproduzir esse deploy em outro provedor parecido.

## Pré-requisitos

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- MySQL Server (o projeto usa o provider [Pomelo.EntityFrameworkCore.MySql](https://github.com/PomeloFoundation/Pomelo.EntityFrameworkCore.MySql))
- Ferramenta `dotnet-ef` (`dotnet tool install --global dotnet-ef`)

## Banco de dados (MySQL)

Nesta máquina de desenvolvimento o MySQL foi instalado localmente (sem privilégios de admin, por
isso não roda como serviço do Windows — precisa ser iniciado manualmente a cada reinício):

```bash
"C:\Program Files\MySQL\MySQL Server 8.4\bin\mysqld.exe" --basedir="C:\Program Files\MySQL\MySQL Server 8.4" --datadir="C:\Users\Usuario\mysql-data" --port=3306
```

Banco e usuário já criados:
- Database: `suporte_remoto_db`
- Usuário da aplicação: `suporte_remoto_app` / senha `SuporteRemoto_App_2026!`
- Root: senha `SuporteRemoto_Root_2026!`

A connection string está em [`src/SuporteRemoto.Api/appsettings.Development.json`](src/SuporteRemoto.Api/appsettings.Development.json).
Para produção, troque host/porta/credenciais lá (ou via variável de ambiente /
`dotnet user-secrets`) — nenhum código muda.

Para aplicar as migrations:

```bash
dotnet ef database update --project src/SuporteRemoto.Infrastructure --startup-project src/SuporteRemoto.Api
```

## Rodando localmente

```bash
# Api (porta 5080) — sobe com Swagger em /swagger, aplica seed dos papéis (Admin, Tecnico, UsuarioFinal)
dotnet run --project src/SuporteRemoto.Api --urls http://localhost:5080

# Web (porta 5090) — Blazor Server, aponta para a Api via appsettings (ApiBaseUrl)
dotnet run --project src/SuporteRemoto.Web --urls http://localhost:5090

# RemoteAgent — conecta ao hub de sessão remota e fica anunciando presença
dotnet run --project src/SuporteRemoto.RemoteAgent

# RemoteViewer — app WPF do técnico (exige sessão desktop do Windows para abrir a janela)
dotnet run --project src/SuporteRemoto.RemoteViewer
```

Fluxo testado ponta a ponta com dois usuários (`UsuarioFinal` e `Tecnico`): registro → login (JWT)
→ criar ticket → listagem respeita papel (`UsuarioFinal` só vê os próprios, `Tecnico`/`Admin` veem
todos) → "Pegar ticket" atribui e avança o status → comentários e anexos (upload/download) → chat
embutido em tempo real via `ChatHub`, testado com dois clientes SignalR simultâneos (mensagem
persistida e recebida por ambos) → acesso a ticket de outro usuário final retorna 404.

## Decisões relevantes desta etapa

- **Render mode do Blazor**: o modo interativo (`InteractiveServer`) é declarado uma única vez, em
  [`App.razor`](src/SuporteRemoto.Web/Components/App.razor) (`<Routes @rendermode="InteractiveServer" />`),
  em vez de por página. Declarar por página faz o Blazor tratar cada página como uma "ilha"
  independente e recriar o circuito a cada navegação — o que derrubava o estado de login guardado
  em memória (`AuthState`). Manter um único circuito por sessão é o que faz login → navegação para
  `/tickets` funcionar sem perder o token.
- **Autenticação na Web**: o JWT fica em memória (`AuthState`, escopo por circuito), suficiente
  para validar o fluxo ponta a ponta. Não sobrevive a um F5 na página — trocar para cookie
  persistente é trabalho do módulo de auth quando for aprofundado.
- **MySQL em vez de SQL Server**: trocado a pedido, usando o provider Pomelo. Nenhuma outra camada
  precisou mudar.
- **Chat embutido, não módulo solto**: `ChatThread` é 1:1 com `Ticket` (criado sob demanda na
  primeira entrada/mensagem). O `ChatHub` exige JWT (`[Authorize]`) e reaplica a mesma regra de
  acesso do `TicketsController` (solicitante, técnico ou Admin).
- **Token via query string para hub e download de anexo**: WebSocket (SignalR) e o link direto de
  download de anexo não conseguem mandar header `Authorization`, então a Api aceita o JWT também
  via `?access_token=` (ver `OnMessageReceived` em `Program.cs`). Isso expõe o token na URL
  (histórico do navegador, logs) — aceitável para este estágio interno, mas normalmente seria
  trocado por um token de download de vida curta antes de ir para produção.
- **Nomes de usuário resolvidos na Api**: `Ticket`/`TicketComment`/`ChatMessage` só guardam Guid
  (Domain não referencia Identity de propósito). A Api injeta `UserManager<ApplicationUser>`
  direto nos controllers/hub pra resolver nomes ao montar os DTOs — sem abstração nova pra isso.
- **Chat confirmado no navegador**: testado com dois usuários reais em abas separadas
  (`joao@teste.com` e `carla@teste.com`) no ticket `/tickets/{id}`. Mensagem enviada por um lado
  aparece na hora do outro lado, sem F5 — SignalR entregando em tempo real via `ChatHub`.

## Próximos passos (fora desta etapa)

1. Acesso remoto de verdade: captura e streaming de tela, injeção de input, NAT traversal — módulo
   de maior risco técnico, merece sessão de planejamento própria.
2. Autenticação persistente na Web (cookie) e Identity mais completo (recuperação de senha etc.).
3. Trocar o token de download de anexo por algo de vida curta (não o JWT de sessão inteiro na URL).
4. Anexos de ticket ficam em disco local dentro do container — no Render isso é efêmero (some a
   cada redeploy/restart). Trocar por armazenamento externo (S3-compatível) quando o módulo de
   tickets for revisitado.
