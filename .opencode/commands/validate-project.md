---
description: Valida o projeto ASP.NET MVC e verifica o estado do Git.
agent: build
---

Execute a validacao do projeto Infinite Coffee:

1. Verifique `git status --short` e inspecione alteracoes inesperadas.
2. Execute `dotnet build InfiniteCoffee2.slnx --no-restore`.
3. Se o build falhar por processo `InfiniteCoffee2` em execucao, informe o PID, reinicie somente o servidor local e repita.
4. Confirme que a branch atual e `CdmEdu` antes de qualquer publicacao.
5. Relate erros, avisos e arquivos modificados.
