# Guia detalhado: personalizar a integração do LUDUS Unity SDK

> **Veja este conteúdo com uma apresentação mais visual:** acesse a
> [documentação do SDK no site LUDUS Acompanha](https://acompanha.maisludus.com.br/documentacao).

Para obter o primeiro resultado, comece pela **Etapa 1** do
[README](../README.md) ou da documentação visual. Este guia é a referência
detalhada para quem já conseguiu enviar uma sessão básica e deseja explorar os
detalhes das etapas 2 e 3.

Se preferir acompanhar a primeira integração em vídeo, veja o
[Tutorial da Etapa 1: do pacote ao primeiro resultado no Dashboard](https://acompanha.maisludus.com.br/documentacao/videos/tutorial-etapa-simples.mp4).

Se preferir acompanhar uma execução completa, assista ao
[tutorial do SDK e do Dashboard](https://acompanha.maisludus.com.br/documentacao/videos/tutorial-sdk-dashboard.mp4).
Para ver a mesma integração aplicada a um jogo existente, assista à
[aplicação no jogo Para Que Serve?](https://acompanha.maisludus.com.br/documentacao/videos/tutorial-sdk-para-que-serve.mp4).

Faça os testes com um projeto ou cópia de teste e use somente nomes fictícios.
Não use dados reais, credenciais ou JWT.

O roteiro considera um jogo **Unity 6, 2D e WebGL**. Ele também serve para
projetos que usam várias cenas, UI do Canvas, objetos 2D no mundo ou uma mistura
dessas abordagens.

## Antes de começar: o que é um SDK

Um SDK (Software Development Kit) é um pacote reutilizável que adiciona ao
projeto componentes, APIs e assistentes para realizar uma integração. O LUDUS
Unity SDK roda dentro do jogo: ele observa somente os tipos de interação
habilitados, organiza esses registros em uma sessão JSON e pode enviá-la ao
LUDUS Acompanha.

```text
Jogo Unity
  -> LUDUS SDK registra a sessão
  -> API recebe e valida os dados
  -> Dashboard apresenta a atividade ao professor
```

O jogo continua responsável por sua interface e suas regras. O SDK não move
peças, não escolhe respostas corretas e não deduz sozinho categorias, fases,
acertos ou erros. Esses resultados só existem quando o jogo os informa
explicitamente.

## Resultado esperado

Na Etapa 1, o jogo deverá registrar automaticamente duração, cliques,
trajetória do ponteiro e arrastes, encerrar a sessão pelo botão do SDK e enviá-la
ao Dashboard.

Nas etapas 2 e 3, a integração também poderá:

1. escolher cenas, interações e capacidades específicas;
2. iniciar e encerrar a sessão em momentos definidos pelo próprio jogo;
3. enviar automaticamente ou exportar um JSON, conforme
   o destino escolhido;
4. informar acertos, erros, fases, categorias e outros eventos semânticos.

Acertos, erros, fases, categorias e objetivos pedagógicos só são registrados
quando o próprio jogo os informa pela API ou pela ponte semântica visual. Sem
essa integração explícita, o escopo continua sendo apenas a coleta das
interações observáveis descritas neste guia.

### Caminho mínimo

O primeiro resultado pode ser obtido assim:

1. instalar o pacote pelo Package Manager;
2. executar **LUDUS > Começar agora (modo simples)** na cena inicial, salvar a
   cena e pressionar **Play** até aparecer a tela que solicita o nome do
   participante e o código da coleta;
3. com essa tela aberta, voltar ao LUDUS Acompanha e abrir a **Visão geral** do
   Laboratório. No primeiro acesso, clicar em **Criar laboratório de testes**;
   depois, abrir **Coletas de teste** e criar uma coleta fictícia;
4. conectar por nome e código e jogar normalmente;
5. abrir **Ver dados recebidos** no Dashboard e acessar o histórico concluído
   ou o último progresso parcial preservado.

Use pelo menos nome e sobrenome ou nome e inicial do sobrenome, como
`“João Silva”` ou `“João S”`. No modo simples, o botão de acesso aparece somente
na cena inicial; a coleta permanece ativa durante as trocas de cena.

## Etapas 2 e 3: referência detalhada

As seções seguintes apresentam o laboratório guiado e as configurações
opcionais. A Etapa 2 acrescenta contexto visual ao mapa; a Etapa 3 integra
eventos que somente o jogo conhece. O laboratório completo permanece como uma
referência opcional para quem quiser explorar todas as possibilidades.

## 1. Preparar o ambiente

- Use Unity 6.
- Este roteiro visual foi validado na **Unity 6.3 LTS (6000.3.25f1)**. Em
  outras versões, nomes e posições de menus podem variar; nessa versão, por
  exemplo, os elementos de interface aparecem como **UI (Canvas)** no menu de
  contexto da Hierarchy.
- Trabalhe em uma cópia limpa ou branch de integração do jogo.
- Confirme que o projeto abre e executa antes de instalar o SDK.
- Confirme que todas as cenas usadas estão no **Build Profile** do projeto.
- Use uma coleta fictícia no Laboratório do LUDUS Acompanha ou um ambiente
  local demonstrativo. Não misture o teste com dados reais.

## 2. Instalar o pacote

Na Unity:

1. Abra **Window > Package Manager**.
2. Clique em **+**.
3. Escolha **Install package from git URL...**.
4. Informe:

   ```text
   https://github.com/RodrigoBichet/LUDUS-Unity-SDK.git#main
   ```

5. Aguarde a importação terminar sem erros.

Se o repositório estiver privado, o computador precisa ter acesso ao GitHub.
Nunca coloque token ou senha em cenas, scripts ou arquivos versionados.

## 3. Explorar o laboratório guiado opcional

Se quiser conhecer as configurações intermediárias em uma cena separada:

1. Abra **LUDUS > Criar tutorial de teste do SDK**.
2. Confirme a criação dos arquivos em `Assets/LUDUS/Tutorial/`.
3. Abra a cena `TutorialLudus` criada.
4. Selecione o objeto `LUDUS SDK — Tutorial` na Hierarchy. Os componentes desse
   objeto já foram criados e interligados automaticamente; não altere suas
   referências. No componente **Ludus Session Controller**, clique somente na
   referência `ConfiguracaoTutorialLudus` para destacar o asset no Project e
   selecione esse asset. Se a seleção automática não aparecer, abra manualmente
   `Assets/LUDUS/Tutorial/ConfiguracaoTutorialLudus.asset`.
5. Revise o Inspector do asset na ordem em que os campos aparecem:
   - em **Identificação do jogo**, mantenha **Nome do jogo** como
     **Tutorial LUDUS**;
   - em **Envio e cópia local**, escolha **Enviar automaticamente ao LUDUS
     Acompanha** e mantenha **Salvar progresso parcial** habilitado;
   - mantenha **Conexão com ambiente LUDUS (avançado)** fechada. A origem
     oficial já vem preenchida e só precisa ser alterada para outro ambiente;
   - em **Cenas acompanhadas**, mantenha **Capturar automaticamente em: Todas
     as cenas**. A alternativa **Somente cenas selecionadas** permite arrastar
     apenas as cenas de atividade e pausa interações nas demais;
   - em **Coleta essencial**, mantenha **Cliques**, **Trajetória do mouse** e
     **Trajetória de arraste** marcados;
   - em **Resultados informados pelo jogo**, mantenha **Categorias**,
     **Eventos de fase** e **Acertos e erros** desmarcados. Na prática, o SDK
     registra interações sozinho, mas o jogo precisa informar o significado de
     uma categoria, fase, resposta correta ou erro. Categorias podem representar
     níveis como Iniciante, Intermediário e Difícil, gêneros ou modos definidos
     pelo próprio jogo. **Eventos
     personalizados** pode permanecer marcado para eventos contextuais criados
     pelo desenvolvedor;
   - em **Capturas visuais**, marque **Capturar imagem da tela** para praticar o
     mapa com a Game View como fundo. Sem imagem, o mapa permanece disponível
     sobre fundo sólido. Mantenha **Capturar após a primeira interação**; só
     desmarque quando o jogo chamar a captura manualmente em um momento estável.
     Em **Imagens automáticas nas cenas**, mantenha **Todas as cenas
     acompanhadas**. No jogo real, prefira cenas principais de atividade e deixe
     menus, configurações e telas auxiliares sem imagem para reduzir payload e
     ruído no Dashboard;
   - em **Desenvolvimento**, mantenha **Exibir mensagens detalhadas no Console**
     marcado.
6. Use **LUDUS > Tutorial > Adicionar exercício de interações**. O comando cria
   `LUDUS — Exercício de interações`, um laboratório com seis cenários
   navegáveis e uma tela final de orientação: interações automáticas,
   alternativas por botão, arraste com destino, contato por física, meta por
   valor e ciclo de categoria/fase.
   Também cria um `EventSystem` quando necessário. Se encontrar a
   versão antiga do exercício, a Unity pede confirmação antes de recriar
   somente esse objeto tutorial.
7. Expanda **LUDUS — Exercício de interações > Painel do exercício > 01 —
   Interações automáticas** e localize `BotaoTutorial`, `CampoTextoTutorial` e
   `Área de arraste livre/PecaArrastavelTutorial`. Dê dois cliques no
   objeto-raiz para enquadrar o Canvas na aba Scene.
8. Abra **LUDUS > Configurar interações desta cena**. Na janela **Interações
   LUDUS**, arraste `BotaoTutorial`, `CampoTextoTutorial` e
   `PecaArrastavelTutorial`, um por vez, para **Arraste um objeto aqui**. Esse é
   o mesmo fluxo usado depois com botões, campos, imagens, peças arrastáveis e
   objetos clicáveis do jogo real.
9. Se a Unity perguntar **Como este objeto funciona no jogo?**, escolha
   **Objeto arrastável** somente para o item que acompanha o gesto, **Objeto
   clicável** para o item que reage ao clique ou **Cancelar** para corrigir a
   seleção. Se o assistente avisar que o arrastável parece ser um filho, use
   **Usar o filho** quando a peça filha realmente se move; use o contêiner
   apenas quando ele próprio receber o gesto.
10. Em cada item registrado, personalize **Nome exibido no dashboard** com o
    texto que deverá aparecer no site. O campo de texto envia somente conclusão,
    quantidade de caracteres e estado vazio, nunca o conteúdo digitado. Isso
    evita guardar textos pessoais ou livres. Na peça, mantenha **Movimento
    mínimo** em 5 pixels durante o tutorial.
    No Play Mode, os botões **Anterior** e **Próximo** do laboratório alternam
    entre os seis cenários. **Anterior** permanece visível para revisar páginas
    concluídas, enquanto **Próximo** só é liberado depois que a tarefa descrita
    acima da navegação for cumprida. O progresso não é perdido ao voltar.
    Depois do sexto, **Próximo** abre a tela **Tutorial concluído**. Quando o
    acesso foi pareado por nome e código, a sessão é encerrada e o envio é
    iniciado automaticamente. Esses botões não substituem os objetos de
    resultado.
11. Feche a janela **Interações LUDUS**. Encare o objeto-raiz `LUDUS — Exercício
    de interações` como o laboratório completo: ele reúne as seis páginas e
    todos os elementos apresentados ao jogador. Selecione esse objeto na
    Hierarchy e use **GameObject > LUDUS > Avançado (casos específicos) >
    Marcar como área de observação**. Use esse recurso somente quando uma parte
    da cena precisar receber um contexto diferente no mapa. O
    comando adiciona o componente **Área de observação LUDUS** e agrupa o
    laboratório inteiro sob um contexto enquanto o objeto permanece ativo.
12. No Inspector do novo componente:
    - mantenha **Iniciar ao ativar este objeto** e **Encerrar ao desativar este
      objeto** marcados. Eles abrem o recorte quando `LUDUS — Exercício de
      interações` é habilitado e o encerram quando ele deixa de estar ativo;
    - em **Título exibido no acompanhamento**, informe `Exercício de
      interações`. Se ficar vazio, o Dashboard usará o nome do GameObject;
    - em **Tipo deste recorte**, selecione **Canvas**;
    - em **Objetivo deste recorte**, no tutorial você pode escrever
      `Experimentar botão, campo de texto e arraste`;
    - mantenha **Usar este recorte como fundo do mapa** marcado para associar a
      captura visual desse contexto ao mapa de interações;
    - deixe **Objeto controlador LUDUS SDK** vazio. Com uma única base, o
      componente a encontra automaticamente; a referência manual só é
      necessária em um teste técnico com mais de uma base.

    Neste ponto, o objeto-raiz `LUDUS — Exercício de interações` agrupa todo o
    laboratório. Em um jogo real,
    objetos-raiz de minijogos, Canvas ou painéis diferentes podem receber suas
    próprias áreas para abrir e encerrar contextos separados. A imagem continua
    sendo da Game View completa; o recorte identifica o momento e o contexto,
    sem cortar fisicamente a imagem.
13. Expanda **LUDUS — Exercício de interações > Painel do exercício > 02 —
    Resultados por botão**. Selecione `BotaoCorretoTutorial`. No fim do
    Inspector, clique em **Add Component** e navegue, sem usar a busca, por
    **LUDUS > Adaptadores > Resultado de botão**. O adaptador preserva os
    listeners existentes de `Button.onClick` e adiciona automaticamente a
    **Ponte semântica do jogo**.
    No Play, esse cenário é um miniquiz em duas partes. Primeiro, responda
    **Quanto é 1 + 1?**: `3` mostra o erro e permite tentar novamente, enquanto
    `2` libera a tarefa visual. Depois, clique na imagem que contém duas formas.
    **Próximo** só é liberado após as duas tarefas.
14. No componente **Ponte semântica do jogo**:
    - mantenha **Registrar eventos semânticos** e **Avisar falhas no Console**
      marcados;
    - mantenha os valores fictícios de categoria, fase, objetivo, opções e
      resultado. Eles só serão usados quando esses eventos forem ligados à
      ponte; não fazem o SDK inventar eventos.
15. No componente **Resultado de botão**:
    - mantenha **Acompanhar esta alternativa** marcado;
    - escolha **Correto** em **Resultado desta alternativa**;
    - em **Resposta exibida no Dashboard**, informe `2`;
    - em **Resposta esperada no Dashboard**, informe `2`;
    - mantenha **Avisar falhas no Console** marcado;
    - apenas confira **Botão observado** e **Ponte semântica**, preenchidos
      automaticamente.
16. Repita os passos 13 a 15 em `BotaoIncorretoTutorial`. Escolha **Incorreto**,
    informe `3` como resposta exibida e mantenha `2`
    como resposta esperada. Assim o Dashboard consegue mostrar o que foi
    escolhido e qual opção deveria ter sido selecionada.
    Depois, selecione `ImagemUmaFormaTutorial`, que representa a opção visual
    incorreta, e repita o adaptador. Escolha **Incorreto**, informe `Uma forma`
    como resposta exibida e `Duas formas` como resposta esperada. Em seguida,
    configure `ImagemDuasFormasTutorial` como **Correto**, usando `Duas formas`
    como resposta exibida e esperada. Esses textos identificam no Dashboard o
    que foi escolhido e qual imagem deveria ter sido selecionada. Uma imagem
    sem `Button` precisa receber o resultado da regra personalizada que
    realmente avalia a ação.
17. Depois de configurar os quatro botões, no painel **Project**, abra
    **Assets > LUDUS > Tutorial** e clique no asset
    `ConfiguracaoTutorialLudus`. No Inspector do asset, habilite **Acertos e
    erros**. Não habilite categoria ou fase enquanto elas não estiverem ligadas
    a eventos reais.
18. Expanda **03 — Arraste com destino > Área do cenário de arraste**. A peça
    `PecaCorretaTutorial` usa a tag exclusiva
    **LARespostaCorretaTutorial**, criada automaticamente pelo gerador, e a
    `PecaIncorretaTutorial` permanece **Untagged**. A tag representa a regra
    técnica que permite ao destino reconhecer qual peça é correta. No jogo
    real, use esse caminho quando a própria regra puder ser expressa por uma
    tag, como `AnimalAquatico`, `ItemCorreto` ou `FerramentaDaAtividade`.
19. Seguindo a mesma hierarquia, selecione `DestinoArrasteTutorial` e adicione
    somente **Add Component > LUDUS > Adaptadores > Resultado de arraste por
    tag**. A Unity adicionará a Ponte semântica automaticamente se ela ainda
    não existir e reutilizará a mesma instância se ela já estiver presente.

    No laboratório, apenas uma tentativa é processada por vez. Depois de uma
    resposta incorreta, as peças retornam automaticamente às posições
    iniciais para permitir nova tentativa. Quando a resposta está correta, a
    peça escolhida permanece encaixada e bloqueada no destino, confirmando
    visualmente a conclusão do cenário.

    A Ponte semântica é o canal que transforma a decisão já conhecida pelo
    jogo em eventos do LUDUS Acompanha. Neste cenário, mantenha **Registrar
    eventos semânticos** e **Avisar falhas no Console** marcados. Os demais
    valores fictícios podem permanecer como vieram, pois quem fornece os nomes
    específicos da tentativa é o adaptador.

    O adaptador fica no destino, recebe o `OnDrop`, compara a tag da peça solta
    e solicita à ponte o registro da tentativa, do acerto ou do erro. Ele não
    movimenta a peça e não cria a regra do quebra-cabeça. Configure:
    - **Acompanhar este destino:** marcado;
    - **Tag considerada correta:** `LARespostaCorretaTutorial`;
    - **Nome do destino no Dashboard:** `Área de destino do tutorial`;
    - **Resposta esperada no Dashboard:** `Peça correta do tutorial`;
    - **Registrar tentativa de arraste**, **Registrar acerto ou erro** e
      **Avisar falhas no Console:** marcados;
    - **Ponte semântica compartilhada:** confira a referência preenchida
      automaticamente com a Ponte do próprio `DestinoArrasteTutorial`.
20. No cenário **04 — Contato por física**, selecione
    `DestinoContatoTutorial` e adicione **LUDUS > Adaptadores > Resultado de
    contato por tag**. A Ponte semântica será adicionada automaticamente. O
    gerador já preparou `BoxCollider2D`, `Rigidbody2D` e o trigger do destino.
    Configure **Trigger 2D**, tag `LARespostaCorretaTutorial`, busca nos pais
    marcada e resposta esperada `Peça de contato correta`. No jogo real,
    Colliders, Rigidbody, layers, movimento e resposta visual continuam sendo
    responsabilidade do jogo.
    O cenário tutorial também devolve as peças à origem depois de cada contato,
    preservando uma única tentativa visível por vez.
21. No cenário **05 — Meta por valor**, selecione `ControladorMetaTutorial` e
    adicione **LUDUS > Adaptadores > Meta por valor ou pontuação**. Use valor
    inicial 0, meta 3, condição **Maior ou igual**, ação **Registrar acerto**,
    nome `Meta de 3 pontos` e registro único. O controlador tutorial atualiza
    a interface e repassa o valor ao adaptador; no jogo real, o evento que já
    altera a pontuação deve chamar os métodos públicos do adaptador.
22. No cenário **06 — Categoria e fase**, adicione **Ponte semântica do jogo**
    a `ControladorFluxoSemanticoTutorial`. Informe categoria `Categoria
    tutorial`, fase `fase-tutorial-1`, objetivo `Concluir o fluxo guiado`,
    resultado e esperado `Ação tutorial` e uma estrela. Nos eventos `On Click`
    dos quatro botões, arraste o controlador e conecte, na ordem,
    `RegistrarCategoriaSelecionada`, `RegistrarInicioDeFase`, `RegistrarAcerto`
    e `RegistrarConclusaoDeFase`. Depois habilite **Categorias** e **Eventos de
    fase** em `Assets/LUDUS/Tutorial/ConfiguracaoTutorialLudus`.
    No Play, apenas a ação atual fica habilitada: escolher o desafio, começar
    a atividade, informar o resultado correto e concluir. A linha visual troca
    cada círculo por uma confirmação conforme o ciclo avança. Os rótulos
    visíveis formam a experiência didática; os nomes técnicos dos quatro
    GameObjects continuam sendo usados para localizar seus eventos no Inspector.
23. Abra **LUDUS > Configurar resultados da cena** para conhecer o assistente
    que localiza destinos por tag em cenas reais. Como o tutorial já foi
    configurado manualmente, apenas confira os objetos encontrados e não
    duplique os componentes.
24. Execute **LUDUS > Validar integração semântica** e corrija erros antes do
    Play Mode.
25. Crie uma coleta de teste no LUDUS Acompanha e copie o código temporário.
26. Execute **GameObject > LUDUS > Adicionar acesso por nome e código**, gere
    um **Build And Run** WebGL e conecte um participante da turma pelo botão
    **L**.
27. Percorra os cenários com **Anterior** e **Próximo**. Realize as interações
    automáticas; no miniquiz, você pode escolher `3` para conferir o erro,
    depois deve escolher `2` e clicar na imagem com duas formas; experimente uma
    peça incorreta se quiser conferir o feedback e depois use a correta no
    destino e no contato; alcance a meta 3; e conclua o ciclo semântico na ordem
    que for sendo liberada. **Próximo** permanece bloqueado enquanto a tarefa
    atual estiver pendente.
28. Depois do sexto cenário, clique em **Próximo** para abrir a tela **Tutorial
    concluído**. A sessão conectada é encerrada e enviada automaticamente;
    confira o participante e o recebimento no cartão da coleta.

#### Escolher o caminho de resultado no jogo real

Nem toda interação observada representa automaticamente um acerto ou erro.
Escolha o adaptador conforme a regra que o próprio jogo já conhece:

- **Botão correto ou incorreto:** adicione **Resultado de botão** ao GameObject
  que possui `Button` e classifique a alternativa.
- **Peça entregue em uma área de interface:** adicione **Resultado de arraste
  por tag** à área que recebe `OnDrop`, não à peça que se move. Informe a tag
  correta, o destino e a resposta esperada. O assistente **LUDUS > Configurar
  resultados da cena** ajuda a preparar esses destinos.
- **Trigger ou colisão 2D/3D:** adicione **Resultado de contato por tag** ao
  objeto que recebe o contato e configure o modo físico e a tag correta.
- **Pontuação ou progresso:** adicione **Meta por valor ou pontuação** ao
  controlador e conecte os eventos reais do jogo aos métodos de adicionar,
  subtrair, definir ou avaliar o valor.
- **Campo de texto, imagem clicável ou regra personalizada:** a captura
  automática registra a interação, mas não avalia seu significado. Imagens que
  são `Button` podem usar o adaptador de botão; nos demais casos, conecte o
  `UnityEvent` que já conhece o resultado à **Ponte semântica do jogo** ou use a
  API C# do SDK.

### 3.1. Isolar o primeiro build WebGL

Para o primeiro build depois da instalação:

1. abra **File > Build Profiles** e selecione **Web**;
2. se o perfil ainda não estiver ativo, use **Switch Profile** ou **Switch
   Platform** e aguarde a importação e a compilação terminarem;
3. abra **Open Scene List** e desmarque temporariamente as cenas do jogo, sem
   excluí-las;
4. com `TutorialLudus` aberta, use **Add Open Scenes**;
5. deixe somente `TutorialLudus` habilitada no índice `0`;
6. execute **Build And Run** e repita o teste manual;
7. ao terminar todo o tutorial, volte à lista, desmarque ou remova
   `TutorialLudus`, habilite novamente as cenas originais na ordem correta e
   confirme que a cena inicial do jogo voltou ao índice `0`.

### 3.2. Levar o tutorial até o Dashboard

Antes do envio automático, prepare o ambiente demonstrativo. No Dashboard,
acesse **Laboratório > Visão geral**. Se **Laboratório LUDUS**, **Turma de
teste** e os três alunos fictícios já aparecerem, apenas confirme os dados; esse
ambiente é a instituição simulada do desenvolvedor. Use **Criar laboratório de
testes** somente se a tela ainda não apresentar esses itens.

Faça o fluxo principal como uma coleta automática, do mesmo modo que acontecerá
no jogo integrado:

1. em **Coletas de teste**, informe primeiro um título para a coleta, escolha a
   turma fictícia, defina a validade, selecione **Reconhecer jogos durante a
   atividade** e gere um código temporário;
2. confirme que `ConfiguracaoTutorialLudus` já usa **Enviar automaticamente ao
   LUDUS Acompanha** e mantém o progresso parcial habilitado. A origem oficial
   já vem preenchida e só precisa ser conferida em **Conexão com ambiente LUDUS
   (avançado)** quando outro ambiente for necessário;
3. execute **GameObject > LUDUS > Adicionar acesso por nome e código**;
4. na **Hierarchy**, mantenha o objeto inteiro `LUDUS — Acesso à coleta` na
   cena inicial, de menu ou primeira cena do jogo. Neste tutorial, coloque-o
   dentro de `LUDUS — Exercício de interações`;
5. o botão **L** é adicionado no canto superior direito. Posicione-o em outro
   local se desejar, movendo apenas o elemento visual sem remover o objeto ou a
   tela de acesso;
6. pressione **Play** para validar o fluxo diretamente na Unity;
7. no site LUDUS Acompanha, abra a **Visão geral** do Laboratório. Se este for
   o primeiro acesso, clique em **Criar laboratório de testes** e aguarde a
   preparação do ambiente. Depois, abra **Coletas de teste**, crie a coleta e
   copie o código temporário;
8. no tutorial em execução, use o botão **L**, informe o nome do participante e
   o código copiado;
9. após a conexão, percorra o tutorial. Ao abrir **Tutorial concluído**, a
   sessão é encerrada e enviada automaticamente; o painel manual não precisa
   ser exibido;
10. em **Coletas de teste**, abra **Ver dados recebidos**. No laboratório
    fictício, o participante novo é criado automaticamente e a sessão concluída
    oferece **Ver dados do aluno**. Se o Dashboard solicitar revisão manual —
    como em uma escola real ou diante de homônimos — confirme o cadastro correto
    antes de adicionar as sessões ao histórico;
11. opcionalmente, faça um **Build And Run** WebGL e repita o teste. Para também
    baixar o JSON, escolha antes **Desenvolvimento: enviar e também baixar**.

Guarde o nome de um aluno pertencente à turma escolhida e o código temporário
da coleta. É esse par que conecta o jogo à coleta correta e permite vincular a
sessão recebida ao participante.

O nome informado no acesso deve corresponder a um participante da turma da
coleta. A identidade passa a vir do pareamento por nome+código; o painel
fictício do tutorial não participa deste segundo fluxo.

<details>
<summary>Opcional — testar por arquivo JSON</summary>

Use o painel `LUDUS — Tutorial de teste` para iniciar uma sessão fictícia e
gerar um JSON somente se quiser validar o modo offline, inspecionar o arquivo ou
simular uma falha de rede. Depois, no site, abra **Laboratório > Alunos de
teste**, escolha um aluno e use **Importar JSON**. Esse caminho não é necessário
no uso normal por nome e código de coleta.

Para exibi-lo, selecione `LUDUS SDK — Tutorial` na Hierarchy e marque **Exibir
painel manual de JSON** no componente `Ludus Tutorial Test Panel`.

</details>

Mais adiante, **GameObject > LUDUS > Adicionar coleta ao meu jogo** criará um asset separado
em `Assets/LUDUS/ConfiguracaoLudus.asset` — ou um nome numerado equivalente se
já houver outro arquivo. É nele que você informará o nome do jogo real,
selecionará as cenas acompanhadas e escolherá como entregar as sessões. O asset
do tutorial permanece isolado, com dados fictícios e opções seguras para
aprendizagem.

O encerramento manual ainda dentro do Play Mode é o caminho recomendado, pois
permite conferir imediatamente o JSON no Console. Se você interromper o Play
Mode por engano com a sessão do tutorial ativa, o próprio painel encerra e
serializa a sessão automaticamente para evitar a perda do teste. Essa proteção
existe somente na cena tutorial; no jogo real, o encerramento deve continuar
ligado explicitamente ao término ou cancelamento da atividade.

O campo **Nome exibido no dashboard** começa preenchido com o nome do
GameObject. Você pode substituí-lo por um texto mais legível sem renomear o
objeto na Hierarchy e sem alterar o funcionamento do jogo. Por exemplo,
`PecaArrastavelTutorial` pode aparecer como `Peça arrastável do tutorial`.

A peça se movimenta graças a um componente exclusivo do tutorial. No jogo real,
o SDK apenas observa e registra o gesto configurado; ele não move objetos nem
define regras de destino, acerto ou erro.

O painel do exercício permanece à direita porque o controle da sessão aparece
à esquerda durante o Play. Não reposicione o Canvas pela aba Scene. O gerador
também cria uma câmera neutra apenas para evitar a mensagem
`No cameras rendering` na Game View.

O tutorial só termina quando a sessão manual ou automática puder ser aberta no
Dashboard e seus registros forem compreendidos. Ele é material de laboratório:
depois da validação, não inclua `TutorialLudus` no build final do jogo.

## 4. Adicionar o coletor ao jogo

Se você concluiu a Etapa 1, reutilize o objeto
**LUDUS SDK — Modo simples** e pule diretamente para a seção 4.1. Não crie uma
segunda base.

Se ainda não usou o modo simples:

1. Abra a primeira cena carregada pelo jogo ou outra cena inicial estável.
2. Use **GameObject > LUDUS > Adicionar coleta ao meu jogo**.
3. Salve a cena.
4. Confirme que foi criado um objeto chamado **LUDUS SDK** e um asset de
   configuração em `Assets/LUDUS/`.
5. Mantenha apenas **uma base LUDUS** no jogo. Ela persiste durante as trocas de
   cena; não repita o comando em todas as cenas.

### 4.1. Localizar o asset que realmente será configurado

1. Selecione o objeto **LUDUS SDK** na Hierarchy.
2. No Inspector, localize o componente **Ludus Session Controller**.
3. Em **Configuração do jogo (asset)**, clique na referência
   `ConfiguracaoLudus` para destacá-la no Project.
4. Selecione `Assets/LUDUS/ConfiguracaoLudus.asset`.

Os componentes do GameObject formam a base de execução. O nome do jogo, o
destino dos dados, as cenas e as capacidades ficam no asset. Não remova a
referência nem reutilize o asset `ConfiguracaoTutorialLudus` no jogo real.

### 4.2. Identificação e destino dos dados

- **Nome do jogo:** use um nome estável e reconhecível, como `Meu Jogo
  Educacional`. O SDK gera internamente o identificador técnico e começa com a
  versão de jogo `1.0.0`.
- **Enviar automaticamente ao LUDUS Acompanha:** fluxo recomendado para uma
  turma. A sessão é enviada quando termina e nenhum JSON é baixado no computador
  do participante. Se o envio falhar, o SDK mantém fallback local.
- **Exportar JSON manualmente:** gera no computador um arquivo JSON para revisão
  e posterior importação autorizada no Dashboard. É o destino mais simples para
  compreender o tutorial isolado.
- **Desenvolvimento: enviar e também baixar:** envia à plataforma e também
  solicita o arquivo. Use apenas durante validação; em uma turma, essa opção
  baixaria um JSON em cada computador.
- **Guardar também uma cópia de segurança local:** conserva uma cópia mesmo
  quando o envio foi bem-sucedido. Essa cópia usa o armazenamento persistente
  interno da aplicação e é separada do JSON baixado pelo navegador. O caminho
  exato aparece no Console após **Sessão salva localmente**. No Windows,
  normalmente começa em `%USERPROFILE%\AppData\LocalLow\<Empresa>\<Jogo>\`;
  no macOS, em `~/Library/Application Support/<Empresa>/<Jogo>/`. O SDK adiciona
  a subpasta `ludus_offline`. No WebGL, o fallback fica no armazenamento interno
  do navegador. Ele não é salvo dentro da pasta do projeto Unity.
- **Salvar progresso parcial durante a atividade:** nos destinos com envio,
  atualiza automaticamente um checkpoint em segundo plano. O intervalo seguro
  é administrado pelo SDK e não precisa ser configurado.
- **Rótulo do arquivo (opcional):** aparece apenas quando há download manual.
  Use um texto curto sobre a atividade, nunca nome, diagnóstico ou outro dado
  pessoal do participante.

### 4.3. Conexão com o ambiente LUDUS

Novos assets já recebem a origem oficial publicada. Abra **Conexão com ambiente
LUDUS (avançado)** para conferi-la ou somente quando precisar usar outro
ambiente. Nesse caso, em **URL da API LUDUS**, informe apenas a origem fornecida,
sem `/api` no final.

Não grave JWT, senha, código de coleta ou token no projeto. A credencial da
atividade é obtida em tempo de execução pelo participante e permanece somente
em memória.

### 4.4. Cenas acompanhadas

- **Todas as cenas:** cria automaticamente um recorte para cada cena ativa,
  inclusive menus e telas intermediárias.
- **Somente cenas selecionadas:** exibe uma área para arrastar do Project apenas
  as cenas de atividade. Nas demais, a sessão permanece ativa, mas mouse e
  cliques ficam pausados.

Se escolher **Somente cenas selecionadas**, adicione todas as cenas de atividade
que realmente devem ser acompanhadas. Os nomes precisam corresponder às cenas
do projeto e essas cenas também precisam estar no Build Profile.

### 4.5. Coleta essencial

- **Cliques:** registra posição e instante de cliques ou toques.
- **Trajetória do mouse:** amostra o caminho do ponteiro. É útil para o mapa,
  mas produz mais pontos e aumenta o JSON.
- **Trajetória de arraste:** registra início, percurso e fim do gesto. Não move a
  peça e não determina se o destino está correto.

Habilite apenas o necessário. Outros campos permanecem no contrato por
compatibilidade, mas não exigem configuração nesta versão.

### 4.6. Resultados informados pelo jogo

O SDK registra cliques, movimentos e arrastes automaticamente, mas não consegue
adivinhar o significado pedagógico ou a regra de uma ação. O próprio jogo deve
informar quando uma categoria mudou, uma fase terminou ou uma resposta foi
avaliada.

- **Categorias:** habilite somente quando o jogo chamar um evento de categoria,
  como Iniciante, Intermediário, Difícil, Ação ou Filmes, conforme sua própria
  organização.
- **Eventos de fase:** habilite somente quando início ou conclusão de fase forem
  conectados pela ponte, por adaptador ou pela API C#.
- **Acertos e erros:** habilite somente quando a regra real do jogo informar o
  resultado.
- **Eventos personalizados:** permite registrar fatos semânticos próprios e
  documentados pela integração.

Marcar uma capacidade não faz o SDK descobrir a regra. Sem uma ligação real,
ela deve permanecer desativada para o Dashboard não anunciar um dado que o jogo
nunca enviará.

### 4.7. Capturas visuais e desenvolvimento

Em **Capturas visuais**, a imagem da Game View é opcional e permanece
desativada por padrão. Quando habilitada, escolha todas as cenas acompanhadas
ou arraste somente as cenas cuja imagem realmente ajuda a interpretar o mapa.
Prefira cenas principais de atividade e deixe menus, configurações, créditos e
telas auxiliares sem imagem. Isso reduz o payload, evita poluir o Dashboard e
mantém o mapa concentrado no que será observado.
O SDK prepara a imagem quando o recorte é aberto e captura após a primeira
interação relevante. Isso evita que uma tela de carregamento exibida na entrada
da cena se torne o fundo do mapa. O SDK usa JPEG, maior dimensão de 1280 px,
qualidade 70 e conserva até quatro
imagens automáticas por sessão. Se houver mais recortes, prioriza os que tiveram
mais interações e usa o tempo como desempate. Essas definições ficam internas
para evitar que uma configuração acidental produza arquivos muito pesados.

Não habilite imagens em telas com nomes completos, credenciais, conversas ou
outros dados sensíveis. A captura visual oferece contexto à observação; ela não
é necessária para cliques, trajetórias ou eventos semânticos funcionarem.

Quando uma mesma cena contiver vários Canvas, painéis ou atividades ativados em
momentos diferentes, use **Telas ou atividades dentro da cena (opcional)** para
identificar esses recortes. Se a cena inteira representar uma única atividade,
não adicione áreas extras.

Mantenha **Exibir mensagens detalhadas no Console** habilitado durante a
integração. Depois dos testes, ele pode ser desligado para reduzir o ruído no
computador do participante; isso não desativa a coleta.

Se a base já existia e o sistema de entrada do projeto mudou, selecione-a e use
**GameObject > LUDUS > Avançado (casos específicos) > Atualizar coletor de
mouse da base selecionada**. Esse comando só é necessário quando uma base já
existente precisa migrar do coletor clássico para o novo Input System.

## 5. Escolher o que será observado

Abra **LUDUS > Configurar interações desta cena**. A janela identifica botões e
campos de texto compatíveis e permite adicionar objetos genéricos da Hierarchy.

Para cada item escolhido:

- dê um nome curto e compreensível para aparecer no Dashboard;
- classifique objetos genéricos como **Objeto clicável** ou **Objeto
  arrastável**;
- em arrastáveis, ajuste a distância mínima apenas se o gesto estiver sendo
  confundido com um clique.

O SDK observa o gesto, mas não move o objeto e não altera as regras do jogo.
Em campos de texto, registra somente conclusão, quantidade de caracteres e
estado vazio; o conteúdo digitado não entra no JSON.

| Elemento do jogo | O que o SDK registra |
| --- | --- |
| `Button` da UI | acionamento, nome, instante e posição |
| `TMP_InputField` ou `InputField` | conclusão, quantidade de caracteres e se ficou vazio |
| objeto clicável genérico | clique, nome, instante e posição |
| objeto arrastável genérico | início, trajetória, fim, duração e distância |

Uma imagem não é automaticamente um objeto arrastável. Ao adicionar um objeto
genérico, escolha a função que ele realmente cumpre no jogo: **Objeto clicável**
ou **Objeto arrastável**. O nome do GameObject é usado inicialmente no Dashboard
e pode ser substituído por um rótulo mais claro.

Em objetos arrastáveis, selecione a peça que recebe o gesto e realmente se
move, não apenas o painel ou a área de destino. Se o manipulador de arraste
estiver em um filho do objeto escolhido, a janela permite trocar para esse
filho. Assim o JSON registra o nome da peça, as coordenadas inicial e final, a
duração e a distância do gesto.

### Requisitos para a interação chegar ao SDK

Para elementos dentro de um Canvas:

- deve existir um `EventSystem` ativo na cena;
- o Canvas deve possuir `GraphicRaycaster`;
- o componente gráfico do objeto deve aceitar raycast (`Raycast Target`);
- outro painel invisível não pode estar bloqueando o ponteiro.

Para objetos 2D no mundo, use um `Collider2D` no objeto e um
`Physics2DRaycaster` na câmera que o renderiza, além do `EventSystem`. Esses
componentes permitem que os eventos de ponteiro cheguem ao marcador LUDUS; eles
não tornam o objeto clicável ou arrastável por conta própria.

Se a atividade ocupar apenas parte da tela, use um `LudusCaptureContextTrigger`
para delimitar o contexto. Contextos são opcionais e não representam, por si
só, acerto, erro ou objetivo pedagógico.

Quando uma mesma cena possui telas ou atividades internas ativadas em momentos
distintos, elas podem ser marcadas em **Telas ou atividades dentro da cena** no
asset LUDUS. A imagem continua sendo da Game View completa, mas fica vinculada
ao recorte ativo. Para uma cena que representa uma única atividade, selecione
somente a cena e não crie recortes extras.

## 6. Ligar o ciclo de vida da sessão ao jogo

### Opção visual: escopo da atividade

Selecione na Hierarchy um objeto que permaneça ativo durante toda a atividade.
No Inspector, clique em **Add Component > LUDUS > Fluxo > Sessão acompanhada**.
Esse caminho pertence ao menu **Add Component**, não ao menu superior
**LUDUS**. Por padrão, o componente inicia a sessão quando o objeto é ativado e
a encerra quando ele é desativado ou quando sua cena é fechada.

No envio automático, o participante precisa estar conectado por nome+código
antes de esse objeto ser habilitado. Normalmente, o acesso fica no menu do jogo
e a Sessão acompanhada fica na cena ou no objeto-raiz da atividade seguinte.

O escopo representa a atividade completa, não cada tela interna. Se uma cena
randomiza vários Canvas e depois mostra um Canvas final de feedback, mantenha o
escopo em um objeto-raiz estável: todos esses momentos formarão uma única
sessão. Canvas opcionais continuam podendo ser usados como recortes de
observação, sem reiniciar a sessão.

Quando a atividade começa ou termina sem trocar de cena, desmarque o
automatismo necessário e ligue os métodos `IniciarSessao` ou `EncerrarSessao`
ao `UnityEvent` real do jogo. O componente só encerra uma sessão que ele próprio
iniciou, evitando que um painel auxiliar finalize outro fluxo por engano.

Os eventos **Depois de iniciar a sessão** e **Antes de encerrar a sessão**
podem acionar uma Ponte semântica para registrar categoria, início e conclusão
de fase. Essa ligação permanece explícita; o escopo não deduz resultados a
partir da troca de Canvas ou de cena.

### Opção por API C#

Para um fluxo de importação manual, chame o SDK quando o jogador realmente
começar e terminar a atividade:

```csharp
using LudusSDK;
using UnityEngine;

public sealed class MeuFluxoLudus : MonoBehaviour
{
    [SerializeField] private string nomeDaSessao = "Atividade 1";

    public void IniciarAtividade()
    {
        if (!LudusSdk.TryStartSessionForManualImport(nomeDaSessao, out string erro))
        {
            Debug.LogError("[LUDUS] " + erro);
        }
    }

    public void EncerrarAtividade()
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

Como ligar sem modificar a lógica central do jogo:

1. crie o script acima em `Assets/Scripts/MeuFluxoLudus.cs`;
2. adicione-o a um GameObject persistente ou ao controlador da atividade;
3. no botão que inicia o jogo, acrescente `MeuFluxoLudus.IniciarAtividade` ao
   evento `On Click()` (um `UnityEvent`) sem remover as ações existentes;
4. no botão ou fluxo que conclui a atividade, chame
   `MeuFluxoLudus.EncerrarAtividade` depois da última interação relevante.

Também é possível chamar os dois métodos diretamente do código já existente.
O nome da sessão serve para organização e não deve conter nome, diagnóstico ou
outro dado pessoal do participante.

Não inicie a sessão apenas porque uma cena carregou se a atividade ainda não
começou. Não a encerre antes da última interação que deve pertencer à sessão.
Não inicie uma segunda sessão sem encerrar a anterior e não encerre a mesma
sessão duas vezes.

No fluxo manual, a Unity gera uma identidade técnica neutra. O vínculo com um
aluno fictício ou autorizado é escolhido somente na importação pelo Dashboard;
o desenvolvedor do jogo não precisa inserir `studentId` no projeto Unity.

### 6.1. Informar eventos que pertencem à regra do jogo

Cliques, trajetórias, arrastes e imagens são observados pelo SDK. Categoria,
fase, acerto e erro só podem ser informados pelo próprio jogo. Quando a
integração realmente conhecer esses fatos, habilite no asset LUDUS as
capacidades correspondentes e use a API tipada:

```csharp
using LudusSDK;

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

Habilite **Eventos de fase** somente se usar `TryPhaseStarted` ou
`TryPhaseCompleted`; **Acertos e erros** somente se usar `TryCorrectMatch` ou
`TryWrongMatch`; e **Categorias** somente se usar `TryCategorySelected`. O SDK
rejeita eventos cuja capacidade esteja desligada e incrementa os totais de
acerto e erro quando essas chamadas são aceitas.

#### 6.2. Conectar um UnityEvent sem escrever chamadas C#

Quando o jogo já possui um `UnityEvent` disparado no momento em que conhece o
resultado, adicione **Add Component > LUDUS > Ponte semântica do jogo** a um objeto da atividade.
Configure uma ponte por atividade ou fase. Depois:

1. localize no Inspector o `UnityEvent` que o jogo já dispara;
2. acrescente um novo ouvinte sem remover os ouvintes do jogo;
3. arraste o objeto que contém `LudusSemanticBridge`;
4. selecione `RegistrarAcerto`, `RegistrarErro`, `RegistrarInicioDeFase` ou o
   método correspondente;
5. habilite no asset LUDUS as capacidades realmente conectadas;
6. valide uma rodada fictícia com pelo menos um erro e um acerto.

`RegistrarInicioDeFase` reinicia o cronômetro e os contadores locais da ponte.
`RegistrarConclusaoDeFase` envia esses totais e o tempo transcorrido. Para um
arraste avaliado, o mesmo evento do jogo pode chamar primeiro o método de
tentativa e depois `RegistrarAcerto` ou `RegistrarErro`.

A ponte não interpreta tags, colisões, pontuação ou scripts. Ela apenas recebe
uma notificação que o jogo já classificou. Se não houver `UnityEvent`
compatível, use a API C# ou um adaptador apoiado pelo SDK; sem essa ligação, não
declare as capacidades semânticas.

#### 6.3. Arraste de UI avaliado por tag

Quando uma área de UI já recebe `OnDrop` pelo `EventSystem` e a regra do jogo
considera correta uma tag conhecida:

1. abra a cena da atividade;
2. abra **LUDUS > Configurar resultados da cena** e informe seu nome ou
   categoria;
3. revise os destinos encontrados e suas tags;
4. desmarque qualquer destino cuja tag não represente a resposta correta;
5. clique em **Aplicar configuração confirmada**;
6. salve a cena e use **LUDUS > Validar integração semântica**.

O assistente cria ou reutiliza uma Ponte semântica compartilhada, conecta
categoria, início e conclusão da fase ao escopo e adiciona o adaptador aos
destinos confirmados. Ele também habilita `categoryEvents`, `phaseEvents`,
`correctWrong` e `customEvents` no asset de configuração. Executá-lo novamente
atualiza a configuração sem duplicar componentes ou ouvintes.

Quando a cena ainda não possui uma **Sessão acompanhada**, o assistente também
cria esse objeto automaticamente na raiz e usa o nome informado. Se houver um
único escopo, ele é reutilizado; múltiplos escopos são bloqueados para evitar
que o SDK escolha silenciosamente o ciclo de vida errado.

Para cada destino confirmado, o assistente também prepara o Canvas mais próximo
como área de observação. Durante o jogo, a tag continua sendo a regra técnica de
acerto ou erro, enquanto o SDK procura o texto, a imagem ou o sprite atualmente
visível para registrar nomes pedagógicos. A tentativa inclui a peça escolhida,
a resposta visual esperada e as alternativas visíveis naquele Canvas.

Objetos `Untagged` ficam visíveis como incompatíveis. O assistente não conclui
que toda tag significa acerto: a confirmação humana preserva a regra declarada
pelo jogo. Para configurar um destino isoladamente, selecione a área, adicione
**Add Component > LUDUS > Adaptadores > Resultado de arraste por tag** e escolha
a tag. A Unity cria ou reutiliza automaticamente a Ponte semântica do próprio
destino.
7. teste uma peça com a tag correta e outra com uma tag diferente.

O adaptador usa a peça informada por `PointerEventData.pointerDrag`. Ele apenas
observa o drop: não move a peça, não determina sua posição final e não executa
a resposta visual do jogo. Se o jogo usa física, triggers, raycasts ou código
próprio sem `OnDrop`, este adaptador não se aplica.

#### 6.4. Trigger ou colisão avaliado por tag

Adicione **Add Component > LUDUS > Adaptadores > Resultado de contato por tag** ao objeto que
recebe o contato. Escolha `Trigger2D`, `Collision2D`, `Trigger3D` ou
`Collision3D`, selecione a tag correta e indique a resposta esperada. Mantenha
a busca nos objetos pais quando o collider estiver em um filho da peça.

Colliders, rigidbodies, `Is Trigger`, layers e a matriz de colisão continuam sob
responsabilidade do jogo. O adaptador apenas observa o callback e não altera a
física ou a posição dos objetos.

#### 6.5. Alternativa correta ou incorreta em botão

Em cada alternativa com `Button`, adicione **Add Component > LUDUS > Adaptadores > Resultado de
botão**. Escolha se ela representa acerto ou erro e informe os nomes exibidos
no Dashboard. O componente preserva todos os listeners existentes de
`Button.onClick`.

#### 6.6. Meta por valor ou pontuação

Adicione **Add Component > LUDUS > Adaptadores > Meta por valor ou pontuação** ao controlador
da atividade e configure:

- valor da meta;
- comparação maior ou igual, menor ou igual, ou igualdade com tolerância;
- acerto, conclusão de fase ou ambos ao atingir a meta;
- registro único ou repetível.

Ligue um `UnityEvent` do jogo a `AdicionarUm`, `SubtrairUm`, `Adicionar`,
`DefinirValor` ou `AvaliarAgora`. O adaptador mantém somente seu valor de
integração; ele não lê nem modifica variáveis privadas do placar do jogo.

Não trate automaticamente toda troca de cena como conclusão. Menus, retorno,
cancelamento e reinício também podem trocar cenas. A progressão deve ser ligada
ao evento real do jogo ou a um adaptador de cena configurado explicitamente.

Não transforme clique, tempo parado, trajetória ou imagem em acerto, erro,
dificuldade ou conclusão pedagógica. Esses registros fornecem evidências para
acompanhamento e mediação docente, não diagnóstico ou avaliação conclusiva.

### 6.7. Conectar uma turma por nome e código

Na cena de menu, use:

```text
GameObject > LUDUS > Adicionar acesso por nome e código
```

O assistente cria **LUDUS — Acesso à coleta**, contendo:

- botão compacto **L** para abrir o acesso;
- modal com nome do participante e código da coleta;
- mensagens de conexão e erro;
- referências já ligadas ao SDK.

O objeto é criado na raiz da cena para evitar que o SDK escolha por engano um
Canvas técnico, de carregamento ou de transição. Arraste o objeto inteiro
**LUDUS — Acesso à coleta** para o Canvas visual em que o botão deverá aparecer.
Ao receber esse Canvas como pai, o contêiner ajusta automaticamente suas âncoras
e mantém escala `1`. Não redimensione nem aplique escala ao objeto principal:
reposicione e redimensione apenas o filho **Abrir acesso**. O modal já usa o
Canvas inteiro, mantém o conteúdo centralizado e reduz o cartão automaticamente
quando a área disponível é menor. O padrão visual usa o azul do LUDUS Observa,
com um botão compacto `L`, mas seus elementos continuam editáveis conforme a
identidade do jogo.
Botão, cartão, campos e ações usam cantos arredondados desenhados pelo SDK, sem
depender de imagens externas. O assistente prefere `Roboto-Regular` quando essa
fonte existe no projeto e mantém um fallback nativo da Unity quando não existe.
Não remova o componente `LudusCollectionAccessPanel` nem suas referências. O
desenvolvedor não preenche professora, escola, turma, aluno ou código no
Inspector: a professora cria a coleta no Dashboard, escolhe a turma e informa o
código temporário durante a aplicação.

Após o pareamento, a credencial fica somente na memória daquela execução. Ela
não é salva em cena, asset, `PlayerPrefs` ou JSON. Cada checkpoint e sessão é
enviado à coleta correta. Quando o nome identifica um único aluno da turma, a
sessão entra diretamente no histórico; nomes inexistentes ou homônimos ficam
preservados para revisão docente. Fechar e reabrir a página exige novo
pareamento, o que evita guardar credenciais no computador do estudante.

O acesso por código é uma alternativa à identificação por parâmetros WebGL.
Projetos hospedados dentro de um portal que já conhece `studentId` podem manter
`ludusStudentId` e `ludusPlayerId`; não use os dois fluxos simultaneamente na
mesma execução.

## 7. Validar no Editor

1. Abra **LUDUS > Validar integração semântica** e corrija os erros estruturais
   indicados. A janela verifica base, configuração e capacidades exigidas pelos
   adaptadores, mas não avalia a regra interna do jogo.
2. Inicie uma sessão com identidade e atividade fictícias.
3. Teste pelo menos um botão, um movimento e um arraste configurado.
4. Se houver campo de texto, digite apenas conteúdo fictício e confirme que o
   texto não aparece no JSON.
5. Se habilitou captura visual, aguarde a imagem terminar antes de encerrar.
6. Encerre a sessão uma única vez.
7. Confirme `schemaVersion`, `captureMode: "sdk"`, `source`, `sourceVersion`, `capabilities`,
   duração, viewport e coleções esperadas.
8. Se integrou eventos do jogo, confirme tipos, payloads e totais de acerto e
   erro com uma rodada fictícia que contenha pelo menos um erro e um acerto.
9. Verifique que nada do jogo mudou por causa do coletor.

Uma ponte conectada manualmente por `UnityEvent` recebe apenas um aviso de
revisão: o Editor não consegue provar que o evento escolhido pelo jogo significa
o resultado descrito. Adaptadores conhecidos podem ser verificados com mais
precisão porque declaram quais capacidades do contrato utilizam.

Na Console, erros do SDK começam com `[LUDUS]`. Um JSON válido deve possuir pelo
menos início e fim coerentes, `gameId`, plataforma, duração e as coleções
correspondentes às capacidades habilitadas.

## 8. Validar no WebGL

1. Abra **File > Build Profiles** e selecione **Web** na lista de plataformas.
2. Se Web ainda não tiver o selo **Active**, clique em **Switch Profile** — a
   interface de algumas versões usa **Switch Platform** — e aguarde a troca de
   plataforma terminar.
3. Clique em **Open Scene List**. Com `TutorialLudus` aberta, use **Add Open
   Scenes**, desabilite ou remova as demais cenas para este teste isolado e
   deixe `TutorialLudus` habilitada no índice `0`.
4. Volte ao perfil **Web** e use **Shorter Build Time** em **Code
   Optimization** durante a validação.
5. Para importação manual, escolha **Exportar JSON manualmente** em **Destino
   dos dados**. Para turma com coleta automática, escolha **Enviar
   automaticamente ao LUDUS Acompanha** e use o botão de nome+código.
6. Execute **Build And Run**.
7. Repita o fluxo completo no navegador.
8. No fluxo manual, encerre a sessão e confirme o download do JSON. No fluxo
   automático, confirme no Dashboard o checkpoint ou a sessão recebida.
9. Feche a aba durante uma segunda rodada curta e confirme que o último
   checkpoint permaneceu protegido; ele não equivale a uma conclusão de fase.
10. Guarde arquivos baixados apenas durante o teste.

Durante a primeira troca para Web, é normal o botão de build ficar indisponível
com o aviso **Cannot build player while editor is importing assets or compiling
scripts**. A Unity precisa reimportar assets dependentes da plataforma e
recompilar scripts antes do build. O primeiro build Web também gera o player
completo; execuções incrementais posteriores podem reutilizar conteúdo que não
mudou e costumam ser mais rápidas.

Projetos maiores e computadores com CPU, RAM ou disco mais ocupados podem levar
mais tempo. A conexão de internet normalmente não afeta um build local já
configurado, salvo quando ainda é necessário baixar pacotes ou quando o destino
é um serviço de publicação online. Consulte a documentação oficial sobre
[troca de perfil](https://docs.unity3d.com/6000.0/Documentation/Manual/create-build-profile.html),
[configurações do build Web](https://docs.unity3d.com/6000.0/Documentation/Manual/web-build-settings.html)
e [builds incrementais](https://docs.unity3d.com/6000.0/Documentation/Manual/build-scripts-only.html).

O teste no Editor não substitui o WebGL: foco, coordenadas, download e sistema
de entrada podem se comportar de forma diferente no navegador.

## 9. Importar e conferir no Dashboard

Siga o
[guia de importação end to end do LUDUS Acompanha](https://github.com/RodrigoBichet/LUDUSAcompanha/blob/main/docs/GUIA_IMPORTACAO_END_TO_END.md).
Se os dois repositórios estiverem no mesmo computador, o arquivo equivalente é
`LUDUSAcompanha/docs/GUIA_IMPORTACAO_END_TO_END.md`.

Use um participante fictício em ambiente com banco temporário. Depois da
importação, confira a sessão, as capacidades declaradas e o mapa. Só espere
eventos semânticos que tenham sido informados explicitamente pelo jogo.

## Checklist de aceite

- [ ] O projeto continuou funcionando antes e depois da instalação.
- [ ] O tutorial isolado gerou JSON.
- [ ] Existe somente uma base `LUDUS SDK` no jogo.
- [ ] Todas as cenas necessárias estão no Build Profile.
- [ ] O botão de nome+código conectou a uma coleta fictícia, quando usado.
- [ ] O jogo inicia e encerra a sessão em momentos conscientes.
- [ ] Somente interações escolhidas aparecem no JSON.
- [ ] Nenhum conteúdo digitado foi armazenado.
- [ ] Capturas visuais, quando habilitadas, mostram apenas conteúdo autorizado.
- [ ] No modo manual, o WebGL baixou o JSON e o Dashboard validou a importação.
- [ ] No modo automático, o Dashboard recebeu a sessão sem download inesperado.
- [ ] O mapa e as capacidades correspondem ao que realmente foi capturado.
- [ ] Nenhum dado real, segredo ou ambiente produtivo foi usado.

## Problemas comuns

- **O menu LUDUS não apareceu:** espere a compilação terminar e corrija erros
  anteriores do projeto.
- **Não há interações no JSON:** confira se a sessão estava ativa, se os itens
  foram configurados, se o contexto correto estava aberto e se `EventSystem`,
  raycaster, collider e `Raycast Target` estão adequados ao tipo de objeto.
- **A trajetória aparece, mas o botão/objeto não é identificado:** o coletor
  global e os marcadores semânticos são independentes. Abra novamente
  **LUDUS > Configurar interações desta cena** e confira o objeto marcado.
- **Uma cena não aparece no JSON:** confira o modo **Capturar automaticamente
  em**, a lista de cenas selecionadas e o Build Profile.
- **Apareceram duas sessões ou eventos duplicados:** procure bases `LUDUS SDK`
  duplicadas e confirme que o método de início não está ligado mais de uma vez.
- **O objeto arrastável não se move:** isso é responsabilidade do jogo; o SDK
  apenas observa o gesto.
- **A trajetória de arraste existe, mas a peça não é identificada:** o coletor
  global registrou o gesto, porém o marcador semântico está no objeto errado.
  Selecione a peça que realmente recebe o arraste, não a área de destino.
- **A imagem não apareceu:** confirme que a capacidade está habilitada, que a
  cena foi marcada para imagem e que a sessão não foi encerrada enquanto a
  captura ainda estava pendente.
- **O download não ocorreu no WebGL:** confirme se **Destino dos dados** está em
  **Exportar JSON manualmente** ou **Desenvolvimento: enviar e também baixar**,
  encerre a sessão a partir de uma ação permitida pelo navegador e teste sem
  bloqueadores.
- **A cópia local não apareceu em Downloads:** a cópia de segurança local não é
  o download do navegador. Para importação manual no WebGL, escolha
  explicitamente **Exportar JSON manualmente** em **Destino dos dados**.
- **O Dashboard recusou o arquivo:** não edite o JSON manualmente; preserve o
  contrato gerado e consulte a mensagem de validação.

## Limites desta versão

- O SDK coleta evidências de interação e apoia o acompanhamento pedagógico; ele
  não diagnostica, classifica clinicamente nem mede aprendizagem de forma
  conclusiva.
- Acertos, erros, fases, categorias e objetivos pedagógicos permanecem
  desativados por padrão e só devem ser habilitados quando forem informados
  explicitamente pelo jogo pela API ou pela ponte semântica.
- Capturas visuais são opcionais, limitadas e devem ser usadas somente com
  finalidade definida e autorização adequada.
- Testar no Editor é necessário, mas o aceite final de uma integração WebGL
  exige um Build And Run e a validação do JSON no Dashboard.
