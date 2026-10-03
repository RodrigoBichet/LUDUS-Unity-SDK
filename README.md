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
- captura imagens JPEG opcionais para contextualizar o mapa de interações;
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

O roteiro visual desta versão foi validado na **Unity 6.3 LTS
(6000.3.25f1)**. Em outras versões, nomes e posições de menus podem variar. Por
exemplo, nessa versão a criação de elementos de interface aparece no menu de
contexto da Hierarchy como **UI (Canvas)**.

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
   https://github.com/RodrigoBichet/LUDUS-Unity-SDK.git#v0.1.4
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
botões **Iniciar sessão fictícia** e **Encerrar sessão e gerar JSON**. Ela usa
uma identidade fictícia, salva a sessão localmente e, no WebGL, baixa um JSON
de demonstração; não altera as cenas do jogo.

Para validar no Editor:

1. Abra `Assets/LUDUS/Tutorial/TutorialLudus.unity`.
2. No Project, selecione
   `Assets/LUDUS/Tutorial/ConfiguracaoTutorialLudus.asset` e observe no
   Inspector que o tutorial usa o nome **Tutorial LUDUS**, não envia sessões
   para a plataforma e mantém uma cópia local. Esse asset pertence somente ao
   tutorial; não o transforme na configuração do jogo real.
3. Opcionalmente, use **LUDUS > Tutorial > Adicionar exercício de
   interações** para criar um botão, um campo de texto e uma peça arrastável
   dentro dessa cena isolada.
4. Se adicionou o exercício, abra **LUDUS > Configurar interações desta cena**
   e arraste `BotaoTutorial`, `CampoTextoTutorial` e
   `PecaArrastavelTutorial` da Hierarchy para o campo **Arraste um objeto
   aqui** da janela **Interações LUDUS**.
5. Pressione **Play**.
6. Clique em **Iniciar sessão fictícia**.
7. Realize as interações apresentadas na Game View.
8. Ainda no Play Mode, clique em **Encerrar sessão e gerar JSON**.
9. Confira o JSON no Console e a cópia local em `ludus_offline`.

Ao integrar o SDK ao jogo real, o comando **LUDUS > Adicionar coleta ao meu
jogo** cria outro asset em `Assets/LUDUS/ConfiguracaoLudus.asset` — ou um nome
numerado equivalente se já existir. Nesse novo asset, informe o nome do jogo,
escolha as cenas acompanhadas e configure a forma de entrega adequada. Assim, a
pessoa desenvolvedora pratica primeiro com dados fictícios e depois repete o
mesmo conceito sem reutilizar a configuração do tutorial.

Encerre a sessão antes de sair do Play Mode. Assim, o JSON aparece imediatamente
no Console e você confirma visualmente que o teste terminou. Se o Play Mode for
interrompido por engano enquanto a sessão do tutorial ainda estiver ativa, o
painel encerra e serializa essa sessão automaticamente como proteção contra
perda. Essa proteção pertence somente ao tutorial e não substitui o encerramento
explícito no fluxo real do jogo.

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

O painel do exercício fica à direita de propósito. Durante o Play, o painel de
controle da sessão aparece à esquerda. Não é necessário reposicionar o Canvas
na aba Scene; a cena também recebe uma câmera neutra para a Game View não exibir
`No cameras rendering`.

Para validar no WebGL, adicione `TutorialLudus` ao Build Profile como primeira
cena habilitada e use **Build And Run**. O JSON deve mostrar
`"platform":"WebGLPlayer"`.

Se quiser validar também a importação manual no Dashboard, use o JSON fictício
gerado pelo tutorial e escolha o destino somente dentro de um ambiente
demonstrativo autorizado.

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

O nome técnico `gameId` é gerado internamente a partir do nome do jogo. Não é
necessário preenchê-lo.

### 3. Inicie e encerre no fluxo do jogo

O jogo é quem sabe quando um estudante foi identificado e quando a atividade
terminou. Conecte estes dois momentos ao fluxo existente do jogo:

Sem escrever código, adicione **LUDUS > Fluxo > Sessão acompanhada** a um
objeto que permaneça ativo durante toda a atividade. O componente pode iniciar
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
adicione **LUDUS > Ponte semântica do jogo** a um objeto da atividade. Configure
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

Para arrastes de UI que utilizam o `EventSystem`, adicione **LUDUS >
Adaptadores > Resultado de arraste por tag** à área que recebe o drop. Selecione
a tag considerada correta e informe nomes legíveis para o destino e para a
resposta esperada. Indique uma Ponte semântica compartilhada ou mantenha uma
única ponte na cena para que o adaptador a localize automaticamente.

Quando a área recebe `OnDrop`, o adaptador compara a tag da peça indicada por
`pointerDrag` e pode registrar tanto `DragAttempt` quanto `CorrectMatch` ou
`WrongMatch`. Os nomes legíveis podem ficar vazios para serem resolvidos pelo
conteúdo visual ativo no momento da tentativa. Ele não move, reposiciona nem
devolve a peça e não substitui a lógica visual do jogo.

Esse adaptador cobre somente destinos que recebem `OnDrop` pelo `EventSystem`.
Arrastes implementados exclusivamente por colisão, trigger, raycast ou código
próprio precisam de outro adaptador ou da API C#.

#### Adaptador de contato por tag

Para jogos que usam física, adicione **LUDUS > Adaptadores > Resultado de
contato por tag** ao objeto que recebe o contato. Escolha entre trigger ou
colisão 2D/3D, selecione a tag correta e indique a resposta esperada. A busca
opcional nos objetos pais cobre o caso comum em que o collider pertence a um
filho e a tag está no objeto principal.

O jogo continua responsável por `Collider`, `Rigidbody`, layers, matriz de
colisão e pela resposta visual. O adaptador somente observa o callback
selecionado e registra `CorrectMatch` ou `WrongMatch`.

#### Adaptador de alternativas por botão

Adicione **LUDUS > Adaptadores > Resultado de botão** a uma alternativa com
`Button`. Marque-a como correta ou incorreta e informe os nomes da resposta
dada e esperada. O adaptador acrescenta um listener a `Button.onClick` sem
remover os listeners existentes do jogo.

#### Adaptador de meta por valor ou pontuação

Adicione **LUDUS > Adaptadores > Meta por valor ou pontuação** ao controlador
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
