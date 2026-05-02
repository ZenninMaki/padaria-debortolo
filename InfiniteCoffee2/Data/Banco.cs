using System.Data;
using Microsoft.Data.SqlClient;

namespace InfiniteCoffee2.Data
{
    public class Banco
    {
        private static string connectionString =
            "Server=DESKTOP-4APN96G;Database=infiniteCoffee;Trusted_Connection=True;TrustServerCertificate=True;";

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
                var cmd = new SqlCommand("sp_ListarProdutos", conn) { CommandType = CommandType.StoredProcedure };
                var reader = cmd.ExecuteReader();
                while (reader.Read())
                    lista.Add(new Dictionary<string, object>
                    {
                        ["id_produto"] = reader["id_produto"],
                        ["nome_produto"] = reader["nome_produto"],
                        ["preco"] = reader["preco"],
                        ["tipo"] = reader["tipo"]
                    });
            }
            return lista;
        }

        public static void CadastrarProduto(string nome, decimal preco, string tipo)
        {
            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                var cmd = new SqlCommand("sp_CadastrarProduto", conn) { CommandType = CommandType.StoredProcedure };
                cmd.Parameters.AddWithValue("@nome", nome);
                cmd.Parameters.AddWithValue("@preco", preco);
                cmd.Parameters.AddWithValue("@tipo", tipo);
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
