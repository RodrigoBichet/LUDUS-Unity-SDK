# Changelog

Todas as mudanças relevantes do LUDUS Unity SDK serão registradas neste arquivo.

## [Não publicado]

### Adicionado

- O modo simples mantém o botão L somente na cena inicial de conexão; a sessão
  e os coletores continuam ativos nas demais cenas, e o botão reaparece ao
  retornar ao menu.
- O pareamento passa a orientar o uso de nome e sobrenome, preservando aliases
  fictícios numerados já usados pelo laboratório.
- Checkpoints automáticos usam intervalo mínimo de 3 segundos e só são
  reenviados quando a telemetria muda; o rastreamento do ponteiro deixa de
  acumular amostras repetidas enquanto ele permanece parado.
- O README e o guia técnico mantêm a referência completa no GitHub, mas agora
  destacam no início a documentação visual publicada no site LUDUS Acompanha.
- Os links de instalação passam a acompanhar o branch `main` enquanto a próxima
  versão ainda não possui tag, e o README oferece também o download direto do
  ZIP para consulta ou instalação manual.
- O asset criado para o tutorial passa a usar envio automático ao LUDUS
  Acompanha desde o início, o botão L é posicionado no canto superior direito e
  o Play no Editor torna-se o teste principal; Build And Run e download de JSON
  permanecem como verificações opcionais.
- Os comandos de atualização do coletor e marcação de área de observação ficam
  agrupados no submenu `Avançado (casos específicos)`, com seus usos
  excepcionais descritos no guia.
- O tutorial passa a iniciar a sessão automaticamente depois do pareamento por
  nome e código e a encerrá-la uma única vez ao abrir `Tutorial concluído`,
  disparando o envio direto à coleta usada no botão L.
- O painel manual de início e exportação de JSON fica oculto por padrão e pode
  ser reativado no Inspector apenas para testes offline ou inspeção técnica.
- O laboratório gerado passa a ocupar quase toda a Game View, com conteúdo
  centralizado em uma largura confortável, tipografia maior, margens
  consistentes, navegação ampliada e uma faixa visual de destaque. A versão
  anterior é recriada mediante confirmação para não preservar o layout
  comprimido; o painel manual opcional continua sem reservar espaço na tela.
- Novos assets de configuração passam a trazer automaticamente a origem oficial
  publicada da API LUDUS. O campo avançado continua editável para ambientes
  alternativos, sem armazenar credenciais no projeto.

- Todos os adaptadores semânticos passam a criar ou reutilizar automaticamente
  uma única `LudusSemanticBridge` no mesmo GameObject. A Ponte agora bloqueia
  componentes duplicados acidentais, e o adaptador de arraste por tag segue a
  mesma configuração automática já usada por botão, contato e meta.
- O exercício isolado passa a ser um laboratório guiado com navegação entre
  seis cenários: interações automáticas, alternativas correta/incorreta por
  botão, arraste com destino, contato por física 2D, meta por valor e ciclo de
  categoria/fase, seguidos por uma tela final que orienta o encerramento e a
  geração do JSON. A atualização de um exercício antigo exige confirmação e
  recria somente o objeto tutorial.
- O segundo cenário passa a apresentar um miniquiz em duas etapas: a resposta
  correta de `1 + 1` libera duas opções visuais distintas, exigindo que a
  pessoa escolha a imagem com duas formas; escolhas incorretas apresentam
  feedback e permitem nova tentativa.
- O sexto cenário transforma os quatro eventos semânticos em um pequeno ciclo
  de atividade, habilitando uma ação por vez e atualizando uma linha visual de
  progresso sem substituir os eventos reais configurados pela pessoa
  desenvolvedora. As etapas ainda bloqueadas agora recebem contraste escuro
  explícito, e o encerramento bem-sucedido orienta no Console a próxima
  validação por **Build and Run** no navegador.
- O controle didático da meta deixa de registrar duas vezes o mesmo clique
  quando mais de um callback tiver sido associado ao botão no mesmo frame.
