# LUDUS Unity SDK

SDK plugável para registrar telemetria de jogos educacionais Unity e gerar
sessões compatíveis com a plataforma **LUDUS Acompanha**.

O LUDUS Acompanha oferece evidências parciais para acompanhamento e mediação
docente. Ele não diagnostica, classifica clinicamente nem produz avaliação
conclusiva de aprendizagem.

> Esta é uma versão de avaliação. Antes de qualquer coleta, valide a integração
> com identidades fictícias, o fluxo do jogo e o build WebGL.

## Comece por aqui

O roteiro único de instalação, configuração, WebGL e importação no Dashboard
está em [Guia de integração end to end](Documentation~/GUIA_INTEGRACAO_END_TO_END.md).
Use esse documento para validar a experiência de uma pessoa desenvolvedora que
está conhecendo o SDK pela primeira vez.

## O que o SDK faz

- inicia e encerra sessões no contrato LUDUS;
- registra cliques e trajetória do mouse quando habilitados;
- acompanha cenas automaticamente;
- permite recortes extras por Canvas, painel ou atividade;
- gera um JSON por sessão;
- envia para `POST /api/sessions` quando há ambiente configurado;
- preserva fallback local quando está offline ou o envio falha.

O SDK **não** interpreta regras pedagógicas do jogo. Esta versão de avaliação
concentra-se nas interações observáveis e nos elementos explicitamente
acompanhados pela pessoa desenvolvedora.

## Requisitos

- Unity 6;
- acesso ao repositório GitHub do pacote;
- projeto configurado para o alvo que pretende publicar, especialmente WebGL.

O pacote usa o novo Input System quando ele está presente no projeto e o Input
clássico como alternativa. Não é necessário alterar `ProjectSettings` para a
integração básica.

## Instalação pelo GitHub

No projeto Unity que receberá o SDK:

1. Abra **Window > Package Manager**.
2. Clique no botão **+**.
3. Escolha **Install package from git URL...**.
4. Informe:

   ```text
   https://github.com/RodrigoBichet/LUDUS-Unity-SDK.git#v0.1.1
   ```

5. Aguarde a Unity importar o pacote.

Se o repositório estiver privado, a conta GitHub configurada no computador deve
ter acesso a ele. Não inclua tokens, senhas ou chaves em arquivos Unity.

## Primeiro teste: tutorial isolado

Antes de tocar no jogo, crie uma cena de demonstração separada:

```text
LUDUS > Criar tutorial de teste do SDK
```

O comando cria:

```text
Assets/LUDUS/Tutorial/
├── ConfiguracaoTutorialLudus.asset
└── TutorialLudus.unity
```

A cena contém uma base LUDUS e um painel visível somente no tutorial, com os
botões **Iniciar sessão fictícia** e **Encerrar sessão e exibir JSON**. Ela usa
uma identidade fictícia, salva a sessão localmente e, no WebGL, baixa um JSON
de demonstração; não altera as cenas do jogo.

Para validar no Editor:

1. Abra `Assets/LUDUS/Tutorial/TutorialLudus.unity`.
2. Pressione **Play**.
3. Clique em **Iniciar sessão fictícia**.
4. Mova o mouse e faça alguns cliques na Game View.
5. Clique em **Encerrar sessão e exibir JSON**.
6. Confira o JSON no Console e a cópia local em `ludus_offline`.

Para validar no WebGL, adicione `TutorialLudus` ao Build Profile como primeira
cena habilitada e use **Build And Run**. O JSON deve mostrar
`"platform":"WebGLPlayer"`.

Se quiser validar também a importação no Dashboard, informe no Inspector do
painel tutorial o ID de um estudante **fictício** já criado no ambiente
demonstrativo. O ID deve corresponder ao aluno escolhido no Dashboard.

> O painel do tutorial não é adicionado à integração real. Ele existe apenas
> para aprender e validar o pacote sem poluir a interface do jogo.

## Integração no jogo real

### 1. Adicione a base LUDUS

Abra a cena inicial do jogo e use:

```text
GameObject > LUDUS > Adicionar coleta ao meu jogo
```

O SDK cria o GameObject `LUDUS SDK`, uma configuração em `Assets/LUDUS/` e
conecta automaticamente:

