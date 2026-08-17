# Amostra: Integração Básica

Esta amostra serve apenas para validar o fluxo com uma identidade fictícia. Não use seus identificadores como modelo para dados reais.

## Montagem

1. Use **GameObject > LUDUS > Adicionar coleta ao meu jogo**. O objeto `LUDUS SDK`, as referências internas e a configuração em `Assets/LUDUS` são criados automaticamente.
2. Abra a configuração criada no painel Project e preencha um nome fictício para o jogo.
3. Adicione `LudusBasicSessionExample` ao objeto `LUDUS SDK`.

## Execução

1. Entre no Play Mode.
2. Na aba **Game**, clique em **Iniciar sessão fictícia**.
3. Mova o mouse e clique dentro da janela Game.
4. Clique em **Encerrar sessão e exibir JSON**.

Ao iniciar, a amostra abre automaticamente um recorte chamado `Atividade de teste`; não é necessário criar Canvas, painel ou objeto adicional. O Console deve exibir o JSON. Com `apiBaseUrl` vazia e fallback habilitado, o exportador tentará salvar uma cópia local. Não configure URL de produção nem use aluno real nesta amostra.

Para integrar um jogo completo e validar o JSON no Dashboard, siga
`Documentation~/GUIA_INTEGRACAO_END_TO_END.md` na raiz do pacote.