- O laboratório passa a manter **Anterior** e **Próximo** em áreas separadas,
  bloquear o avanço até a tarefa atual ser cumprida e exibir uma orientação
  dinâmica do que falta. O progresso permanece concluído ao revisar páginas.
- Os cenários de arraste e contato processam uma tentativa por vez e
  apresentam feedback. Alternativas incorretas retornam à origem; a peça
  correta do cenário de arraste permanece encaixada e bloqueada no destino.
- A navegação ignora callbacks duplicados do mesmo botão no mesmo frame, para
  não saltar páginas; o cenário de arraste usa a tag exclusiva
  `LARespostaCorretaTutorial`, criada pelo gerador sem disputar `Player` ou
  convenções globais do jogo consumidor.
- O gerador reconhece a versão intermediária com três páginas e oferece sua
  atualização explícita para o laboratório de seis cenários, sem duplicar o
  objeto existente.
- Controlador didático de progresso, objetos físicos 2D e testes estruturais
  permitem exercitar os adaptadores restantes sem incorporar regras de um jogo
  específico ao núcleo do SDK.
- Componente interno de navegação do tutorial e peças didáticas que liberam o
  raycast durante o arraste, permitindo que destinos `OnDrop` recebam o evento.
- A ponte de entrada usada pela cena tutorial no Editor passa a encaminhar o
  evento de `Drop` ao destino antes de encerrar o arraste, permitindo que o
  terceiro cenário reconheça a peça solta na área indicada.
- Testes EditMode para navegação entre cenários e preservação do raycast das
  peças demonstrativas.
- Documentação introdutória com tutorial isolado antes da integração real,
  explicação do fluxo Unity–SDK–API–Dashboard, localização do asset
  `ConfiguracaoLudus` e descrição de cada grupo do Inspector; caminhos de
  componentes agora indicam explicitamente o uso de **Add Component**. O
  tutorial passa a documentar também isolamento no Build Profile, resultado
  semântico de botão, laboratório fictício, coleta automática e conferência no
  Dashboard. O roteiro técnico segue a ordem real do Inspector e documenta as
  escolhas dos assistentes de interação para objeto clicável, arrastável,
  contêiner e peça filha. Também esclarece os usos de rótulo de arquivo, URL da
  API, seleção de cenas, resultados semânticos, mapas com captura visual,
  recortes internos e restauração do Build Profile após o tutorial. Os caminhos
  de fallback por plataforma e os campos da Área de observação passam a ser
  explicados em linguagem prática. O tutorial também detalha os Inspectors da
  Ponte semântica e do Resultado de botão e diferencia os caminhos de resultado
  para botão, arraste por tag, contato físico, pontuação e eventos próprios. O
  seletores de botão, meta e contato passam a exibir termos em português sem
  alterar os valores serializados usados pelo SDK.
- Assistente **GameObject > LUDUS > Adicionar acesso por nome e código** que
  cria botão e modal já conectados à coleta temporária do Dashboard na raiz da
  cena, evitando selecionar automaticamente Canvas técnicos; ao ser arrastado
  para o Canvas visual, o contêiner ajusta âncoras, dimensões e escala também no
  modo de edição, mantendo botão e modal em proporções utilizáveis.
- Padrão visual compacto para o acesso à coleta, com botão `L`, paleta azul do
  LUDUS Observa e cartão responsivo à área disponível no Canvas do jogo.
- Fundos arredondados vetoriais sem dependência de sprites e seleção automática
  de Roboto com fallback para a fonte nativa da Unity.
- Contexto efêmero de coleta mantido apenas em memória, sem serializar código,
  token, professora, escola, turma ou aluno em cenas e assets do jogo.
- Transporte pareado para sessões e checkpoints do SDK, com retorno automático
  às rotas diretas quando a identidade veio pela URL ou API pública existente.
- Modos de entrega mutuamente exclusivos para envio automático ao Dashboard,
  exportação manual e desenvolvimento com os dois destinos.
