# Regras do Infinite Coffee

Estas regras valem para todo trabalho neste repositorio.

## Contexto tecnico

- Aplicacao ASP.NET Core MVC em .NET 8.
- Views Razor em `InfiniteCoffee2/Views`.
- Estilos estaticos em `InfiniteCoffee2/wwwroot`.
- Acesso ao SQL Server centralizado em `InfiniteCoffee2/Data/Banco.cs`.
- O banco usa procedures armazenadas e chaves estrangeiras para manter integridade.

## Regras de desenvolvimento

- Inspecione Controller, View, Banco.cs e README antes de alterar um fluxo existente.
- Preserve as rotas, nomes de campos, parametros de procedures e contratos entre Controller e View.
- Prefira a menor alteracao correta; nao reescreva a arquitetura sem necessidade concreta.
- Nao coloque senhas, tokens ou strings de conexao com credenciais no repositorio.
- Toda operacao de escrita deve validar entrada e retornar feedback compreensivel ao usuario.
- Exclusoes devem ser `POST`, pedir confirmacao na interface e respeitar historico e chaves estrangeiras.
- Nao apague pedidos, pagamentos ou itens relacionados sem autorizacao explicita; quando autorizado, use transacao e documente a perda de historico.
- Ao alterar uma View, preserve a acessibilidade basica, responsividade e as acoes existentes.
- Depois de alterar C# ou Razor, execute `dotnet build InfiniteCoffee2.slnx --no-restore`.
- Se o servidor local estiver em execucao e bloquear o build, reinicie o processo antes de validar.

## Regras de banco de dados

- Antes de alterar schema ou procedure, consulte dependencias, foreign keys e dados existentes.
- Nunca execute `DROP DATABASE`, `TRUNCATE`, `DELETE` sem filtro ou alteracoes destrutivas sem confirmacao explicita.
- Prefira migrations ou scripts versionados e idempotentes; documente ordem de execucao.
- Teste inserts, updates, deletes e consultas em uma base de desenvolvimento.
- Preserve dados historicos de pedidos e pagamentos.
- Ao encontrar erro de integridade referencial, explique a causa e implemente tratamento explicito; cascade destrutivo exige autorizacao explicita.

## Git e publicacao

- O remoto oficial e `https://github.com/pideias/pideias-infinittecoffee.git`.
- Todo push deste projeto deve ser feito na branch `CdmEdu`.
- Antes de commit, confira `git status`, `git diff` e o resultado do build.
- Nao use `git reset --hard`, `git checkout --` ou force push para apagar trabalho existente.
- Use mensagens de commit curtas e descreva uma mudanca coesa.
