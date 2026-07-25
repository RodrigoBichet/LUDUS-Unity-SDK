# Changelog

Todas as mudanças relevantes do LUDUS Unity SDK serão registradas neste arquivo.

## [Unreleased]

### Adicionado

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
- Capacidades ainda não implementadas deixam de ser habilitadas por padrão.
- Inspector separa coleta essencial de opções avançadas e explica o destino da sessão.
- Recortes localizam a base LUDUS automaticamente e usam o nome do objeto como título padrão.
