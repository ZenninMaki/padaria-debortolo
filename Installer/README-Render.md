# Deploy gratuito da API no Render

Este modo hospeda somente a API de consulta. O SQL Server continua no desktop e
o PC envia `estoque.json` ao Google Drive a cada hora.

## Preparar o arquivo no Drive

1. Confirme que o desktop já criou `estoque.json`.
2. No arquivo, abra **Compartilhar**.
3. Em **Acesso geral**, selecione **Qualquer pessoa com o link** e **Leitor**.
4. Copie a URL do arquivo. O valor entre `/d/` e `/view` é o `FILE_ID`.

Não publique nesse arquivo clientes, pagamentos, senhas ou outros dados sensíveis.
Ele deve conter apenas catálogo e saldo de estoque.

## Criar o serviço

1. No Render, escolha **New → Blueprint**.
2. Selecione o repositório `pideias/padaria-debortolo`.
3. Escolha a branch `CdmEdu`.
4. O Render usará o `render.yaml` e o `Dockerfile` da raiz.
5. Escolha o plano **Free**.

Configure as variáveis secretas do serviço:

```text
GOOGLE_DRIVE_SNAPSHOT_FILE_ID=ID_DO_ARQUIVO_estoque.json
PADARIA_API_TOKEN=token-administrativo-longo
PADARIA_READONLY_TOKEN=token-somente-leitura-do-mobile
```

O `PADARIA_SNAPSHOT_ONLY` já fica definido como `true` no `render.yaml`.

## Testar

Depois do deploy, teste:

```text
https://SEU-SERVICO.onrender.com/api/health
https://SEU-SERVICO.onrender.com/api/estoque/snapshot
```

Os tokens continuam disponiveis para uso futuro, mas a API demonstrativa atualmente aceita
as chamadas sem header `X-Api-Key`.

```text
X-Api-Key: opcional
```

O Render neste modo e somente leitura. Entradas e saídas devem ser enviadas para a API local
que acessa o SQL Server. O desktop publica o snapshot do SQL Server no Drive.

O serviço gratuito pode dormir após 15 minutos sem acesso. O primeiro acesso
depois disso pode levar cerca de um minuto.
