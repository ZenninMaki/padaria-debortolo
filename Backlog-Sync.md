# Backlog — Banco Embutido + Sync Bidirecional (App Windows/Mobile ↔ Web)

> Objetivo: um único app Flutter (Windows `.exe` + mobile `.apk`) que já vem com o banco
> local Hive populado no primeiro sync, funciona offline e sincroniza de forma bidirecional com o SQL Server
> através da API web. A "conversa" entre Windows e celular passa sempre pelo servidor.

---

## 1. Visão geral da arquitetura

```
[App Windows / Mobile  -> Hive embutido]  --pull/push-->  [API /api/sync/*]  -->  [SQL Server]
                                              <--pull/push--                   <--  (fonte da verdade)
```

- **App** = espelho offline (Hive). Vende e mexe no estoque sem internet.
- **Servidor** = SQL Server (`localhost\KAIO`, `infiniteCoffee`), fonte autoritativa.
- **Ponte** = endpoints REST `/api/sync/*`.
- **Conversa entre plataformas** = indireta, via servidor (nunca peer-to-peer).

---

## 2. Stack — "com o quê" será feito

| Camada | Tecnologia | Para que |
|---|---|---|
| App multiplataforma | **Flutter 3.47 / Dart 3.13** | Um código gera Windows + Android |
| Banco local embutido | **Hive** + **hive_flutter** | Dados locais dentro do app/APK |
| Caminhos de arquivo | **path_provider** | copiar DB de `assets/` no 1º launch |
| Rede | **http** (ou **dio**) | chamar a API de sync |
| Conectividade | **connectivity_plus** | disparar sync ao recuperar rede |
| Servidor | **ASP.NET Core .NET 10** | endpoints `/api/sync/*` |
| Banco autoritativo | **SQL Server** (`localhost\KAIO`) | dados oficiais + `modified_at` |
| Builds | `flutter build windows` / `flutter build apk` | gerar `.exe` e `.apk` |
| Testes | **xUnit** (C#) + **flutter test** (Dart) | validar sync |
| CORS | `Program.cs` | liberar `localhost` + IP da LAN |

---

## 3. Fases e passos

### Fase 0 — Preparar o servidor (SQL Server)
1. Adicionar coluna de auditoria nas tabelas espelhadas:
   - `Produtos`: `modified_at DATETIME DEFAULT GETUTCDATE()` e `ativo BIT DEFAULT 1`
   - `MovimentacoesEstoque`: `modified_at`
   - (se aplicável) `Pedidos`, `Itens_Pedidos`, `Pagamentos`: `modified_at`
2. Ajustar SPs (`sp_CadastrarProduto`, `sp_AtualizarProduto`, saída/entrada) para
   atualizar `modified_at = GETUTCDATE()` em toda mutação.
3. Criar tabela `SyncLog` (opcional) para rastrear lotes aceitos.
4. **Com o quê:** script SQL idempotente (`IF NOT EXISTS ... ADD COLUMN`) em
   `DatabaseScripts/`.

### Fase 1 — Endpoints de sync (ASP.NET Core)
1. `GET /api/sync/pull?since={token}` → retorna linhas com `modified_at > since`
   (catálogo + estoque + movimentações). Reaproveita a ideia do `/versao`, mas devolve dados.
2. `POST /api/sync/push` → recebe lote `{ operacoes: [ {tipo, payload, client_uuid, created_at} ] }`;
   aplica transacionalmente (vendas, saídas) e responde `{ aceitos: [client_uuid...] }`.
3. Generalizar `/api/sync/versao` (substitui os `/versao` por tabela).
4. Ajustar CORS em `Program.cs` para aceitar o IP da LAN do celular (ex.: `http://192.168.x.x:5049`).
5. **Com o quê:** novos controllers em `Controllers/Api/`, métodos em `Data/Banco.cs`, testes xUnit.
6. **Critério:** chamadas via Swagger retornam JSON coerente; push de venda cria Pedido + Itens + Pagamento + Movimentacao.

### Fase 2 — Banco embutido no Flutter (Hive)
1. Adicionar dependências: `hive` e `hive_flutter`.
2. Criar schema local em `lib/database/local_database.dart`:
   - `produtos`, `movimentacoes_estoque`, `pedidos`, `itens_pedido`, `pagamentos`
   - `sync_queue (id, tipo, payload, client_uuid, created_at, synced)`
   - `sync_state (chave, valor)` → guarda `last_pull`, `device_id`
3. Seed inicial:
   - **Recomendado:** gerar `assets/infinite_coffee.db` populado e copiar no 1º launch.
   - **Fallback:** no 1º launch com rede, `pull` inicial de `/api/produtos`.
4. **Com o quê:** `hive` + `hive_flutter`; seed inicial por API.
5. **Critério:** app abre offline já com catálogo; `flutter test` valida criação do schema.

### Fase 3 — Motor de sync no app
1. `lib/services/sync_service.dart`:
   - `pull()`: `GET /api/sync/pull?since=last_pull` → upsert no Hive → atualiza `sync_state`.
   - `push()`: lê `sync_queue` com `synced=0` → `POST /api/sync/push` → marca aceitos.
   - Agendamento: `Timer` (a cada 15–30 s) + gatilho de `connectivity_plus` ao voltar a rede.
2. Resolução de conflito: catálogo usa **last-write-wins** por `modified_at`; vendas são
   **append-only** (não conflitam).
3. **Com o quê:** `http`/`dio`, `connectivity_plus`, `Timer`.
4. **Critério:** venda offline vira `synced=1` após reconectar; sem duplicar no servidor.

### Fase 4 — Conectar telas ao banco local
1. Lista de produtos (`home_screen` / estoque) passa a ler do Hive (não mais do reload HTTP).
2. PDV grava venda no Hive + insere em `sync_queue` (`synced=0`).
3. Dashboard lê totais do Hive.
4. Auto-refresh vira **observação local** (Stream/ValueNotifier) em vez de `window.location.reload()`.
5. **Com o quê:** `repository` local já existente + `StreamController`.
6. **Critério:** sem internet dá para vender; ao reconectar, dados sobem e descem.

### Fase 5 — Builds multiplataforma
1. Windows: `flutter build windows` → `build\windows\x64\runner\Release\infinite_coffee_app.exe`
   (já existe; revalidar).
2. Mobile: `flutter build apk` (e `flutter build ios` se houver macOS).
3. Config de URL por ambiente:
   - Windows: `http://localhost:5049`
   - Mobile na LAN: `http://192.168.x.x:5049` (IP da máquina)
   - Usar `flutter --dart-define=API_BASE_URL=...` ou arquivo de config.
4. **Com o quê:** `flutter build`, `dart-define`, CORS liberado.
5. **Critério:** `.exe` e `.apk` instalam e sincronizam com o mesmo servidor.

### Fase 6 — Validação ponta a ponta
1. Venda offline no celular → ficar sem rede → reconectar → aparece no Windows.
2. Saída de estoque no Windows → aparece no celular no próximo pull.
3. Conflito de edição de produto resolvido por `modified_at`.
4. **Com o quê:** testes manuais + `flutter test` + xUnit dos endpoints.
5. **Critério:** dados consistentes entre Windows, mobile e SQL Server.

---

## 4. Riscos e atenção
- **localhost no celular ≠ PC:** usar IP da LAN; ajustar CORS.
- **Tamanho do DB embutido:** manter seed enxuto; catálogo completo vem no 1º sync.
- **Conflitos:** definir regra (last-write-wins) antes de codar o push.
- **Idempotência do push:** usar `client_uuid` para não duplicar venda ao reenviar.
- **Regra de telas:** não alterar HTML/CSS existentes; sync é tudo em arquivos/rotas novas.

---

## 5. Ordem sugerida de execução
Fase 0 → Fase 1 (servidor + API) → Fase 2 (Hive no app) → Fase 3 (motor de sync) →
Fase 4 (telas) → Fase 5 (builds) → Fase 6 (validação).
