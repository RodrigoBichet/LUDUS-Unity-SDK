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

Acertos, erros, fases, categorias e objetivos pedagógicos só são registrados
quando o próprio jogo os informa pela API ou pela ponte semântica visual. Sem
essa integração explícita, o escopo continua sendo apenas a coleta das
interações observáveis descritas neste guia.

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
   https://github.com/RodrigoBichet/LUDUS-Unity-SDK.git#v0.1.4
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
10. Antes de sair do Play Mode, clique em **Encerrar sessão e gerar JSON**.
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
| **Capturar imagem da tela** | desmarcado no primeiro teste; habilite somente com finalidade e autorização |

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

Em **Capturas visuais**, a imagem da Game View é opcional e permanece
desativada por padrão. Quando habilitada, escolha todas as cenas acompanhadas
ou arraste somente as cenas cuja imagem realmente ajuda a interpretar o mapa.
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

Adicione **LUDUS > Fluxo > Sessão acompanhada** a um objeto que permaneça
ativo durante toda a atividade. Por padrão, ele inicia uma sessão para
importação manual ao ser ativado e a encerra ao ser desativado ou quando sua
cena é fechada.

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
resultado, adicione **LUDUS > Ponte semântica do jogo** a um objeto da atividade.
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
**LUDUS > Adaptadores > Resultado de arraste por tag**, escolha a tag e indique
uma Ponte semântica compartilhada.
7. teste uma peça com a tag correta e outra com uma tag diferente.

O adaptador usa a peça informada por `PointerEventData.pointerDrag`. Ele apenas
observa o drop: não move a peça, não determina sua posição final e não executa
a resposta visual do jogo. Se o jogo usa física, triggers, raycasts ou código
próprio sem `OnDrop`, este adaptador não se aplica.

#### 6.4. Trigger ou colisão avaliado por tag

Adicione **LUDUS > Adaptadores > Resultado de contato por tag** ao objeto que
recebe o contato. Escolha `Trigger2D`, `Collision2D`, `Trigger3D` ou
`Collision3D`, selecione a tag correta e indique a resposta esperada. Mantenha
a busca nos objetos pais quando o collider estiver em um filho da peça.

Colliders, rigidbodies, `Is Trigger`, layers e a matriz de colisão continuam sob
responsabilidade do jogo. O adaptador apenas observa o callback e não altera a
física ou a posição dos objetos.

#### 6.5. Alternativa correta ou incorreta em botão

Em cada alternativa com `Button`, adicione **LUDUS > Adaptadores > Resultado de
botão**. Escolha se ela representa acerto ou erro e informe os nomes exibidos
no Dashboard. O componente preserva todos os listeners existentes de
`Button.onClick`.

#### 6.6. Meta por valor ou pontuação

Adicione **LUDUS > Adaptadores > Meta por valor ou pontuação** ao controlador
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
- [ ] Capturas visuais, quando habilitadas, mostram apenas conteúdo autorizado.
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
- **A trajetória de arraste existe, mas a peça não é identificada:** o coletor
  global registrou o gesto, porém o marcador semântico está no objeto errado.
  Selecione a peça que realmente recebe o arraste, não a área de destino.
- **A imagem não apareceu:** confirme que a capacidade está habilitada, que a
  cena foi marcada para imagem e que a sessão não foi encerrada enquanto a
  captura ainda estava pendente.
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
- Acertos, erros, fases, categorias e objetivos pedagógicos permanecem
  desativados por padrão e só devem ser habilitados quando forem informados
  explicitamente pelo jogo pela API ou pela ponte semântica.
- Capturas visuais são opcionais, limitadas e devem ser usadas somente com
  finalidade definida e autorização adequada.
- Testar no Editor é necessário, mas o aceite final de uma integração WebGL
  exige um Build And Run e a validação do JSON no Dashboard.
