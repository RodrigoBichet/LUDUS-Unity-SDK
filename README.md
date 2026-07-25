# LUDUS Unity SDK

SDK plugável para registrar telemetria de jogos educacionais Unity e gerar sessões compatíveis com a plataforma LUDUS Acompanha.

O SDK oferece evidências parciais para acompanhamento e mediação docente. Ele não faz diagnóstico, classificação clínica ou avaliação conclusiva de aprendizagem.

> Projeto em evolução. A API pública e o processo de integração podem mudar antes da primeira versão estável.

## O que ele faz

- inicia e encerra sessões no contrato LUDUS;
- registra cliques e trajetória do mouse quando habilitados;
- delimita recortes de observação por objetos ativos;
- gera um JSON por sessão;
- pode enviar para `POST /api/sessions` ou salvar fallback local.

O SDK não infere acertos, erros, fases ou objetivos: cada jogo informa suas próprias regras e eventos semânticos.

## Instalação local

No projeto Unity consumidor, abra `Packages/manifest.json` e inclua uma referência local:

```json
{
  "dependencies": {
    "br.edu.ufpel.ludus.sdk": "file:../LUDUS-Unity-SDK"
  }
}
```

Alternativamente, use **Window > Package Manager > + > Add package from disk** e selecione o `package.json` deste repositório.

## Integração mínima

### 1. Crie a base de coleta

Use **GameObject > LUDUS > Criar base de coleta**. O SDK cria um GameObject `LUDUS SDK`, adiciona os componentes necessários, conecta automaticamente o rastreador e o exportador ao controlador e cria uma configuração em `Assets/LUDUS/ConfiguracaoLudus.asset`.

Selecione esse arquivo no painel Project e informe o **Nome do jogo**, como `Meu Jogo Educacional`. O SDK gera internamente o `gameId` técnico exigido pelo contrato e usa a versão `1.0.0` até uma edição futura do jogo ser distribuída.

Não coloque JWT, senha ou credencial de usuário nesse asset.

Projetos que usam somente o novo Input System precisarão do adaptador específico, planejado para uma etapa posterior.

### 2. Delimite o recorte observado

Adicione `LudusCaptureContextTrigger` ao Canvas, painel ou objeto-raiz desejado. Preencha:

- **Título apresentado ao professor no dashboard**;
- **Tipo geral**: Scene, Canvas, Activity ou Other;
- **Objetivo observacional ou pedagógico**, se houver.

Enquanto o objeto estiver ativo em uma sessão, o SDK registra dados brutos. Ao desativá-lo, o contexto é encerrado. Para informar o controlador, arraste o GameObject `LUDUS SDK` criado no passo anterior. O identificador técnico é criado internamente; o integrador não preenche `contextId`.

### 3. Inicie e encerre pelo fluxo do jogo

Quando o jogo já souber quem está jogando, chame o controlador:

```csharp
using LudusSDK;
using UnityEngine;

public sealed class MeuFluxoDoJogo : MonoBehaviour
{
    [SerializeField] private LudusSessionController ludus;

    public void IniciarParaAluno(string studentId, string playerId)
    {
        if (!ludus.TryStartSession(studentId, playerId, out string erro))
        {
            Debug.LogError("[LUDUS] " + erro);
        }
    }

    public void EncerrarAtividade()
    {
        if (ludus.TryEndAndSerialize(out string json, out string erro))
        {
            Debug.Log("[LUDUS] Sessão finalizada: " + json);
            return;
        }

        Debug.LogError("[LUDUS] " + erro);
    }
}
```

`studentId` deve vir do fluxo seguro já existente no jogo ou plataforma. Não fixe identificadores reais em scripts, cenas, prefabs ou exemplos.

## Exportação e fallback

Com `LudusSessionExporter` configurado, a opção **Enviar sessões para LUDUS Acompanha** controla o envio à plataforma. A URL da API não é algo que o integrador comum deve preencher: ela será fornecida pela distribuição oficial/ambiente da plataforma. Enquanto essa conexão não estiver configurada, o SDK usa fallback local.

A opção **Salvar também uma cópia local** cria um arquivo local mesmo quando o envio for bem-sucedido. Independentemente dessa opção, uma falha de envio também gera fallback local.

O endpoint direto de telemetria não recebe JWT nesta etapa por compatibilidade. Testes de envio devem usar backend local e identidade fictícia; nunca dados reais ou MongoDB Atlas.

## Amostra de laboratório

No Package Manager, importe a amostra **Integração Básica**. Ela contém um componente com identidade fictícia e comandos no menu de contexto do Inspector. Leia `Samples~/BasicIntegration/README.md` antes de usar.

## Validação recomendada

1. Execute os testes EditMode do pacote.
2. Inicie uma sessão com identidade fictícia no laboratório.
3. Ative um contexto, mova o mouse e clique.
4. Encerre e confira o JSON no Console.
5. Teste fallback sem URL, em ambiente local controlado.
6. Teste envio apenas contra backend local e banco temporário.
7. Faça build WebGL antes de integrar um jogo real.