- Contrato `1.1.0` com `runId`, `attemptNumber` e `status`, preservando cada
  categoria ou repetição como uma sessão própria dentro da mesma execução.
- Checkpoints periódicos leves no envio automático, protegidos por chave
  efêmera, para reduzir a perda de progresso quando uma aba WebGL é fechada.
- Resolução genérica da identidade do participante pelos parâmetros WebGL
  `ludusStudentId` e `ludusPlayerId`, com alternativa pela API pública
  `LudusSdk.TrySetParticipant` e bloqueio legível do envio automático sem
  vínculo de aluno.
- Intervalo técnico dos checkpoints mantido internamente, sem exigir essa
  decisão do desenvolvedor no fluxo principal do Inspector.
- Pacote `ludus-session-bundle` com sessões concluídas e fotografia opcional
  da sessão atual, além do componente `LudusRunExportButton` para um botão
  posicionado pelo desenvolvedor dentro do Canvas.

- Componente visual `LudusSemanticBridge` para conectar `UnityEvent` já
  existente no jogo a categorias, fases, tentativas de arraste, acertos e
  erros sem escrever uma chamada C# para cada evento.
- Cronômetro e contadores locais por fase na ponte visual, usados ao registrar
  `PhaseCompleted` sem inferir o resultado a partir da interação observada.
- Inspector orientado em português e testes EditMode do fluxo semântico
  completo e do estado desabilitado.
- Primeiro adaptador sem código para áreas de drop da UI: compara a tag da
  peça recebida pelo `EventSystem`, registra tentativa e encaminha acerto ou
  erro sem interferir na movimentação implementada pelo jogo.
- Validação legível de tag inexistente e testes para resultado correto e
  incorreto com nomes descobertos em tempo de execução.
- Adaptador de contato por tag para triggers e colisões 2D/3D, com busca
  opcional da tag nos objetos pais sem interferir em colliders ou movimento.
- Adaptador de alternativas de UI que escuta `Button.onClick`, preserva os
  listeners do jogo e encaminha o resultado configurado no Inspector.
- Adaptador de meta por valor ou pontuação com comparação configurável,
  bloqueio de duplicação e ações de acerto e/ou conclusão de fase.
- Janela **LUDUS > Validar integração semântica** para localizar base ou
  configuração ausente, múltiplos controladores e capacidades incompatíveis
  com os adaptadores presentes na cena.
- Seção **Resultados informados pelo jogo** no Inspector da configuração para
  habilitar categorias, fases, acertos/erros e eventos personalizados sem
  recorrer ao modo de depuração do Inspector.
- Testes EditMode das regras estruturais do validador sem modificar cenas do
  jogo consumidor.
- Componente visual `LudusSessionScope` para iniciar e encerrar uma sessão por
  atividade, cena ou painel, mantendo vários Canvas internos na mesma coleta e
  encerrando somente a sessão que ele próprio iniciou.
- Eventos opcionais do escopo para ligar categoria e fase à Ponte semântica,
  além de métodos públicos para fluxos que não coincidem com a troca de cena.
- Cache das cenas exibidas no Inspector, evitando buscas no `AssetDatabase` a
  cada repaint quando a seleção de capturas visuais está aberta.
- Assistente **LUDUS > Configurar resultados da cena** para localizar destinos
  de drop com tags explícitas, pedir confirmação e configurar tentativa,
  acerto, erro, categoria e fase sem alterar os scripts do jogo.
- Ponte semântica compartilhada entre os destinos configurados pelo
  assistente, permitindo agregar os resultados da atividade em uma única
  conclusão de fase.
- Resolução automática dos nomes semânticos de arraste pelo texto, imagem ou
  sprite visível no momento da tentativa, mantendo a tag apenas como regra
  técnica de comparação.
- Registro das alternativas visíveis e da resposta esperada em cada tentativa
  de arraste, para que o Dashboard apresente o contexto real da atividade.
- Preparação automática de cada Canvas confirmado como área de observação,
  sem exigir que a pessoa desenvolvedora crie esses componentes manualmente.
