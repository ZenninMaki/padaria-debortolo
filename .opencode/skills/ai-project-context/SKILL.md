---
name: ai-project-context
description: Use when an AI or new developer needs to understand, modify, test, or document the complete Infinite Coffee project.
---

# Contexto para IAs

1. Leia `AGENTS.md` e `DOCUMENTACAO-IA.md` antes de alterar codigo.
2. Identifique se a tarefa pertence ao MVC/API SQL Server ou ao app Flutter/Hive.
3. Preserve contratos existentes: nomes de campos em portugues, rotas, procedures e mensagens.
4. Para banco, leia `Banco.cs`, consulte foreign keys e nunca presuma que uma exclusao pode ser fisica.
5. Para Flutter, leia `InventoryApi`, `LocalDatabase`, `SyncService`, `InventoryRepository` e a tela afetada.
6. Trate SQL Server como fonte da verdade e Hive como cache/espelho offline.
7. Documente alteracoes de contrato, sincronizacao, rede ou schema no `DOCUMENTACAO-IA.md`.
8. Nao invente credenciais, tabelas ou endpoints; confirme no codigo e no README.
9. Ao terminar, informe arquivos alterados, validacoes executadas e limitacoes conhecidas.