- controlador de sessão;
- exportador;
- coletor de mouse compatível;
- coordenador de captura por cenas.

A base permanece ativa ao trocar de cena. Caso a cena inicial seja carregada
novamente, o SDK preserva a base original e evita uma duplicação de sessão.

### 2. Configure o jogo e as cenas

No painel **Project**, selecione o asset criado em `Assets/LUDUS/` e preencha:

- **Nome do jogo**, por exemplo `Historietas Divertidas`;
- **Capturar automaticamente em**:
  - **Todas as cenas**, para acompanhar o jogo inteiro;
  - **Somente cenas selecionadas**, para registrar apenas cenas marcadas no
    Build Profile.

No modo de cenas selecionadas, marque as cenas de atividade. Nas demais, a
sessão continua ativa, mas mouse e cliques ficam pausados. Ao voltar para uma
cena marcada, a coleta retoma automaticamente.

O nome técnico `gameId` é gerado internamente a partir do nome do jogo. Não é
necessário preenchê-lo.

### 3. Inicie e encerre no fluxo do jogo

O jogo é quem sabe quando um estudante foi identificado e quando a atividade
terminou. Conecte estes dois momentos ao fluxo existente do jogo:

```csharp
using LudusSDK;
using UnityEngine;

public sealed class MeuFluxoLudus : MonoBehaviour
{
    public void IniciarSessaoParaImportarDepois()
    {
        if (!LudusSdk.TryStartSessionForManualImport("Atividade 1", out string erro))
        {
            Debug.LogError("[LUDUS] " + erro);
        }
    }

    public void EncerrarSessao()
    {
        if (!LudusSdk.TryEndSession(out string json, out string erro))
        {
            Debug.LogError("[LUDUS] " + erro);
            return;
        }

        Debug.Log("[LUDUS] Sessão encerrada: " + json);
    }
}
```

Chame `IniciarSessaoParaImportarDepois` antes da primeira
cena acompanhada. Chame `EncerrarSessao` quando a atividade for concluída,
cancelada ou quando o fluxo pedagógico determinar o fim da sessão.

No fluxo de importação manual, o desenvolvedor não informa `studentId` na
Unity. O SDK usa internamente uma marca de sessão sem vínculo, e o Dashboard
associa o JSON ao aluno escolhido na importação. Nunca fixe identificadores
reais em scripts, cenas, prefabs, exemplos ou testes.

Para envio automático à API, use `TryStartSession(studentId, playerId, ...)`
somente quando o jogo já receber uma identidade técnica por um fluxo seguro
externo. JWTs de usuário não pertencem ao build Unity.

### 4. Recortes extras por Canvas ou painel (opcional)

A captura por cena cobre o caso comum. Quando uma mesma cena possui vários
recortes relevantes, adicione `LudusCaptureContextTrigger` ao Canvas, painel
ou objeto-raiz desejado.

No Inspector, preencha em linguagem do seu jogo:

- **Título exibido no acompanhamento**;
- **Tipo deste recorte**;
- **Objetivo deste recorte**.

O SDK encontra a base automaticamente. Só use a referência manual se o jogo
tiver mais de uma base LUDUS, situação que normalmente deve ser evitada.

## Envio, fallback e privacidade

Em **Conexão com ambiente LUDUS (avançado)**, informe a URL somente quando ela
for fornecida pelo ambiente oficial LUDUS ou por um backend local controlado.
Use apenas a origem, sem `/api` no final.

- **Enviar sessões para LUDUS Acompanha** envia ao encerrar, quando existe URL.
- **Salvar também uma cópia local** mantém um arquivo local mesmo se o envio
  funcionar.
- **Baixar arquivo JSON ao encerrar (WebGL)** solicita um download normal no
  navegador, pronto para a importação manual no Dashboard. A pasta de destino
  é definida pelo navegador e pelo sistema operacional, portanto funciona em
  Windows, macOS e Linux.
- **Rótulo do arquivo (opcional)** organiza o nome do download. Com o rótulo
  `atividade 1`, por exemplo, o navegador recebe
  `ludus-nome-do-jogo-atividade-1-2026-08-04_14-32-10.json`. Sem rótulo, o
  SDK usa nome do jogo e data/hora de encerramento. O nome nunca inclui aluno.