- Criação automática e idempotente da Sessão acompanhada pelo assistente de
  resultados, com nome da atividade informado na janela e bloqueio de cenas
  ambíguas que possuam múltiplos escopos.
- Navegação guiada pelas cenas habilitadas no Build Profile, preservando o
  diálogo de salvamento do Unity e renovando a sugestão de categoria em cada
  cena sem configurar telas automaticamente.
- Ação **Aplicar e abrir próxima** no assistente semântico, mantendo a
  confirmação individual de nomes e destinos antes de avançar no Build Profile.

### Mantido

- A decisão de acerto ou erro continua pertencendo ao jogo. A ponte apenas
  encaminha ao contrato LUDUS um fato explicitamente ligado pelo integrador.
- Valores internos, mudanças de cena e regras não expostas pelo jogo não são
  inspecionados ou classificados automaticamente.

### Corrigido

- O escopo de sessão reutiliza a mesma base LUDUS que iniciou a coleta ao
  encerrar uma atividade, evitando falhas de redescoberta durante a troca de
  cenas.
- A ponte semântica conserva o controlador usado no início da fase para
  registrar `PhaseCompleted` durante a desativação da cena, antes da
  serialização final da sessão.
- A captura visual automática passa a ocorrer depois da primeira interação
  relevante de cada recorte, evitando usar telas intermediárias de
  carregamento como fundo do mapa.

## [0.1.4] - 2026-10-01

### Adicionado

- API tipada `LudusGameEvents` para categorias, início e conclusão de fases,
  tentativas de arraste, acertos e erros informados pelo próprio jogo.
- Validação imediata do tipo e payload dos eventos semânticos antes de
  adicioná-los à sessão.
- Atualização automática de `metrics.totalCorrect` e `metrics.totalWrong`
  quando o jogo registra `CorrectMatch` e `WrongMatch`.
- Testes EditMode para eventos semânticos, métricas e rejeição quando a
  capacidade correspondente está desabilitada.

### Alterado

- Os campos `phaseEvents`, `correctWrong` e `categoryEvents` passam a indicar
  integrações disponíveis pela API, mantendo-se desligados por padrão.
- O guia de integração explica como registrar somente fatos conhecidos pelas
  regras do jogo, preservando o caráter de apoio pedagógico da plataforma.

## [0.1.3] - 2026-10-01

### Adicionado

- Captura visual opcional da Game View em JPEG para contextualizar mapas de
  interação, desativada por padrão.
- Seleção por arrastar e soltar das cenas que podem fornecer imagem e das
  telas ou atividades internas que formam recortes próprios.
- Retenção de uma imagem por cena ou recorte, com máximo padrão de quatro por
  sessão e prioridade por quantidade de interações e duração.
- Validação de formato, dimensões, Base64 e limites individuais e totais das
  imagens antes da serialização.
- API manual `LudusSdk.TryCaptureScreenshot` para integrações que precisam
  solicitar uma imagem em momento explícito.
- Testes EditMode para configuração, serialização, limites, seleção e
  priorização das capturas visuais.

### Alterado

- Cenas acompanhadas e cenas com imagem passam a ser escolhidas por objetos de
  cena arrastados do Project, evitando listas extensas no Inspector.
- Áreas internas opcionais podem ser marcadas arrastando Canvas, painel ou
  atividade da Hierarchy, sem exigir que toda cena seja dividida em recortes.
- A janela de interações avisa quando a área selecionada apenas contém a peça
  que realmente recebe o gesto de arraste e permite acompanhar o filho certo.
- Logs do tutorial deixam de imprimir o JSON completo quando ele contém
  imagens Base64.
- Documentação passa a explicar limites, privacidade, escolha de cenas,
  seleção correta da peça arrastável e validação WebGL com capturas.

## [0.1.2] - 2026-09-29

### Adicionado

- Exercício opcional e isolado com botão, campo de texto e peça arrastável para
  praticar a configuração antes de modificar o jogo real.
- Comando **LUDUS > Tutorial > Adicionar exercício de interações**, seguro
  contra duplicação dos objetos na cena tutorial.
