# Guia definitivo: integrar o LUDUS Unity SDK do zero ao Dashboard

Este é o roteiro principal para uma pessoa desenvolvedora integrar o SDK em um
jogo Unity. Faça primeiro com um projeto ou cópia de teste e use somente nomes
fictícios. Não use dados reais, credenciais, JWT ou a base produtiva.

O roteiro considera um jogo **Unity 6, 2D e WebGL**. Ele também serve para
projetos que usam várias cenas, UI do Canvas, objetos 2D no mundo ou uma mistura
dessas abordagens.

## Resultado esperado

Ao terminar, o jogo deverá:

1. iniciar uma sessão LUDUS conscientemente;
2. registrar somente as capacidades habilitadas;
3. encerrar a sessão e baixar um JSON no WebGL;
4. ter esse JSON validado e importado no LUDUS Acompanha;
5. exibir a sessão e o mapa de interações no Dashboard.

O SDK não adivinha acertos, erros, fases ou objetivos. Informações semânticas só
podem existir quando o próprio jogo as fornece explicitamente.

### Caminho mínimo

Se você já conhece Unity, o fluxo completo pode ser resumido assim:

1. instalar o pacote pelo Package Manager;
2. executar o tutorial isolado;
3. adicionar uma única base LUDUS à primeira cena do jogo;
4. configurar cenas e capacidades no asset criado em `Assets/LUDUS/`;
5. ligar o início e o encerramento da sessão ao fluxo real do jogo;
6. opcionalmente marcar botões, campos e objetos relevantes;
7. testar no Editor e depois em um build WebGL;
8. importar o JSON no LUDUS Acompanha.

## 1. Preparar o ambiente

- Use Unity 6.
- Trabalhe em uma cópia limpa ou branch de integração do jogo.
- Confirme que o projeto abre e executa antes de instalar o SDK.
- Confirme que todas as cenas usadas estão no **Build Profile** do projeto.
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
O tutorial é material de laboratório: depois da validação, não inclua
`TutorialLudus` no build final do jogo.

## 4. Adicionar o coletor ao jogo

1. Abra uma cena representativa do jogo.
2. Use **GameObject > LUDUS > Adicionar coleta ao meu jogo**.
3. Salve a cena.
4. Confirme que foi criado um objeto chamado **LUDUS SDK** e um asset de
   configuração em `Assets/LUDUS/`.
5. Mantenha apenas **uma base LUDUS** no jogo. Ela persiste durante as trocas de
   cena; não repita o comando em todas as cenas.

Selecione o objeto **LUDUS SDK** e configure o Inspector:

| Campo | Valor recomendado no primeiro teste |
| --- | --- |
| **Nome do jogo** | nome estável e reconhecível, por exemplo `Meu Jogo Educacional` |
| **Enviar sessões para LUDUS Acompanha** | desmarcado |
| **Guardar também uma cópia de segurança local** | marcado durante a validação |
| **Baixar arquivo JSON ao encerrar (WebGL)** | marcado |
| **Rótulo do arquivo (opcional)** | vazio ou um rótulo curto, sem dados pessoais |
| **Capturar automaticamente em** | todas as cenas ou somente as selecionadas |

Em **Conexão com ambiente LUDUS (avançado)**, deixe a URL vazia no fluxo de
importação manual. Quando o envio direto for configurado futuramente, a URL
deverá conter somente a origem do backend, sem acrescentar `/api`.

Se escolher **Somente cenas selecionadas**, adicione todas as cenas de atividade
que realmente devem ser acompanhadas. Os nomes precisam corresponder às cenas
do projeto e essas cenas também precisam estar no Build Profile.

Em **Coleta essencial**, habilite apenas o que o jogo realmente oferece. Para a
primeira validação, cliques, trajetória do ponteiro e arrastes são suficientes.
Não marque recursos avançados como acerto/erro, categorias ou fases apenas para
fazê-los aparecer no JSON: essas capacidades exigem eventos que o jogo forneça.

Se a base já existia e o sistema de entrada do projeto mudou, selecione-a e use
**GameObject > LUDUS > Atualizar coletor de mouse da base selecionada**.

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

## 6. Ligar o ciclo de vida da sessão ao jogo

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

## 7. Validar no Editor

1. Inicie uma sessão com identidade e atividade fictícias.
2. Teste pelo menos um botão, um movimento e um arraste configurado.
3. Se houver campo de texto, digite apenas conteúdo fictício e confirme que o
   texto não aparece no JSON.
4. Encerre a sessão uma única vez.
5. Confirme `schemaVersion`, `captureMode: "sdk"`, `source`, `capabilities`,
   duração, viewport e coleções esperadas.
6. Verifique que nada do jogo mudou por causa do coletor.

Na Console, erros do SDK começam com `[LUDUS]`. Um JSON válido deve possuir pelo
menos início e fim coerentes, `gameId`, plataforma, duração e as coleções
correspondentes às capacidades habilitadas.

## 8. Validar no WebGL

1. Abra **File > Build Profiles** e selecione o perfil **Web**.
2. Confirme que as cenas do fluxo estão incluídas e na ordem correta.
3. Confirme no asset LUDUS que **Baixar arquivo JSON ao encerrar (WebGL)** está
   marcado.
4. Execute **Build And Run**.
5. Repita o fluxo completo no navegador.
6. Encerre a sessão por uma ação do usuário e confirme o download do JSON.
7. Guarde o arquivo apenas durante o teste.

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
- **O download não ocorreu no WebGL:** confirme a opção de download no asset,
  encerre a sessão a partir de uma ação permitida pelo navegador e teste sem
  bloqueadores.
- **A cópia local não apareceu em Downloads:** a cópia de segurança local não é
  o download do navegador. Para importação manual no WebGL, marque explicitamente
  **Baixar arquivo JSON ao encerrar (WebGL)**.
- **O Dashboard recusou o arquivo:** não edite o JSON manualmente; preserve o
  contrato gerado e consulte a mensagem de validação.

## Limites desta versão

- O SDK coleta evidências de interação e apoia o acompanhamento pedagógico; ele
  não diagnostica, classifica clinicamente nem mede aprendizagem de forma
  conclusiva.
- Acertos, erros, fases, categorias e objetivos não podem ser deduzidos apenas
  por cliques ou imagens. O jogo precisa emitir esses eventos explicitamente.
- Testar no Editor é necessário, mas o aceite final de uma integração WebGL
  exige um Build And Run e a validação do JSON no Dashboard.
