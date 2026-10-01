# Changelog

Todas as mudanças relevantes do LUDUS Unity SDK serão registradas neste arquivo.

## [Não publicado]

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
