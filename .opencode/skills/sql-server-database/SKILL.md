---
name: sql-server-database
description: Use when working with SQL Server, infiniteCoffee, Banco.cs, stored procedures, foreign keys, CRUD persistence, database setup, data integrity, or SQL errors.
---

# SQL Server do Infinite Coffee

Trate o banco como sistema persistente e compartilhado, nunca como dados descartaveis de teste.

## Diagnostico

- Leia `Banco.cs` e `README.md` para confirmar servidor, database, procedures e schema.
- Use consultas somente leitura para descobrir constraints, dependencias e quantidade de registros relacionados.
- Diferencie SSMS fechado do servico `SQL Server (MSSQLSERVER)` parado.
- Reproduza o erro com o menor registro possivel e sem alterar dados reais.

## Alteracoes seguras

- Use parametros em comandos SQL; nao concatene entrada do usuario.
- Mantenha nomes e parametros das procedures compativeis com o C# existente.
- Nao use cascade delete para contornar erro de foreign key sem aprovacao explicita.
- Para produtos com historico, use inativacao (`ativo = 0`) e estoque zero; nunca apague vendas ou itens.
- Scripts de schema devem ser versionados, revisaveis e, quando possivel, idempotentes.
- Nunca inclua credenciais em scripts, commits ou mensagens de erro.

## Validacao

- Confirme que a conexao abre no ambiente local.
- Valide leitura, inclusao, alteracao e exclusao de um registro sem dependencias.
- Valide que a tentativa de excluir registro referenciado falha de forma controlada.
- Rode o build da aplicacao depois de mudar o acesso ao banco.
- Ao alterar produto, valide que ele desaparece das consultas ativas sem desaparecer dos relatorios historicos.
