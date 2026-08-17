# Guia definitivo: integrar o LUDUS Unity SDK do zero ao Dashboard

Este é o roteiro principal para uma pessoa desenvolvedora integrar o SDK em um
jogo Unity. Faça primeiro com um projeto ou cópia de teste e use somente nomes
fictícios. Não use dados reais, credenciais, JWT ou a base produtiva.

## Resultado esperado

Ao terminar, o jogo deverá:

1. iniciar uma sessão LUDUS conscientemente;
2. registrar somente as capacidades habilitadas;
3. encerrar a sessão e baixar um JSON no WebGL;
4. ter esse JSON validado e importado no LUDUS Acompanha;
5. exibir a sessão e o mapa de interações no Dashboard.

O SDK não adivinha acertos, erros, fases ou objetivos. Informações semânticas só
podem existir quando o próprio jogo as fornece explicitamente.

## 1. Preparar o ambiente

- Use Unity 6.
- Trabalhe em uma cópia limpa ou branch de integração do jogo.
- Confirme que o projeto abre e executa antes de instalar o SDK.
- Para o primeiro teste, não configure API de produção. Use download manual do
  JSON e um ambiente de Dashboard com banco temporário ou demonstrativo.

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

## 3. Fazer o teste isolado recomendado

Antes de mexer no jogo real:

1. Abra **LUDUS > Criar tutorial de teste do SDK**.
2. Confirme a criação dos arquivos em `Assets/LUDUS/Tutorial/`.
3. Abra a cena `TutorialLudus` criada.
4. Entre no Play Mode.
5. Inicie a sessão fictícia pelos controles exibidos na aba **Game**.
6. Clique, mova o ponteiro e use os elementos fictícios da cena.
7. Encerre a sessão.
8. Confirme no Console que foi produzido um JSON LUDUS.

Esse teste comprova que o pacote funciona sem depender das regras do seu jogo.

## 4. Adicionar o coletor ao jogo

1. Abra uma cena representativa do jogo.
2. Use **GameObject > LUDUS > Adicionar coleta ao meu jogo**.
3. No arquivo de configuração criado em `Assets/LUDUS/`, informe um nome técnico
   estável para o jogo.
4. Escolha se a coleta vale para todas as cenas ou somente para cenas
   selecionadas.
5. No primeiro end to end, mantenha o envio automático desativado e o download
   manual habilitado.

O endereço da API, quando for usado futuramente, deve conter apenas a origem do
backend, sem acrescentar `/api`. Não configure a API produtiva durante testes.

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

Se a atividade ocupar apenas parte da tela, use um `LudusCaptureContextTrigger`
para delimitar o contexto. Contextos são opcionais e não representam, por si
só, acerto, erro ou objetivo pedagógico.

## 6. Ligar o ciclo de vida da sessão ao jogo

Para um fluxo de importação manual, chame o SDK quando o jogador realmente
começar e terminar a atividade:

```csharp
using LudusSDK;
using UnityEngine;

public sealed class MeuFluxoLudus : MonoBehaviour
{
    public void IniciarAtividade()
    {
        if (!LudusSdk.TryStartSessionForManualImport("Atividade 1", out string erro))
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

Não inicie a sessão apenas porque uma cena carregou se a atividade ainda não
começou. Não a encerre antes da última interação que deve pertencer à sessão.

## 7. Validar no Editor

1. Inicie uma sessão com identidade e atividade fictícias.
2. Teste pelo menos um botão, um movimento e um arraste configurado.
3. Se houver campo de texto, digite apenas conteúdo fictício e confirme que o
   texto não aparece no JSON.
4. Encerre a sessão uma única vez.
5. Confirme `schemaVersion`, `captureMode: "sdk"`, `source`, `capabilities`,
   duração, viewport e coleções esperadas.
6. Verifique que nada do jogo mudou por causa do coletor.

## 8. Validar no WebGL

1. Troque o Build Profile para WebGL.
2. Execute **Build And Run**.
3. Repita o fluxo completo no navegador.
4. Encerre a sessão e confirme o download do JSON.
5. Guarde o arquivo apenas durante o teste.

O teste no Editor não substitui o WebGL: foco, coordenadas, download e sistema
de entrada podem se comportar de forma diferente no navegador.

## 9. Importar e conferir no Dashboard

Siga o guia do Dashboard:

`LUDUSAcompanha/docs/GUIA_IMPORTACAO_END_TO_END.md`

Use um participante fictício em ambiente com banco temporário. Depois da
importação, confira a sessão, as capacidades declaradas e o mapa. Só espere
eventos semânticos que tenham sido informados explicitamente pelo jogo.

## Checklist de aceite

- [ ] O projeto continuou funcionando antes e depois da instalação.
- [ ] O tutorial isolado gerou JSON.
- [ ] O jogo inicia e encerra a sessão em momentos conscientes.
- [ ] Somente interações escolhidas aparecem no JSON.
- [ ] Nenhum conteúdo digitado foi armazenado.
- [ ] O WebGL baixou o JSON.
- [ ] O Dashboard validou e importou o arquivo.
- [ ] O mapa e as capacidades correspondem ao que realmente foi capturado.
- [ ] Nenhum dado real, segredo ou ambiente produtivo foi usado.

## Problemas comuns

- **O menu LUDUS não apareceu:** espere a compilação terminar e corrija erros
  anteriores do projeto.
- **Não há interações no JSON:** confira se a sessão estava ativa, se os itens
  foram configurados e se o contexto correto estava aberto.
- **O objeto arrastável não se move:** isso é responsabilidade do jogo; o SDK
  apenas observa o gesto.
- **O download não ocorreu no WebGL:** confirme que a sessão foi encerrada a
  partir de uma ação permitida pelo navegador e teste sem bloqueadores.
- **O Dashboard recusou o arquivo:** não edite o JSON manualmente; preserve o
  contrato gerado e consulte a mensagem de validação.

