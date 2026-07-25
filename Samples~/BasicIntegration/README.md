# Amostra: Integração Básica

Esta amostra serve apenas para validar o fluxo com uma identidade fictícia. Não use seus identificadores como modelo para dados reais.

## Montagem

1. Use **GameObject > LUDUS > Criar base de coleta**. O objeto `LUDUS SDK`, as referências internas e a configuração em `Assets/LUDUS` são criados automaticamente.
2. Abra a configuração criada no painel Project e preencha identificador e versão fictícios.
3. Adicione `LudusBasicSessionExample` ao mesmo objeto e arraste o próprio objeto `LUDUS SDK` para **Objeto controlador LUDUS SDK**.
4. Em um Canvas ou painel, adicione `LudusCaptureContextTrigger`, preencha título/tipo/objetivo e arraste o objeto `LUDUS SDK` para **Objeto controlador LUDUS SDK**.

## Execução

No menu de contexto do componente `LudusBasicSessionExample`, execute:

1. entre no Play Mode e mantenha o Canvas/painel ativo;
2. **LUDUS/Iniciar sessão fictícia**;
3. mova o mouse e clique;
4. **LUDUS/Encerrar sessão e exibir JSON**.

O Console deve exibir o JSON. Com `apiBaseUrl` vazia e fallback habilitado, o exportador tentará salvar uma cópia local. Não configure URL de produção nem use aluno real nesta amostra.
