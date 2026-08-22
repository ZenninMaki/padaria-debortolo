---
name: sync-architecture
description: Use when planning or implementing the embedded local database (Hive) inside the Flutter app and the bidirectional sync between the app, the local web server (SQL Server) and the network. Covers target platforms Windows + mobile from one Flutter codebase.
---

# Arquitetura de Banco Embutido + Sync Bidirecional

## Objetivo
O app (APK/Windows) deve vir com o banco já incluso e funcionar offline, sincronizando
de forma bidirecional com o SQL Server através da API web local. Tudo que foi feito
offline se auto-alimenta no banco web quando há rede, e vice-versa.

## Banco embutido no app (offline-first)
- Usar **Hive** (banco puro Dart, embutido, sem SQLite) para o armazenamento local offline.
- O app ja vem populado apos o primeiro pull (seed) do servidor; funciona offline.
- Para "vir com o banco incluso":
     - **Recomendado:** no primeiro launch o app faz o seed via `GET /api/sync/pull` e
    popula as boxes do Hive; nas próximas açoes trabalha offline.
  - Alternativa: app sobe vazio e faz `pull` inicial de `/api/produtos` na primeira conexão.
  - Híbrido: embuta schema + poucos itens; catálogo completo no primeiro sync.

## Plataformas alvo (um único código Flutter)
- Pasta do app: `C:\Users\kaiof\Desktop\infinite_coffee_app` (espelhada em `InfiniteCoffeeMobile` no repo).
- Um só código gera:
  - **Windows:** `flutter build windows` → `build\windows\x64\runner\Release\infinite_coffee_app.exe` (já funcional).
  - **Mobile (Android):** `flutter build apk` → instalável no celular.
  - iOS opcional (requer macOS).
- **Conversa entre eles NÃO é peer-to-peer:** todos apontam para a MESMA API web
  (`/api/*` no SQL Server). Venda no mobile → push p/ servidor → Windows enxerga no pull. E vice-versa.

## Detalhe crítico de rede
- App Windows e servidor no mesmo PC: usar `http://localhost:5049`.
- App mobile (celular) na mesma rede: usar o IP da máquina na LAN, ex.: `http://192.168.1.10:5049`
  (localhost no celular NÃO é o PC).
- O CORS em `Program.cs` deve permitir a origem do celular (IP), não só `localhost:*`.

## Sync bidirecional (rede ⇄ app ⇄ web)
- **Servidor (SQL Server):** adicionar coluna `modified_at DATETIME DEFAULT GETUTCDATE()` em
  `Produtos`, `MovimentacoesEstoque`, etc., atualizada nas SPs de cadastro/edição/saída/entrada.
- **`GET /api/sync/pull?since={token}`:** devolve só linhas mudadas desde o token
  (evolui a ideia do endpoint `/versao`, retornando os dados, não só o hash).
- **`POST /api/sync/push`:** recebe lote de operações offline (vendas, saídas) com
  `client_uuid` + `created_at`; aplica transacionalmente e devolve IDs aceitos.
- **App (Hive):**
  - `sync_queue`: operações feitas offline com flag `synced=0`.
  - `sync_state`: guarda `last_pull` e `device_id`.
  - Motor de sync (a cada X s ou ao recuperar rede):
     1. Pull: `GET /api/sync/pull?since=last_pull` → upsert no Hive.
    2. Push: envia `sync_queue` pendente → marca `synced=1` com confirmação do servidor.
- **Resolução de conflito:** last-write-wins por `modified_at` no catálogo; vendas são
  append-only (nunca conflitam, só somam).

## Fluxo resumido
```
[App Hive] --push vendas/offline--> [API /sync/push] --> [SQL Server]
[App Hive] <--pull catálogo/estoque-- [API /sync/pull] <-- [SQL Server]
```
App funciona sem internet (vende, mexe no estoque) e, com rede, auto-alimenta o banco web
e recebe mudanças dos outros caixas. Ver também a skill `offline-sync`.

## Regras de implementação (herdadas do AGENTS.md)
- Não alterar telas/HTML/CSS existentes; novas funcionalidades em novos arquivos/rotas.
- SQL Server é autoritativo; Hive é espelho offline.
- Commits/pushes somente na branch `kaio`.
- Exclusao de produto e inativacao (`ativo = 0`), nunca remocao fisica de historico.
- Atualizacoes de tela devem ocorrer em segundo plano; nao substitua o conteudo visivel por spinner.
