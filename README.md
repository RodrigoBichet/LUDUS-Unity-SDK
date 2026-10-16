# LUDUS Unity SDK

O LUDUS funciona em duas partes que se complementam:

1. o **LUDUS Unity SDK** é instalado dentro do projeto Unity e registra o que
   acontece durante a jogada;
2. o site **LUDUS Acompanha** recebe a sessão e organiza os registros no
   Dashboard, facilitando a visualização de cliques, trajetórias, arrastes,
   duração e outros dados que o jogo disponibilizar.

```text
Jogo Unity + SDK  →  sessão da jogada  →  LUDUS Acompanha  →  Dashboard
```

O primeiro teste não exige configurar manualmente cenas, componentes ou regras
do jogo. Depois de instalar o pacote, use **LUDUS > Conectar ao Dashboard
(Etapa 1)**, conecte o jogo a uma coleta de teste e já visualize o resultado no
Dashboard.

> **Prefere acompanhar com imagens e navegação visual?** Abra a
> [documentação do SDK no site LUDUS Acompanha](https://acompanha.maisludus.com.br/documentacao).

O LUDUS Acompanha oferece evidências parciais para acompanhamento e mediação
docente. Ele não diagnostica, classifica clinicamente nem produz avaliação
conclusiva de aprendizagem.

> Esta é uma versão de avaliação. Antes de qualquer coleta, valide a integração
> com identidades fictícias, o fluxo do jogo e o build WebGL.

## Avance por etapas e veja o resultado crescer

### Etapa 1 — Conectar e visualizar

Instale o SDK, conecte o jogo por nome e código, jogue, finalize a sessão e
confira o resultado no Dashboard. O SDK registra automaticamente duração,
cliques, trajetória do ponteiro e arrastes, sem exigir configuração manual na
Unity.

### Etapa 2 — Dar contexto ao mapa

Acrescente uma captura visual para exibir cliques, movimentos e arrastes sobre
a própria tela do jogo. Quando fizer sentido, escolha cenas específicas e
organize os recortes apresentados no mapa.

### Etapa 3 — Integrar regras do jogo

Para uma sessão mais detalhada, conecte início e encerramento aos momentos reais
da atividade e informe eventos que somente o jogo conhece, como acertos, erros,
fases, alternativas e metas. O SDK não tenta inferir sozinho essas regras.

### Tutoriais em vídeo

Os vídeos não possuem narração e podem ser pausados, avançados ou reproduzidos
em outra velocidade pelo controle do navegador.

[![Tutorial da Etapa 1: do pacote ao primeiro resultado no Dashboard](https://acompanha.maisludus.com.br/documentacao/videos/tutorial-etapa-simples-capa.jpg)](https://acompanha.maisludus.com.br/documentacao/videos/tutorial-etapa-simples.mp4)

**Tutorial da Etapa 1:** instalação do pacote, ativação do modo simples,
criação da coleta, conexão do jogo e visualização dos primeiros dados no
Dashboard.

[![Tutorial da Etapa 2: adicione a tela do jogo ao mapa](https://acompanha.maisludus.com.br/documentacao/videos/tutorial-etapa-2-capa.jpg)](https://acompanha.maisludus.com.br/documentacao/videos/tutorial-etapa-2.mp4)

**Tutorial da Etapa 2:** escolha das cenas acompanhadas, ativação das capturas
visuais e conferência de cliques, movimentos e arrastes sobre a imagem do jogo
no Dashboard.

[![Tutorial completo do SDK e do Dashboard](https://acompanha.maisludus.com.br/documentacao/videos/tutorial-sdk-dashboard-capa.jpg)](https://acompanha.maisludus.com.br/documentacao/videos/tutorial-sdk-dashboard.mp4)

**Tutorial completo do SDK e do Dashboard:** instalação, configuração,
pareamento por nome e código e conferência da sessão recebida.

[![Aplicação do SDK no jogo Para Que Serve?](https://acompanha.maisludus.com.br/documentacao/videos/tutorial-sdk-para-que-serve-capa.jpg)](https://acompanha.maisludus.com.br/documentacao/videos/tutorial-sdk-para-que-serve.mp4)

**Aplicação no jogo Para Que Serve?:** acompanhamento somente da cena `Fase00`
e integração de uma mecânica de arraste avaliada por tag.

### Resumo visual da integração

| 1. Instalar o pacote | 2. Preparar as interações |
|---|---|
| [![SDK instalado no Package Manager](https://acompanha.maisludus.com.br/documentacao/package-manager/03-sdk-instalado.png)](https://acompanha.maisludus.com.br/documentacao/package-manager/03-sdk-instalado.png) | [![Objetos preparados na Hierarchy](https://acompanha.maisludus.com.br/documentacao/interacoes/01-objetos-hierarchy.png)](https://acompanha.maisludus.com.br/documentacao/interacoes/01-objetos-hierarchy.png) |

| 3. Conectar nome e código | 4. Conferir o recebimento |
|---|---|
| [![Conexão com a coleta confirmada](https://acompanha.maisludus.com.br/documentacao/acesso-coleta/04-conexao-confirmada.png)](https://acompanha.maisludus.com.br/documentacao/acesso-coleta/04-conexao-confirmada.png) | [![Sessão recebida no Dashboard](https://acompanha.maisludus.com.br/documentacao/recebimentos/01-recebimento-pendente.png)](https://acompanha.maisludus.com.br/documentacao/recebimentos/01-recebimento-pendente.png) |

A sequência completa, com instruções e ampliação de cada captura, permanece na
[documentação visual do LUDUS Acompanha](https://acompanha.maisludus.com.br/documentacao).

## Caminho recomendado

Comece pela **Etapa 1** para obter rapidamente um resultado no Dashboard. A
**Etapa 2** acrescenta contexto visual ao mapa e a **Etapa 3** permite informar
dados que somente o jogo conhece. O
[Guia de integração end to end](Documentation~/GUIA_INTEGRACAO_END_TO_END.md)
permanece como referência detalhada para as etapas 2 e 3, incluindo o
laboratório guiado, WebGL, eventos semânticos e validações adicionais.

## O que o SDK faz

- inicia e encerra sessões no contrato LUDUS;
- registra automaticamente cliques, trajetória do ponteiro e arrastes no modo
  simples;
- acompanha cenas automaticamente;
- permite recortes extras por Canvas, painel ou atividade;
- captura imagens JPEG opcionais para contextualizar o mapa de interações;
- gera um JSON por sessão;
- envia pela coleta pareada ou por `POST /api/sessions` quando há ambiente
  configurado;
- preserva fallback local quando está offline ou o envio falha.

O SDK **não** interpreta regras pedagógicas do jogo. Esta versão de avaliação
concentra-se nas interações observáveis e nos elementos explicitamente
acompanhados pela pessoa desenvolvedora.

Para uso em turma, o desenvolvedor pode executar **GameObject > LUDUS >
Adicionar acesso por nome e código** na cena de menu. O comando cria um botão e
um modal funcionais na raiz da cena. Depois, o desenvolvedor arrasta o objeto
inteiro para o Canvas visual escolhido. O contêiner ocupa automaticamente esse
Canvas com escala `1`; para escolher tamanho e posição, edite somente o filho
**Abrir acesso**. Por padrão, ele aparece como um botão azul compacto com `L`
branco, seguindo a identidade do LUDUS Observa. A criança informa apenas nome e
código temporário. Escola, turma e professora já pertencem à coleta criada no
Dashboard e não devem ser gravadas no projeto Unity.

O painel usa fundos arredondados vetoriais do próprio SDK, sem exigir sprites do
jogo. Quando encontra `Roboto-Regular` no projeto, o assistente usa essa fonte;
caso contrário, aplica automaticamente a fonte padrão da Unity.

## Requisitos

- Unity 6;
- acesso ao repositório GitHub do pacote;
- projeto configurado para o alvo que pretende publicar, especialmente WebGL.

O roteiro visual desta versão foi validado na **Unity 6.3 LTS
(6000.3.25f1)**. Em outras versões, nomes e posições de menus podem variar. Por
exemplo, nessa versão a criação de elementos de interface aparece no menu de
contexto da Hierarchy como **UI (Canvas)**.

O pacote usa o novo Input System quando ele está presente no projeto e o Input
clássico como alternativa. Não é necessário alterar `ProjectSettings` para a
integração básica.

## Etapa 1: conectar e visualizar no Dashboard

### 1. Instale o pacote no projeto Unity

No projeto Unity que receberá o SDK:

1. Abra **Window > Package Manager**.
2. Clique no botão **+**.
3. Escolha **Install package from git URL...**.
4. Informe:

   ```text
   https://github.com/RodrigoBichet/LUDUS-Unity-SDK.git#main
   ```

5. Aguarde a Unity importar o pacote.

Para inspecionar o código fora do Package Manager, baixe o ZIP do branch atual:

```text
https://github.com/RodrigoBichet/LUDUS-Unity-SDK/archive/refs/heads/main.zip
```

O ZIP é uma alternativa de consulta e instalação manual. Para receber
atualizações pelo Package Manager, prefira a URL Git acima.

Se o repositório estiver privado, a conta GitHub configurada no computador deve
ter acesso a ele. Não inclua tokens, senhas ou chaves em arquivos Unity.

### 2. Ative o modo simples e abra o acesso

Na Unity, abra a cena que é carregada primeiro quando a pessoa inicia o jogo —
normalmente o menu ou a tela inicial — e execute:

```text
LUDUS > Conectar ao Dashboard (Etapa 1)
```

Salve a cena e pressione **Play**. Quando aparecer a tela que solicita o nome do
participante e o código da coleta, mantenha-a aberta e siga para o próximo
passo.

### 3. Prepare o laboratório e crie uma coleta de teste

Com essa tela do jogo aberta, volte ao
[LUDUS Acompanha](https://acompanha.maisludus.com.br/inicio) e abra a
**Visão geral** do Laboratório. Se este for o primeiro acesso, clique em
**Criar laboratório de testes** e aguarde a preparação do ambiente. Depois,
abra **Coletas de teste**, crie uma coleta e copie o código temporário exibido.

### 4. Conecte e jogue normalmente

Volte à Unity. Para evitar confusão entre pessoas com o mesmo nome, informe
também o sobrenome ou a inicial — como **“João Silva”** ou **“João S”** —,
digite o código da coleta e clique em **Começar**. Após a mensagem de sucesso,
o SDK estará conectado ao Dashboard. Jogue normalmente: a atividade será
registrada automaticamente.

### 5. Veja o resultado no Dashboard

Abra a coleta no Dashboard e clique em **Ver dados recebidos**. Sessões
concluídas oferecem **Ver dados do aluno**; se o jogo foi fechado antes da
finalização, o último checkpoint pode ser aberto em **Ver progresso salvo**.

O botão **L** pode ser reposicionado e redimensionado. Na Hierarchy, selecione
o objeto `Abrir acesso` dentro da interface criada pelo modo simples, ajuste a
posição e o tamanho e deixe-o do jeito que preferir na tela.
No modo simples, ele aparece apenas na cena inicial de conexão. A coleta
continua ativa nas demais cenas e o botão reaparece se o jogo voltar ao menu.

O progresso parcial é verificado a cada 3 segundos e reenviado somente quando
existirem novos dados. Pontos repetidos do mouse parado não são armazenados.

Esse caminho já configura a base persistente, a conexão e o envio. Não crie
outra base LUDUS e não adicione componentes manualmente antes de confirmar esse
primeiro resultado.

## Etapa 2: dar contexto visual ao mapa

O mapa da Etapa 1 funciona sobre uma área neutra. Para exibir as interações
sobre a imagem do jogo, pare o Play e execute:

```text
LUDUS > Adicionar imagens ao mapa (Etapa 2)
```

Uma janela própria será aberta. Use todas as cenas acompanhadas ou desmarque a
opção recomendada para selecionar cenas individualmente. A lista manual reúne
as cenas cadastradas em **File > Build Profiles** e também a cena aberta no
momento. Se uma cena exibir senha, perfil ou outra informação pessoal que você
não queira registrar, deixe-a desmarcada. Depois da escolha, clique em
**Ativar imagens no mapa**.

Pressione Play, conecte o jogo à coleta e jogue normalmente. A primeira
interação relevante prepara a imagem, que entra automaticamente no próximo
salvamento parcial. O recomendado ainda é voltar à tela inicial ao terminar a
jogada, abrir o botão **L** e clicar em **Finalizar e enviar** para marcar a
sessão como concluída no Dashboard.

A seleção pode ser revisada a qualquer momento pelo mesmo menu. Ao criar uma
cena nova, adicione-a em **File > Build Profiles** e abra novamente a Etapa 2
para conferir se ela deve ou não receber imagens.

## Tutoriais e laboratório guiado (opcional)

Para conhecer configurações e interações adicionais sem alterar as cenas do
jogo, crie uma demonstração separada:

```text
LUDUS > Criar tutorial de teste do SDK
```

O comando cria:

```text
Assets/LUDUS/Tutorial/
├── ConfiguracaoTutorialLudus.asset
└── TutorialLudus.unity
```

A cena contém uma base LUDUS e um laboratório guiado. O fluxo principal usa o
botão **L** para conectar o nome do participante ao código de uma coleta; ao
concluir o sexto cenário, a sessão é encerrada e enviada automaticamente. O
painel de início e exportação manual de JSON fica oculto por padrão e pode ser
ativado no componente `Ludus Tutorial Test Panel` somente para testes offline.

Para validar no Editor:

1. Abra `Assets/LUDUS/Tutorial/TutorialLudus.unity`.
2. No Project, selecione
   `Assets/LUDUS/Tutorial/ConfiguracaoTutorialLudus.asset` e observe no
   Inspector que o tutorial usa o nome **Tutorial LUDUS**. Escolha **Enviar
   automaticamente ao LUDUS Acompanha**. Esse asset pertence somente ao
   tutorial; não o transforme na configuração do jogo real.
3. Use **LUDUS > Tutorial > Adicionar exercício de
   interações** para criar um laboratório com seis páginas e uma tela final:
   interações automáticas, um miniquiz com alternativas e imagem, arraste com
   destino, contato por física, meta por valor e ciclo guiado de categoria/fase.
   O gerador cria a tag
   exclusiva `LARespostaCorretaTutorial` para os exemplos, sem reutilizar a
   tag global `Player` do projeto.
4. Se adicionou o exercício, abra **LUDUS > Configurar interações desta cena**
   e, na página **01 — Interações automáticas**, arraste `BotaoTutorial`,
   `CampoTextoTutorial` e `PecaArrastavelTutorial` da Hierarchy para o campo
   **Arraste um objeto aqui** da janela **Interações LUDUS**.
5. Execute **GameObject > LUDUS > Adicionar acesso por nome e código** e gere
   uma coleta de teste no Dashboard.
6. Pressione **Play**, use o botão **L** e informe o nome de um participante e
   o código da coleta. O tutorial já usa **Enviar automaticamente ao LUDUS
   Acompanha**. Se quiser validar a plataforma final, faça também um **Build And
   Run** WebGL; selecione **Desenvolvimento: enviar e também baixar** somente se
   quiser receber uma cópia JSON durante esse teste opcional.
7. Use **Anterior** e **Próximo** no laboratório e realize as interações
   configuradas em cada página. **Próximo** permanece bloqueado até a tarefa
   atual ser concluída; a orientação acima dos botões informa o que falta, e
   **Anterior** permite revisar o que já foi feito. No miniquiz, responda
   `1 + 1`, depois identifique a imagem com duas formas. No ciclo semântico,
   execute somente a ação liberada e acompanhe o progresso visual. Depois da
   sexta página, clique em **Próximo** para abrir **Tutorial concluído**.
8. Ao abrir **Tutorial concluído**, aguarde a confirmação de que o envio foi
   iniciado automaticamente e confira a coleta no Dashboard.

No primeiro build WebGL, deixe somente `TutorialLudus` habilitada no índice 0
do **Build Profile**. Depois do teste, retire-a do perfil e habilite novamente
as cenas originais antes de publicar o jogo.

O roteiro completo — incluindo resultado semântico de botão, validação,
importação manual, laboratório fictício, código de coleta e recebimento
automático no Dashboard — está em
[`Documentation~/GUIA_INTEGRACAO_END_TO_END.md`](Documentation~/GUIA_INTEGRACAO_END_TO_END.md).

Ao integrar o SDK ao jogo real, o comando **GameObject > LUDUS > Adicionar
coleta ao meu jogo** cria outro asset em `Assets/LUDUS/ConfiguracaoLudus.asset` — ou um nome
numerado equivalente se já existir. Nesse novo asset, informe o nome do jogo,
escolha as cenas acompanhadas e configure a forma de entrega adequada. Assim, a
pessoa desenvolvedora pratica primeiro com dados fictícios e depois repete o
mesmo conceito sem reutilizar a configuração do tutorial.

Se o Play Mode for interrompido por engano enquanto uma sessão do tutorial
estiver ativa, o componente tutorial tenta encerrá-la como proteção contra
perda. No jogo real, conecte o início e o encerramento aos momentos verdadeiros
da atividade.

Ao adicionar uma interação, o SDK preenche inicialmente **Nome exibido no
dashboard** com o nome do GameObject. Esse texto pode ser alterado para ficar
mais legível sem renomear o objeto na Hierarchy e sem modificar o funcionamento
do jogo.

Para objetos arrastáveis, marque o GameObject que realmente recebe o gesto e se
move. Não marque apenas a área de destino ou o painel que contém a peça. Se o
SDK detectar que o componente de arraste está em um filho, a janela oferece a
opção de acompanhar esse filho automaticamente.

A peça do exercício se movimenta porque recebe um componente exclusivamente
didático. O movimento visual continua sendo responsabilidade do jogo real; o
componente `LudusTrackedDraggable` observa e registra o gesto, sem alterar as
regras ou a posição dos objetos do jogo.

O painel do exercício fica centralizado. A cena também recebe uma câmera neutra
para a Game View não exibir `No cameras rendering`.

Para validar no WebGL, adicione `TutorialLudus` ao Build Profile como primeira
cena habilitada e use **Build And Run**. Depois do pareamento e da conclusão,
confirme o participante e a sessão na coleta usada.

Se quiser validar também a importação manual no Dashboard, marque **Exibir
painel manual de JSON** no componente `Ludus Tutorial Test Panel`, gere um JSON
fictício e use somente um ambiente demonstrativo autorizado.

> O painel do tutorial não é adicionado à integração real. Ele existe apenas
> para aprender e validar o pacote sem poluir a interface do jogo.

## Etapa 3: integração detalhada no jogo

### 1. Adicione a base LUDUS

Se você concluiu a Etapa 1, reutilize o GameObject
`LUDUS SDK — Modo simples` e o asset criado em `Assets/LUDUS/`. Não execute
outro comando de criação, pois deve existir somente uma base persistente.

Se ainda não usou o modo simples, abra a cena inicial do jogo e use:

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

### 2. Abra e configure o asset do jogo

Depois que o assistente terminar:

1. selecione o GameObject **LUDUS SDK** na **Hierarchy**;
2. no componente **Ludus Session Controller**, localize **Configuração do jogo**;
3. clique na referência `ConfiguracaoLudus` para destacá-la no painel
   **Project**;
4. selecione o asset destacado em `Assets/LUDUS/ConfiguracaoLudus.asset`.

O Inspector desse asset concentra as escolhas da integração:

- **Nome do jogo**: nome legível que aparecerá no LUDUS Acompanha. O `gameId`
  técnico é gerado internamente e não precisa ser digitado;
- **Destino dos dados**:
  - **Enviar automaticamente ao LUDUS Acompanha**: envia sessões e checkpoints
    para a API. É o modo indicado quando estudantes usarão nome e código de
    coleta;
  - **Exportar JSON manualmente**: não envia à plataforma. A execução fica
    disponível para exportação e posterior importação em um ambiente
    autorizado do Dashboard;
  - **Desenvolvimento: enviar e também baixar**: envia à plataforma e também
    oferece o JSON para inspeção. Use apenas durante testes, pois pode gerar
    downloads extras;
- **Guardar também uma cópia de segurança local**: mantém um fallback local
  quando o ambiente permitir;
- **Salvar progresso parcial durante a atividade**: envia checkpoints sem
  encerrar a sessão, protegendo parte do andamento em execuções longas;
- **Rótulo do arquivo (opcional)**: personaliza o nome do arquivo nos modos que
  permitem exportação;
- **Conexão com ambiente LUDUS (avançado)**: mostra a origem oficial da API,
  preenchida automaticamente em novos assets. Abra essa opção somente para
  conferir ou trocar o ambiente; informe apenas a origem, sem `/api`,
  credenciais ou tokens;
- **Capturar automaticamente em**:
  - **Todas as cenas**, para acompanhar o jogo inteiro;
  - **Somente cenas selecionadas**, para registrar apenas cenas marcadas no
    Build Profile;
- **Interações automáticas**: habilitam cliques, trajetória do mouse e trajetória
  de arraste. Desative apenas o que não fizer sentido para o jogo;
- **Resultados informados pelo jogo**: categorias, fases, acertos, erros e eventos
  personalizados só devem ser habilitados quando a regra do jogo estiver ligada
  ao SDK pela Ponte semântica, por um adaptador ou pela API C#;
- **Capturas visuais**: registram uma imagem de referência opcional para sobrepor
  o mapa de interações;
- **Exibir mensagens detalhadas no Console**: ajuda durante a integração e pode
  ser desativado depois da validação.

No modo de cenas selecionadas, marque as cenas de atividade. Nas demais, a
sessão continua ativa, mas mouse e cliques ficam pausados. Ao voltar para uma
cena marcada, a coleta retoma automaticamente.

As **Capturas visuais** são opcionais e permanecem desativadas por padrão. Ao
habilitá-las, escolha se todas as cenas acompanhadas ou somente cenas marcadas
podem fornecer uma imagem para o mapa. O SDK prepara a captura quando o recorte
é aberto e registra a Game View completa após a primeira interação relevante,
evitando usar uma tela intermediária de carregamento como referência. Depois,
reduz a maior dimensão para 1280 px, compacta em JPEG com
qualidade 70 e conserva no máximo quatro imagens automáticas por sessão. Quando
existem mais recortes, prioriza os que receberam mais interações e usa o tempo
como desempate. Esses limites ficam internos para evitar configurações pesadas
ou acidentais.

Habilite imagens somente quando elas forem necessárias para interpretar as
interações e houver autorização adequada. Não use a captura em telas com dados
pessoais, credenciais, conversas ou outras informações sensíveis.

### 3. Inicie e encerre no fluxo do jogo

O jogo é quem sabe quando um estudante foi identificado e quando a atividade
terminou. Conecte estes dois momentos ao fluxo existente do jogo:

Sem escrever código, selecione um objeto que permaneça ativo durante toda a
atividade e, no **Inspector**, use **Add Component > LUDUS > Fluxo > Sessão
acompanhada**. Essa opção pertence ao seletor de componentes, não ao menu
superior **LUDUS**. O componente pode iniciar
ao ser ativado e encerrar ao ser desativado ou quando a cena for fechada.

Uma cena pode conter vários Canvas, painéis e etapas aleatórias: eles continuam
na mesma sessão enquanto o objeto do escopo permanecer ativo. Para atividades
que começam ou terminam sem ativar/desativar o objeto, ligue `IniciarSessao` e
`EncerrarSessao` aos `UnityEvent` correspondentes. Os eventos opcionais do
escopo também podem acionar uma Ponte semântica no início e antes do fim.

Se o jogo preferir fazer a integração por código, use a mesma API pública:

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

Quando o jogo conhece regras pedagógicas próprias, como categoria, fase,
acerto e erro, use a API tipada `LudusGameEvents`. Habilite no asset somente
as capacidades que o jogo realmente informa: **Eventos de fase**, **Acertos e
erros** e **Categorias**. O SDK rejeita o evento se a capacidade correspondente
estiver desligada.

```csharp
LudusGameEvents.TryCategorySelected("Alimentos", out _);
LudusGameEvents.TryPhaseStarted(
    "alimentos-1",
    "maçã",
    new[] { "maçã", "bola", "copo" },
    out _
);
LudusGameEvents.TryDragAttempt("bola", "maçã", false, out _);
LudusGameEvents.TryWrongMatch("bola", "maçã", out _);
LudusGameEvents.TryCorrectMatch("maçã", 4.5f, out _);
LudusGameEvents.TryPhaseCompleted(1, 1, 5f, 2, out _);
```

Esses métodos atualizam o contrato semântico e os totais de acerto e erro da
sessão. Use-os somente para fatos conhecidos pela própria regra do jogo. O SDK
não deduz desempenho pedagógico a partir de cliques, trajetórias ou imagens.

### Conectar eventos pelo Inspector, sem alterar scripts

Quando o jogo já expõe um `UnityEvent` para o resultado ou a progressão,
adicione **Add Component > LUDUS > Ponte semântica do jogo** a um objeto da atividade. Configure
uma ponte por atividade ou fase e, no evento existente, arraste esse objeto e
escolha um dos métodos públicos:

- `RegistrarCategoriaSelecionada`;
- `RegistrarInicioDeFase`;
- `RegistrarTentativaDeArrasteCorreta` ou
  `RegistrarTentativaDeArrasteIncorreta`;
- `RegistrarAcerto` ou `RegistrarErro`;
- `RegistrarConclusaoDeFase`.

A ponte mantém o cronômetro e os contadores desde o último início de fase. Em
uma tentativa de arraste, o mesmo `UnityEvent` pode chamar o método de tentativa
e depois o método de acerto ou erro. As capacidades correspondentes ainda
precisam estar habilitadas no asset de configuração LUDUS.

Esse componente não inspeciona scripts, tags ou pontuação para adivinhar o
resultado. Se o jogo não expõe um evento compatível, use a API C# acima ou um
adaptador específico. Sem uma dessas ligações, a sessão permanece válida com
somente as evidências observacionais disponíveis.

#### Adaptador de arraste por tag

Para cenas com vários destinos, prefira abrir **LUDUS > Configurar resultados
da cena**. O assistente localiza objetos que já recebem `OnDrop`, apresenta a
tag de cada destino para confirmação e configura uma única ponte compartilhada.
Ele também liga categoria, início e conclusão da fase ao escopo da sessão e
habilita as capacidades necessárias no asset LUDUS. Objetos `Untagged` são
exibidos como incompatíveis e não são configurados automaticamente.

Informe o nome da atividade ou categoria na própria janela. Se a cena ainda
não possuir uma **Sessão acompanhada**, o assistente cria esse objeto na raiz
automaticamente; se já existir uma, ele a reutiliza. Mais de um escopo na mesma
cena continua sendo tratado como ambíguo e precisa ser corrigido manualmente.

O botão **Aplicar configuração confirmada** adiciona somente componentes LUDUS
e pode ser executado novamente sem duplicar adaptadores ou ouvintes. A decisão
continua sendo do integrador: confirme apenas destinos em que a tag realmente
representa a resposta correta segundo a regra existente do jogo.

A própria janela oferece os botões **Anterior** e **Próxima** para percorrer as
cenas habilitadas no Build Profile. Quando houver alterações ainda não salvas,
o Unity pergunta se elas devem ser salvas, descartadas ou se a troca deve ser
cancelada. Ao abrir outra cena, o nome sugerido volta a ser o nome daquela cena
ou o nome pedagógico já salvo em sua Sessão acompanhada; revise esse texto antes
de aplicar. Cenas de menu, carregamento ou outras telas sem destinos compatíveis
podem simplesmente ser ignoradas no fluxo guiado.

Nas cenas compatíveis, **Aplicar e abrir próxima** configura somente os destinos
confirmados e então reutiliza o mesmo diálogo de salvamento antes de avançar.
Isso reduz a repetição entre várias atividades sem aplicar regras em lote e sem
eliminar a revisão do nome pedagógico e das tags de cada cena.

O assistente também marca o Canvas de cada destino confirmado como área de
observação. Em tempo de execução, a tag continua decidindo acerto ou erro, mas
o SDK usa prioritariamente o texto, a imagem ou o sprite visível para registrar
a peça arrastada, a resposta esperada e as alternativas daquele Canvas.

Para arrastes de UI que utilizam o `EventSystem`, adicione **Add Component >
LUDUS > Adaptadores > Resultado de arraste por tag** à área que recebe o drop. Selecione
a tag considerada correta e informe nomes legíveis para o destino e para a
resposta esperada. A Unity adiciona a Ponte semântica no mesmo GameObject
quando ela ainda não existe e reutiliza a instância presente quando o destino
já está configurado.

Quando a área recebe `OnDrop`, o adaptador compara a tag da peça indicada por
`pointerDrag` e pode registrar tanto `DragAttempt` quanto `CorrectMatch` ou
`WrongMatch`. Os nomes legíveis podem ficar vazios para serem resolvidos pelo
conteúdo visual ativo no momento da tentativa. Ele não move, reposiciona nem
devolve a peça e não substitui a lógica visual do jogo.

Esse adaptador cobre somente destinos que recebem `OnDrop` pelo `EventSystem`.
Arrastes implementados exclusivamente por colisão, trigger, raycast ou código
próprio precisam de outro adaptador ou da API C#.

#### Adaptador de contato por tag

Para jogos que usam física, adicione **Add Component > LUDUS > Adaptadores >
Resultado de contato por tag** ao objeto que recebe o contato. Escolha entre trigger ou
colisão 2D/3D, selecione a tag correta e indique a resposta esperada. A busca
opcional nos objetos pais cobre o caso comum em que o collider pertence a um
filho e a tag está no objeto principal.

O jogo continua responsável por `Collider`, `Rigidbody`, layers, matriz de
colisão e pela resposta visual. O adaptador somente observa o callback
selecionado e registra `CorrectMatch` ou `WrongMatch`.

#### Adaptador de alternativas por botão

Adicione **Add Component > LUDUS > Adaptadores > Resultado de botão** a uma alternativa com
`Button`. Marque-a como correta ou incorreta e informe os nomes da resposta
dada e esperada. O adaptador acrescenta um listener a `Button.onClick` sem
remover os listeners existentes do jogo.

#### Adaptador de meta por valor ou pontuação

Adicione **Add Component > LUDUS > Adaptadores > Meta por valor ou pontuação** ao controlador
da atividade. Configure a meta, a comparação e o evento semântico desejado.
Ligue os eventos já existentes no jogo a `AdicionarUm`, `SubtrairUm`,
`Adicionar`, `DefinirValor` ou `AvaliarAgora`.

O adaptador não procura placares ou campos privados por reflection. Um jogo que
não expõe mudança de pontuação por `UnityEvent` ainda precisa usar a API C# ou
um adaptador próprio. Por padrão, a meta é registrada somente uma vez até que
`ReiniciarMeta` ou `ReiniciarValorEMeta` seja chamado.

### Validar a integração semântica

Depois de configurar uma cena, abra **LUDUS > Validar integração semântica**.
A janela verifica se existe uma única base `LudusSessionController`, se ela tem
um asset de configuração e se as capacidades exigidas pelos adaptadores estão
habilitadas. As opções ficam em **Resultados informados pelo jogo** no asset
LUDUS. Cada problema ligado a um objeto oferece um botão para selecioná-lo.

Essa validação é estrutural: ela não executa a atividade, não interpreta a
regra pedagógica e não confirma que um `UnityEvent` manual representa realmente
um acerto ou erro. Essa ligação continua sendo revisada e testada pelo
desenvolvedor do jogo.

No fluxo de importação manual, o desenvolvedor não informa `studentId` na
Unity. O SDK usa internamente uma marca de sessão sem vínculo, e o Dashboard
associa o JSON ao aluno escolhido na importação. Nunca fixe identificadores
reais em scripts, cenas, prefabs, exemplos ou testes.

Para envio automático à API, use `TryStartSession(studentId, playerId, ...)`
somente quando o jogo já receber uma identidade técnica por um fluxo seguro
externo. JWTs de usuário não pertencem ao build Unity.

Os componentes de **Sessão acompanhada** também resolvem essa identidade de
forma genérica quando o WebGL é aberto com os parâmetros
`ludusStudentId` e `ludusPlayerId`. Isso permite que um Dashboard, portal ou
ambiente de teste inicie jogos diferentes sem gravar o aluno em cenas ou
prefabs. Exemplo fictício:

```text
https://jogo.exemplo/?ludusStudentId=000000000000000000000123&ludusPlayerId=Aluno%20Fict%C3%ADcio
```

Se o próprio jogo já possui identificação, ele pode informar a mesma
identidade em tempo de execução com
`LudusSdk.TrySetParticipant(studentId, playerId, out erro)`. No modo automático,
uma sessão sem participante identificado é bloqueada com uma mensagem clara,
em vez de enviar dados sem vínculo ao Dashboard. No modo manual, o fluxo de
importação posterior continua disponível.

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

Quando as capturas visuais estiverem habilitadas, o recorte pode ser marcado
como fundo do mapa. A imagem continua sendo a Game View completa; a marcação
serve para associá-la ao momento, Canvas, painel ou atividade correspondente.
Em cenas com uma única atividade, selecionar a cena já é suficiente.

## Envio, fallback e privacidade

Em **Conexão com ambiente LUDUS (avançado)**, informe a URL somente quando ela
for fornecida pelo ambiente oficial LUDUS ou por um backend local controlado.
Use apenas a origem, sem `/api` no final.

- **Enviar automaticamente ao LUDUS Acompanha** é o modo recomendado para
  turmas. Cada categoria é enviada ao encerrar e o SDK mantém checkpoints
  parciais leves durante a atividade, sem baixar JSON nos computadores dos
  jogadores. A proteção parcial funciona em segundo plano; o integrador não
  precisa escolher seu intervalo.
- **Exportar JSON manualmente** não envia à plataforma e baixa cada sessão no
  WebGL. Um botão opcional pode exportar toda a execução acumulada.
- **Desenvolvimento: enviar e também baixar** mantém os dois comportamentos
  apenas para validação técnica.
- **Salvar também uma cópia local** mantém um arquivo local mesmo se o envio
  funcionar.
- Nos dois destinos que incluem exportação, o WebGL solicita um download normal
  no navegador, pronto para a importação manual no Dashboard. A pasta de destino
  é definida pelo navegador e pelo sistema operacional.
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

O schema `1.1.0` separa três conceitos:

- `sessionId`: uma categoria ou tentativa individual;
- `runId`: a execução contínua do jogo, reunindo várias sessões;
- `attemptNumber`: a ordem das tentativas, inclusive repetições;
- `status`: `in_progress`, `completed` ou `interrupted`.

Para oferecer um botão pequeno dentro do próprio jogo, crie um `Button` no
Canvas, posicione-o como preferir e adicione o componente **Add Component >
LUDUS > Fluxo > Botão para exportar execução**. O arquivo gerado usa o formato
`ludus-session-bundle` e inclui tudo que já foi concluído, mais uma fotografia
da sessão atual sem encerrá-la. Por padrão o botão se oculta no modo de envio
automático, pois o professor pode baixar a execução pelo Dashboard.

Fechar ou voltar a aba não garante a execução de código JavaScript. Por isso,
o SDK não depende de um download nesse instante: no modo automático ele envia
checkpoints periódicos. Esses registros são evidências parciais e não devem
ser interpretados como conclusão da atividade.

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
disponível para portais que já fornecem uma identidade técnica. No fluxo por
nome+código, o backend emite uma credencial temporária limitada àquela coleta e
ao participante; o SDK a mantém somente em memória e a envia no cabeçalho das
rotas pareadas.

Para testes, use somente estudantes fictícios, backend local ou ambiente
demonstrativo seguro. Nunca use dados reais, MongoDB Atlas produtivo, tokens,
senhas ou credenciais.

## Build WebGL

Antes de publicar:

1. Abra **File > Build Profiles**.
2. Selecione **Web**. Se ainda não houver o selo **Active**, use **Switch
   Profile** — algumas versões exibem **Switch Platform** — e aguarde a
   importação e a compilação terminarem.
3. Clique em **Open Scene List**. Com `TutorialLudus` aberta, use **Add Open
   Scenes**, deixe somente essa cena habilitada e confirme que ela ocupa o
   índice `0` durante o teste isolado.
4. Volte ao perfil **Web** e confirme que **Code Optimization** está em
   **Shorter Build Time** para esta validação.
5. Execute **Build And Run**.
6. Faça ao menos um clique, movimento, início e encerramento de sessão.
7. Confira no JSON baixado `platform: "WebGLPlayer"` e os eventos de
   contexto esperados.

O aviso **Cannot build player while editor is importing assets or compiling
scripts** significa que a troca ainda está em andamento; aguarde até o botão de
build ser habilitado. A primeira ativação de Web pode ser demorada porque a
Unity reimporta os assets afetados pela plataforma e recompila os scripts. O
primeiro build também precisa produzir todo o player Web; builds incrementais
posteriores podem reutilizar conteúdo inalterado e tendem a ser mais rápidos.

O tempo depende do tamanho do projeto, CPU, RAM disponível, velocidade do disco
e processos concorrentes. A internet normalmente não participa do build local,
exceto quando a Unity precisa baixar pacotes ou quando a pessoa escolhe publicar
em um serviço online. Fechar aplicativos pesados pode ajudar se o computador
estiver com pouca memória. Consulte a documentação oficial sobre
[troca de perfil](https://docs.unity3d.com/6000.0/Documentation/Manual/create-build-profile.html),
[configurações do build Web](https://docs.unity3d.com/6000.0/Documentation/Manual/web-build-settings.html)
e [reutilização incremental do conteúdo](https://docs.unity3d.com/6000.0/Documentation/Manual/build-scripts-only.html).

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
- [ ] Capturas visuais mantidas desativadas ou habilitadas somente em cenas autorizadas.
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
