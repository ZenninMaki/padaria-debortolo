---
name: offline-sync
description: Use when implementing local persistence, offline operations, queues, synchronization, or conflict handling in the mobile app.
---

# Offline e Sincronizacao

1. O SQL Server e a fonte oficial quando houver conexao.
2. Sem internet, salve dados no armazenamento local.
3. Registre cada operacao em uma fila com `clientUuid` unico.
4. Salve entidade, tipo, data, dispositivo, tentativas e erro.
5. Reenvie operacoes quando a conexao voltar.
6. A sincronizacao deve ser idempotente.
7. Nao duplique pedidos, pagamentos ou movimentacoes.
8. Conflitos de estoque devem ser resolvidos pelo servidor.
9. Informe operacoes pendentes e falhas ao usuario.
10. Nunca apague historico local ou remoto automaticamente.
11. O pull deve atualizar o Hive em segundo plano, sem piscar ou bloquear a tela atual.
12. Exclusao de produto deve ser inativacao no servidor, preservando pedidos e auditoria.
