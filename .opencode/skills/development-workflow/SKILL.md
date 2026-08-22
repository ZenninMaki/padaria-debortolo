---
name: development-workflow
description: Use for any implementation, review, documentation, testing, or release task in Infinite Coffee.
---

# Workflow de Desenvolvimento

1. Confira `git status` e nao reverta alteracoes de outro trabalho.
2. Leia regras, documentacao e o fluxo completo antes de editar.
3. Prefira a menor mudanca correta e comentarios que expliquem motivo, fluxo ou integridade.
4. C# e Razor: compile com `dotnet build InfiniteCoffee2.slnx --no-restore` e reinicie o servidor.
5. Flutter/Dart: rode `dart format`, `flutter analyze`, `flutter test` e o build necessario.
6. API: valide status HTTP, JSON, erros de validacao, transacao e foreign keys.
7. Sync: teste push, pull incremental, fila offline, reconexao e conflito de estoque.
8. Produto excluido deve ser inativado (`ativo = 0`), preservando vendas e auditoria.
9. Nunca execute limpeza, `DELETE` amplo, `TRUNCATE` ou `DROP` sem confirmacao explicita.
10. Commit e push somente na branch `kaio`, quando o usuario solicitar.
11. O workflow `Validate` deve validar pushes em `kaio`, PRs para `master`, backend e Flutter.
