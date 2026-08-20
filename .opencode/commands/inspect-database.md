---
description: Inspeciona com seguranca a conexao, procedures e relacionamentos do banco infiniteCoffee.
agent: build
---

Inspecione o banco sem alterar dados:

1. Leia `InfiniteCoffee2/Data/Banco.cs` e `README.md`.
2. Confirme o servico SQL Server no Windows.
3. Use `sqlcmd` apenas com consultas `SELECT`, `sp_helptext` ou metadados de constraints.
4. Nao execute INSERT, UPDATE, DELETE, DROP, TRUNCATE ou ALTER.
5. Relacione cada erro de integridade referencial a tabela pai, tabela filha e procedure envolvida.
6. Apresente a causa e uma correcao segura, sem apagar historico.