- Câmera neutra criada somente no tutorial para manter a Game View limpa sem
  depender das cenas ou câmeras do jogo real.
- Compatibilidade visual no Editor quando o novo Input System não encaminha a
  posição do ponteiro ao EventSystem da cena tutorial.
- Limites visuais do exercício de arraste passam a usar o mesmo espaço local
  da área e da peça, impedindo que ela escape do painel.
- Proteção exclusiva do tutorial que encerra e serializa uma sessão ainda ativa
  quando a pessoa sai do Play Mode sem usar o botão de encerramento.

### Alterado

- A documentação esclarece que o nome exibido no Dashboard começa com o nome
  do GameObject e pode ser personalizado sem alterar a Hierarchy.
- O tutorial passa a orientar a inspeção do asset de configuração fictício e a
  separá-lo explicitamente da configuração criada depois para o jogo real.
- A validação Web detalha a ativação do perfil, a lista de cenas, o primeiro
  tempo de compilação e a execução isolada do tutorial.

## [0.1.1] - 2026-09-25

### Alterado

- O botão **Documentation** do Package Manager passa a abrir o guia renderizado
  no navegador, preservando o arquivo Markdown do pacote como fallback offline.
- O botão **Changelog** passa a abrir o histórico da versão no navegador.
- As instruções de instalação passam a usar a tag fixa da versão de avaliação,
  em vez de acompanhar alterações futuras da branch `main`.

## [0.1.0] - 2026-09-25

### Alterado

- O Inspector da versão de avaliação apresenta somente recursos com coleta
  disponível e mantém capacidades reservadas desativadas no contrato.
- Textos do Editor e da documentação deixam de sugerir recursos futuros ou
  incompletos para a pessoa desenvolvedora avaliadora.

### Adicionado

- Guia definitivo de integração end to end revisado com instalação via UPM,
  configuração exata do Inspector, ciclo de vida por código ou UnityEvent,
  requisitos de UI/objetos 2D, teste WebGL e importação segura no Dashboard.
- Download WebGL com nome legível formado por jogo, rótulo opcional e data/hora de encerramento.
- Fluxo de sessão para importação manual sem o integrador informar ID técnico do aluno na Unity.

- Coletor opcional para o novo Input System, selecionado automaticamente ao criar a base de coleta.
- Botões visíveis na aba Game da amostra de laboratório para iniciar e encerrar a sessão fictícia.

- Estrutura inicial do pacote Unity Package Manager.
- Assembly próprio para o código de Runtime.
- Contratos neutros de configuração, capacidades e participante.
- Modelo interno canônico de sessão e registros de telemetria.
- Serializador JSON canônico com validação local de sessão e payloads de eventos.
- Testes EditMode para serialização canônica e rejeição de payload inválido.
- API de ciclo de vida para iniciar, encerrar e exportar sessões localmente.
- Contextos genéricos de captura com início, troca e encerramento automáticos.
- Registro local de cliques e trajetória do mouse condicionado ao contexto ativo.
- Componente Unity para configurar, iniciar e encerrar sessões pelo Inspector ou código.
- Componente opcional de captura de mouse e clique com o sistema de Input clássico da Unity.
- Componente de Inspector para delimitar contextos de captura por objeto ativo.
- Exportador opcional para POST público de telemetria e fallback local por sessão.
- Guia inicial de integração e amostra fictícia de laboratório.
- Assistente de criação e rótulos em português para a configuração pelo Inspector.
- A base de coleta agora cria e conecta automaticamente a configuração do jogo.
- Configuração simplificada com nome do jogo, envio à plataforma e cópia local opcional.
- Capacidades sem coletor nesta versão permanecem desativadas por padrão.
- Inspector apresenta a coleta disponível e explica o destino da sessão.
- Recortes localizam a base LUDUS automaticamente e usam o nome do objeto como título padrão.
- Amostra de laboratório também localiza automaticamente a base LUDUS.
- Amostra abre seu recorte fictício automaticamente ao iniciar a sessão.