- Se não houver URL, internet ou resposta válida, o fallback local é usado
  automaticamente quando habilitado.

No WebGL, o fallback aparece em um caminho semelhante a:

```text
/idbfs/.../ludus_offline/<sessionId>.json
```

Esse caminho representa o armazenamento local do navegador.

Para importar manualmente uma sessão WebGL, marque a opção de baixar JSON,
encerre a sessão e use o arquivo `ludus-<jogo>-<data-hora>.json` em:

```text
Dashboard > Perfil do aluno > Importar JSON > Validar prévia > Confirmar importação
```

No Dashboard, selecione o aluno que deve receber a sessão antes de importar.
O Dashboard registra o `studentId` canônico desse aluno, valida o arquivo e
impede duplicação antes de gravar. Por segurança, um JSON que já tenha um
`studentId` técnico diferente continua sendo recusado.

Não coloque JWT de usuário no Unity. O endpoint direto de telemetria permanece
sem JWT por compatibilidade nesta etapa; uma credencial específica do SDK será
tratada separadamente.

Para testes, use somente estudantes fictícios, backend local ou ambiente
demonstrativo seguro. Nunca use dados reais, MongoDB Atlas produtivo, tokens,
senhas ou credenciais.

## Build WebGL

Antes de publicar:

1. Abra **File > Build Profiles**.
2. Selecione **Web** como plataforma ativa.
3. Confirme que as cenas corretas estão habilitadas e na ordem esperada.
4. Execute **Build And Run**.
5. Faça ao menos um clique, movimento, início e encerramento de sessão.
6. Confira no Console do navegador `platform: "WebGLPlayer"` e os eventos de
   contexto esperados.

O primeiro build WebGL — e builds após mudar de plataforma — pode levar mais
tempo porque a Unity compila e prepara os artefatos do jogador. Consulte a
documentação oficial sobre [introdução ao processo de build](https://docs.unity3d.com/6000.0/Documentation/Manual/building-introduction.html)
e sobre [build para Web](https://docs.unity3d.com/6000.0/Documentation/Manual/webgl-building.html).

## Solução de problemas

### O Console mostra aviso sobre posição do ponteiro no Editor

O novo Input System pode ainda não fornecer uma posição válida no primeiro
instante do Editor. O SDK evita registrar o ponto falso `(0,0)` e usa a Game
View como compatibilidade. Os cliques e movimentos reais continuam sendo
capturados.

### A sessão foi salva localmente

Isso é esperado no tutorial, offline, sem URL configurada ou quando o envio
falha. Confira o JSON antes de configurar um servidor.

### Foram encontradas várias bases LUDUS SDK ativas

Mantenha uma base por jogo. A mensagem indica os objetos e cenas encontrados.
Se isso ocorrer ao recarregar uma cena inicial, confirme que o pacote está
atualizado: a base persistente deve preservar a cópia original.

### A Unity não atualizou o pacote Git

Abra o Package Manager, selecione **LUDUS Unity SDK** e clique em **Update**.
Confirme também que o commit foi enviado ao GitHub e que sua conta tem acesso
ao repositório.

## Checklist antes de integrar um jogo

- [ ] Pacote instalado pelo GitHub.
- [ ] Tutorial isolado validado no Editor.
- [ ] Tutorial isolado validado no WebGL.
- [ ] Nome do jogo preenchido.
- [ ] Cenas acompanhadas configuradas.
- [ ] Início e encerramento ligados ao fluxo do jogo.
- [ ] Apenas identidade fictícia usada nos testes.
- [ ] JSON conferido.
- [ ] Fallback offline conferido.
- [ ] Build WebGL conferido.

## Contrato de telemetria

O SDK preserva o contrato LUDUS, incluindo `schemaVersion`, `captureMode`,
`source`, `capabilities`, `studentId`, `playerId`, `gameId`, métricas,
cliques, trajetórias e eventos. Campos reservados pelo contrato permanecem
desativados quando não possuem coleta nesta versão. O modo deste pacote é
`"captureMode":"sdk"`.

Mudanças no payload devem ser avaliadas junto do backend e Dashboard LUDUS
Acompanha para preservar compatibilidade com sessões existentes.
