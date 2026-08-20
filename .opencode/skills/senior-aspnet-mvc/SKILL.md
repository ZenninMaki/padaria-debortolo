---
name: senior-aspnet-mvc
description: Use when developing or reviewing this ASP.NET Core MVC project, especially Controllers, Razor Views, Banco.cs, routing, validation, CRUD flows, or visual changes.
---

# Senior ASP.NET MVC

Atue como desenvolvedor senior neste projeto Infinite Coffee.

## Fluxo obrigatorio

1. Leia a View, o Controller, o modelo de dados usado e as procedures relacionadas antes de editar.
2. Identifique o contrato do fluxo: rota, verbo HTTP, nomes dos campos, redirects, TempData e formato de dados.
3. Faça a menor mudanca que resolve o problema sem quebrar outros CRUDs.
4. Para formularios, use `POST` para mutacoes, validacao no servidor e mensagens de erro uteis.
5. Para Views, reutilize o tema existente e teste desktop e mobile.
6. Rode `dotnet build InfiniteCoffee2.slnx --no-restore` ao terminar.

## Criterios de revisao

- Verifique falhas de null, conversoes de tipo, ids invalidos e listas vazias.
- Verifique se links de exclusao nao executam mutacoes via GET.
- Verifique se excecoes do banco nao vazam stack trace ou detalhes sensiveis para o usuario.
- Preserve historico e integridade referencial.
- Nao introduza dependencias ou frameworks novos sem necessidade.
- Nao substitua a camada `Banco` inteira por Entity Framework sem solicitacao explicita.

## Entrega

Informe arquivos alterados, comportamento preservado, validacao executada e qualquer limitacao restante.
