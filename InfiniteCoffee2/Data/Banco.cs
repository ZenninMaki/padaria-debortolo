using System.Data;
using Microsoft.Data.SqlClient;

namespace InfiniteCoffee2.Data
{
    public class Banco
    {
        private static string connectionString =
            "Server=localhost\\KAIO;Database=infiniteCoffee;Trusted_Connection=True;TrustServerCertificate=True;";

        // =========================
        // CLIENTES
        // =========================

        public static List<Dictionary<string, object>> ListarClientes()
        {
            var lista = new List<Dictionary<string, object>>();
            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                var cmd = new SqlCommand("sp_ListarClientes", conn) { CommandType = CommandType.StoredProcedure };
                var reader = cmd.ExecuteReader();
                while (reader.Read())
                    lista.Add(new Dictionary<string, object>
                    {
                        ["id_cliente"] = reader["id_cliente"],
                        ["nome_cliente"] = reader["nome_cliente"],
                        ["email"] = reader["email"],
                        ["telefone"] = reader["telefone"]
                    });
            }
            return lista;
        }

        public static List<Dictionary<string, object>> BuscarCliente(string valor)
        {
            var lista = new List<Dictionary<string, object>>();
            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                var cmd = new SqlCommand("sp_BuscarCliente", conn) { CommandType = CommandType.StoredProcedure };
                cmd.Parameters.AddWithValue("@valor", valor);
                var reader = cmd.ExecuteReader();
                while (reader.Read())
                    lista.Add(new Dictionary<string, object>
                    {
                        ["id_cliente"] = reader["id_cliente"],
                        ["nome_cliente"] = reader["nome_cliente"],
                        ["email"] = reader["email"],
                        ["telefone"] = reader["telefone"]
                    });
            }
            return lista;
        }

        public static void CadastrarCliente(string nome, string email, string telefone)
        {
            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                var cmd = new SqlCommand("sp_CadastrarCliente", conn) { CommandType = CommandType.StoredProcedure };
                cmd.Parameters.AddWithValue("@nome", nome);
                cmd.Parameters.AddWithValue("@email", email);
                cmd.Parameters.AddWithValue("@telefone", telefone);
                cmd.ExecuteNonQuery();
            }
        }

        public static void AtualizarCliente(int id, string nome, string email, string telefone)
        {
            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                var cmd = new SqlCommand("sp_AtualizarCliente", conn) { CommandType = CommandType.StoredProcedure };
                cmd.Parameters.AddWithValue("@id", id);
                cmd.Parameters.AddWithValue("@nome", nome);
                cmd.Parameters.AddWithValue("@email", email);
                cmd.Parameters.AddWithValue("@telefone", telefone);
                cmd.ExecuteNonQuery();
            }
        }

        public static void ExcluirCliente(int id)
        {
            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                var cmd = new SqlCommand("sp_ExcluirCliente", conn) { CommandType = CommandType.StoredProcedure };
                cmd.Parameters.AddWithValue("@id", id);
                cmd.ExecuteNonQuery();
            }
        }

        // =========================
        // PRODUTOS
        // =========================

        public static List<Dictionary<string, object>> ListarProdutos()
        {
            var lista = new List<Dictionary<string, object>>();
            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                // Consulta direta mantém a tela compatível mesmo antes das procedures opcionais.
                var cmd = new SqlCommand("SELECT id_produto, nome_produto, preco, tipo, quantidade_estoque, codigo_barras, descricao FROM Produtos ORDER BY nome_produto", conn);
                var reader = cmd.ExecuteReader();
                while (reader.Read())
                    lista.Add(new Dictionary<string, object>
                    {
                        ["id_produto"] = reader["id_produto"],
                        ["nome_produto"] = reader["nome_produto"],
                        ["preco"] = reader["preco"],
                        ["tipo"] = reader["tipo"],
                        ["quantidade_estoque"] = reader["quantidade_estoque"],
                        ["codigo_barras"] = reader["codigo_barras"],
                        ["descricao"] = reader["descricao"]
                    });
            }
            return lista;
        }

        public static void CadastrarProduto(string nome, decimal preco, string tipo, int quantidade, string codigoBarras, string descricao)
        {
            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                var cmd = new SqlCommand("INSERT INTO Produtos (nome_produto, preco, tipo, quantidade_estoque, codigo_barras, descricao) VALUES (@nome, @preco, @tipo, @quantidade, @codigoBarras, @descricao)", conn);
                cmd.Parameters.AddWithValue("@nome", nome);
                cmd.Parameters.AddWithValue("@preco", preco);
                cmd.Parameters.AddWithValue("@tipo", tipo);
                cmd.Parameters.AddWithValue("@quantidade", quantidade);
                cmd.Parameters.AddWithValue("@codigoBarras", string.IsNullOrWhiteSpace(codigoBarras) ? DBNull.Value : codigoBarras.Trim());
                cmd.Parameters.AddWithValue("@descricao", string.IsNullOrWhiteSpace(descricao) ? DBNull.Value : descricao.Trim());
                cmd.ExecuteNonQuery();
            }
        }

        public static void AtualizarProduto(int id, string nome, decimal preco, string tipo)
        {
            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                var cmd = new SqlCommand("sp_AtualizarProduto", conn) { CommandType = CommandType.StoredProcedure };
                cmd.Parameters.AddWithValue("@id", id);
                cmd.Parameters.AddWithValue("@nome", nome);
                cmd.Parameters.AddWithValue("@preco", preco);
                cmd.Parameters.AddWithValue("@tipo", tipo);
                cmd.ExecuteNonQuery();
            }
        }

        public static void ExcluirProduto(int id)
        {
            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                var cmd = new SqlCommand("sp_ExcluirProduto", conn) { CommandType = CommandType.StoredProcedure };
                cmd.Parameters.AddWithValue("@id", id);
                cmd.ExecuteNonQuery();
            }
        }

        // =========================
        // ESTOQUE
        // =========================

        public static List<Dictionary<string, object>> ListarEstoque(string busca = "")
        {
            GarantirTabelaMovimentacoes();
            var lista = new List<Dictionary<string, object>>();
            using var conn = new SqlConnection(connectionString);
            conn.Open();
            using var cmd = new SqlCommand("SELECT id_produto, nome_produto, preco, tipo, quantidade_estoque, codigo_barras, descricao FROM Produtos WHERE @busca = '' OR nome_produto LIKE '%' + @busca + '%' OR codigo_barras LIKE '%' + @busca + '%' ORDER BY nome_produto", conn);
            cmd.Parameters.AddWithValue("@busca", (busca ?? string.Empty).Trim());
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                lista.Add(new Dictionary<string, object>
                {
                    ["id_produto"] = reader["id_produto"], ["nome_produto"] = reader["nome_produto"], ["preco"] = reader["preco"],
                    ["tipo"] = reader["tipo"], ["quantidade_estoque"] = reader["quantidade_estoque"],
                    ["codigo_barras"] = reader["codigo_barras"], ["descricao"] = reader["descricao"]
                });
            return lista;
        }

        public static List<Dictionary<string, object>> ListarEstoqueBaixo(int limite = 5)
        {
            return ListarEstoque().Where(item => Convert.ToInt32(item["quantidade_estoque"]) <= limite).ToList();
        }

        public static List<Dictionary<string, object>> ListarMovimentacoesEstoque()
        {
            GarantirTabelaMovimentacoes();
            var lista = new List<Dictionary<string, object>>();
            using var conn = new SqlConnection(connectionString);
            conn.Open();
            using var cmd = new SqlCommand("SELECT m.id_movimentacao, p.nome_produto, m.tipo_movimentacao, m.quantidade, m.motivo, m.data_movimentacao FROM MovimentacoesEstoque m INNER JOIN Produtos p ON p.id_produto = m.produtoid ORDER BY m.id_movimentacao DESC", conn);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                lista.Add(new Dictionary<string, object>
                {
                    ["id_movimentacao"] = reader["id_movimentacao"], ["nome_produto"] = reader["nome_produto"], ["tipo_movimentacao"] = reader["tipo_movimentacao"],
                    ["quantidade"] = reader["quantidade"], ["motivo"] = reader["motivo"], ["data_movimentacao"] = reader["data_movimentacao"]
                });
            return lista;
        }

        public static bool RegistrarSaidaEstoque(int produtoId, int quantidade, string motivo)
        {
            GarantirTabelaMovimentacoes();
            using var conn = new SqlConnection(connectionString);
            conn.Open();
            using var transaction = conn.BeginTransaction();
            using var cmd = new SqlCommand("UPDATE Produtos SET quantidade_estoque = quantidade_estoque - @quantidade WHERE id_produto = @produtoId AND quantidade_estoque >= @quantidade; IF @@ROWCOUNT = 1 INSERT INTO MovimentacoesEstoque (produtoid, tipo_movimentacao, quantidade, motivo, data_movimentacao) VALUES (@produtoId, 'Saida', @quantidade, @motivo, GETDATE());", conn, transaction);
            cmd.Parameters.AddWithValue("@produtoId", produtoId);
            cmd.Parameters.AddWithValue("@quantidade", quantidade);
            cmd.Parameters.AddWithValue("@motivo", motivo.Trim());
            var affected = cmd.ExecuteNonQuery();
            if (affected == 0) return false;
            transaction.Commit();
            return true;
        }

        private static void GarantirTabelaMovimentacoes()
        {
            using var conn = new SqlConnection(connectionString);
            conn.Open();
            using var cmd = new SqlCommand("IF OBJECT_ID(N'dbo.MovimentacoesEstoque', N'U') IS NULL CREATE TABLE MovimentacoesEstoque (id_movimentacao INT IDENTITY(1,1) PRIMARY KEY, produtoid INT NOT NULL, tipo_movimentacao VARCHAR(20) NOT NULL, quantidade INT NOT NULL, motivo VARCHAR(200) NOT NULL, data_movimentacao DATETIME NOT NULL, CONSTRAINT FK_Movimentacoes_Produtos FOREIGN KEY (produtoid) REFERENCES Produtos(id_produto));", conn);
            cmd.ExecuteNonQuery();
        }

        // =========================
        // MESAS
        // =========================

        public static List<Dictionary<string, object>> ListarMesas()
        {
            var lista = new List<Dictionary<string, object>>();
            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                var cmd = new SqlCommand("sp_ListarMesas", conn) { CommandType = CommandType.StoredProcedure };
                var reader = cmd.ExecuteReader();
                while (reader.Read())
                    lista.Add(new Dictionary<string, object>
                    {
                        ["id_mesa"] = reader["id_mesa"],
                        ["numero"] = reader["numero"],
                        ["capacidade"] = reader["capacidade"],
                        ["status_mesa"] = reader["status_mesa"]
                    });
            }
            return lista;
        }

        public static void AtualizarStatusMesa(int id, string status)
        {
            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                var cmd = new SqlCommand("sp_AtualizarStatusMesa", conn) { CommandType = CommandType.StoredProcedure };
                cmd.Parameters.AddWithValue("@id", id);
                cmd.Parameters.AddWithValue("@status", status);
                cmd.ExecuteNonQuery();
            }
        }

        // =========================
        // PEDIDOS
        // =========================

        public static int CriarPedido(int mesaId, int funcionarioId, int clienteId)
        {
            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                var cmd = new SqlCommand("sp_CriarPedido", conn) { CommandType = CommandType.StoredProcedure };
                cmd.Parameters.AddWithValue("@mesaId", mesaId);
                cmd.Parameters.AddWithValue("@funcionarioId", funcionarioId);
                cmd.Parameters.AddWithValue("@clienteId", clienteId);
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        public static void AdicionarItemPedido(int pedidoId, int produtoId, int quantidade)
        {
            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                var cmd = new SqlCommand("sp_AdicionarItemPedido", conn) { CommandType = CommandType.StoredProcedure };
                cmd.Parameters.AddWithValue("@pedidoId", pedidoId);
                cmd.Parameters.AddWithValue("@produtoId", produtoId);
                cmd.Parameters.AddWithValue("@quantidade", quantidade);
                cmd.ExecuteNonQuery();
            }
        }

        public static decimal CalcularTotalPedido(int pedidoId)
        {
            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                var cmd = new SqlCommand("sp_CalcularTotalPedido", conn) { CommandType = CommandType.StoredProcedure };
                cmd.Parameters.AddWithValue("@pedidoId", pedidoId);
                return Convert.ToDecimal(cmd.ExecuteScalar());
            }
        }

        public static void RegistrarPagamento(int pedidoId, string forma, decimal valor)
        {
            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                var cmd = new SqlCommand("sp_RegistrarPagamento", conn) { CommandType = CommandType.StoredProcedure };
                cmd.Parameters.AddWithValue("@pedidoId", pedidoId);
                cmd.Parameters.AddWithValue("@forma", forma);
                cmd.Parameters.AddWithValue("@valor", valor);
                cmd.ExecuteNonQuery();
            }
        }

        public static void FinalizarPedido(int pedidoId)
        {
            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                var cmd = new SqlCommand("sp_FinalizarPedido", conn) { CommandType = CommandType.StoredProcedure };
                cmd.Parameters.AddWithValue("@pedidoId", pedidoId);
                cmd.ExecuteNonQuery();
            }
        }

        public static List<Dictionary<string, object>> ListarPedidosAbertos()
        {
            var lista = new List<Dictionary<string, object>>();
            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                var cmd = new SqlCommand("sp_ListarPedidosAbertos", conn) { CommandType = CommandType.StoredProcedure };
                var reader = cmd.ExecuteReader();
                while (reader.Read())
                    lista.Add(new Dictionary<string, object>
                    {
                        ["id_pedido"] = reader["id_pedido"],
                        ["mesaid"] = reader["mesaid"],
                        ["status_pedido"] = reader["status_pedido"]
                    });
            }
            return lista;
        }

        // =========================
        // FUNCIONÁRIOS
        // =========================

        public static List<Dictionary<string, object>> ListarFuncionarios()
        {
            var lista = new List<Dictionary<string, object>>();
            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                var cmd = new SqlCommand("sp_ListarFuncionarios", conn) { CommandType = CommandType.StoredProcedure };
                var reader = cmd.ExecuteReader();
                while (reader.Read())
                    lista.Add(new Dictionary<string, object>
                    {
                        ["id_funcionario"] = reader["id_funcionario"],
                        ["nome_funcionario"] = reader["nome_funcionario"],
                        ["cargo"] = reader["cargo"]
                    });
            }
            return lista;
        }

        // =========================
        // RELATÓRIOS
        // =========================

        public static List<Dictionary<string, object>> PedidosDoDia()
        {
            var lista = new List<Dictionary<string, object>>();
            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                var cmd = new SqlCommand("sp_PedidosDoDia", conn) { CommandType = CommandType.StoredProcedure };
                var reader = cmd.ExecuteReader();
                while (reader.Read())
                    lista.Add(new Dictionary<string, object>
                    {
                        ["id_pedido"] = reader["id_pedido"],
                        ["datahora"] = reader["datahora"]
                    });
            }
            return lista;
        }

        public static List<Dictionary<string, object>> ProdutosMaisVendidos()
        {
            var lista = new List<Dictionary<string, object>>();
            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                var cmd = new SqlCommand("sp_ProdutosMaisVendidos", conn) { CommandType = CommandType.StoredProcedure };
                var reader = cmd.ExecuteReader();
                while (reader.Read())
                    lista.Add(new Dictionary<string, object>
                    {
                        ["nome_produto"] = reader["nome_produto"],
                        ["total_vendido"] = reader["total_vendido"]
                    });
            }
            return lista;
        }

        public static List<Dictionary<string, object>> HistoricoCliente(int clienteId)
        {
            var lista = new List<Dictionary<string, object>>();
            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                var cmd = new SqlCommand("sp_HistoricoCliente", conn) { CommandType = CommandType.StoredProcedure };
                cmd.Parameters.AddWithValue("@clienteId", clienteId);
                var reader = cmd.ExecuteReader();
                while (reader.Read())
                    lista.Add(new Dictionary<string, object>
                    {
                        ["id_pedido"] = reader["id_pedido"],
                        ["status_pedido"] = reader["status_pedido"],
                        ["datahora"] = reader["datahora"]
                    });
            }
            return lista;
        }

        public static decimal Faturamento()
        {
            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                var cmd = new SqlCommand("sp_Faturamento", conn) { CommandType = CommandType.StoredProcedure };
                return Convert.ToDecimal(cmd.ExecuteScalar());
            }
        }
    }
}
