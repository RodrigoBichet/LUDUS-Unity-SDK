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

Esta versão de avaliação não registra acertos, erros, fases, categorias ou
objetivos pedagógicos. O escopo validado é a coleta das interações observáveis
descritas neste guia.

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
- Este roteiro visual foi validado na **Unity 6.3 LTS (6000.3.25f1)**. Em
  outras versões, nomes e posições de menus podem variar; nessa versão, por
  exemplo, os elementos de interface aparecem como **UI (Canvas)** no menu de
  contexto da Hierarchy.
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
   https://github.com/RodrigoBichet/LUDUS-Unity-SDK.git#v0.1.2
   ```

5. Aguarde a importação terminar sem erros.

Se o repositório estiver privado, o computador precisa ter acesso ao GitHub.
Nunca coloque token ou senha em cenas, scripts ou arquivos versionados.

## 3. Fazer o teste isolado recomendado

Antes de mexer no jogo real:

1. Abra **LUDUS > Criar tutorial de teste do SDK**.
2. Confirme a criação dos arquivos em `Assets/LUDUS/Tutorial/`.
3. Abra a cena `TutorialLudus` criada.
4. Selecione `Assets/LUDUS/Tutorial/ConfiguracaoTutorialLudus.asset` e observe
   no Inspector que o tutorial usa o nome **Tutorial LUDUS**, não envia sessões
   para a plataforma e preserva uma cópia local. Esse asset serve somente ao
   exercício e não deve ser reutilizado como configuração do jogo real.
5. Para praticar a configuração completa, use **LUDUS > Tutorial > Adicionar
   exercício de interações**. Esse passo opcional cria, somente no tutorial,
   um botão, um campo de texto e uma peça arrastável.
6. Abra **LUDUS > Configurar interações desta cena** e adicione os três objetos
   criados, arrastando cada um da Hierarchy para o campo **Arraste um objeto
   aqui** da janela **Interações LUDUS**.
7. Entre no Play Mode.
8. Inicie a sessão fictícia pelos controles exibidos na aba **Game**.
9. Clique no botão, conclua o campo de texto e arraste a peça.
10. Antes de sair do Play Mode, clique em **Encerrar sessão e exibir JSON**.
11. Confirme no Console que foi produzido um JSON LUDUS.

Mais adiante, **LUDUS > Adicionar coleta ao meu jogo** criará um asset separado
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
importação manual. Se optar pelo envio direto em um ambiente autorizado, a URL
deve conter somente a origem do backend, sem acrescentar `/api`.

Se escolher **Somente cenas selecionadas**, adicione todas as cenas de atividade
que realmente devem ser acompanhadas. Os nomes precisam corresponder às cenas
do projeto e essas cenas também precisam estar no Build Profile.

Em **Coleta essencial**, habilite apenas o que deseja registrar: cliques,
trajetória do ponteiro e arrastes. O Inspector desta versão apresenta somente
recursos cuja coleta está disponível. Outros campos permanecem no contrato JSON
por compatibilidade, desativados e sem exigir configuração da pessoa
desenvolvedora.

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

1. Abra **File > Build Profiles** e selecione **Web** na lista de plataformas.
2. Se Web ainda não tiver o selo **Active**, clique em **Switch Profile** — a
   interface de algumas versões usa **Switch Platform** — e aguarde a troca de
   plataforma terminar.
3. Clique em **Open Scene List**. Com `TutorialLudus` aberta, use **Add Open
   Scenes**, desabilite ou remova as demais cenas para este teste isolado e
   deixe `TutorialLudus` habilitada no índice `0`.
4. Volte ao perfil **Web** e use **Shorter Build Time** em **Code
   Optimization** durante a validação.
5. Confirme no asset LUDUS que **Baixar arquivo JSON ao encerrar (WebGL)** está
   marcado.
6. Execute **Build And Run**.
7. Repita o fluxo completo no navegador.
8. Encerre a sessão pelo botão ainda dentro da página e confirme o download do
   JSON. Fechar a aba não substitui essa ação explícita.
9. Guarde o arquivo apenas durante o teste.

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
- Acertos, erros, fases, categorias e objetivos pedagógicos estão fora do
  escopo desta versão de avaliação e permanecem desativados no contrato.
- Testar no Editor é necessário, mas o aceite final de uma integração WebGL
  exige um Build And Run e a validação do JSON no Dashboard.
