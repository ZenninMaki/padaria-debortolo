using InfiniteCoffee2.Data;
using Microsoft.AspNetCore.Mvc;

namespace InfiniteCoffee2.Controllers
{
    // Esse controller controla o fluxo completo de um novo atendimento.
    // Usamos Session para guardar os dados entre as etapas (clienteId, mesaId, pedidoId).
    public class AtendimentoController : Controller
    {
        // ── ETAPA 1: Escolher ou cadastrar cliente ──────────────────────────
        public IActionResult Index()
        {
            HttpContext.Session.Clear(); // limpa qualquer atendimento anterior
            ViewBag.Clientes = Banco.ListarClientes();
            return View();
        }

        [HttpPost]
        public IActionResult SelecionarCliente(int clienteId)
        {
            HttpContext.Session.SetInt32("clienteId", clienteId);
            return RedirectToAction("EscolherMesa");
        }

        [HttpPost]
        public IActionResult CadastrarCliente(string nome, string email, string telefone)
        {
            Banco.CadastrarCliente(nome, email, telefone);
            // Busca o ID do cliente recém-cadastrado pelo nome
            var clientes = Banco.BuscarCliente(nome);
            var clienteId = Convert.ToInt32(clientes.FirstOrDefault()?["id_cliente"] ?? 0);
            HttpContext.Session.SetInt32("clienteId", clienteId);
            return RedirectToAction("EscolherMesa");
        }

        // ── ETAPA 2: Escolher mesa ──────────────────────────────────────────
        public IActionResult EscolherMesa()
        {
            ViewBag.Mesas = Banco.ListarMesas();
            return View();
        }

        [HttpPost]
        public IActionResult SelecionarMesa(int mesaId)
        {
            HttpContext.Session.SetInt32("mesaId", mesaId);
            Banco.AtualizarStatusMesa(mesaId, "Ocupada");
            return RedirectToAction("EscolherFuncionario");
        }

        // ── ETAPA 3: Escolher funcionário e criar pedido ────────────────────
        public IActionResult EscolherFuncionario()
        {
            ViewBag.Funcionarios = Banco.ListarFuncionarios();
            return View();
        }

        [HttpPost]
        public IActionResult CriarPedido(int funcionarioId)
        {
            var clienteId    = HttpContext.Session.GetInt32("clienteId") ?? 0;
            var mesaId       = HttpContext.Session.GetInt32("mesaId") ?? 0;
            var pedidoId     = Banco.CriarPedido(mesaId, funcionarioId, clienteId);
            HttpContext.Session.SetInt32("pedidoId", pedidoId);
            return RedirectToAction("AdicionarItens");
        }

        // ── ETAPA 4: Adicionar itens ────────────────────────────────────────
        public IActionResult AdicionarItens()
        {
            ViewBag.Produtos = Banco.ListarProdutos();
            ViewBag.PedidoId = HttpContext.Session.GetInt32("pedidoId");
            return View();
        }

        [HttpPost]
        public IActionResult AdicionarItem(int produtoId, int quantidade)
        {
            var pedidoId = HttpContext.Session.GetInt32("pedidoId") ?? 0;
            Banco.AdicionarItemPedido(pedidoId, produtoId, quantidade);
            TempData["Mensagem"] = "Item adicionado!";
            return RedirectToAction("AdicionarItens");
        }

        // ── ETAPA 5: Pagamento e finalização ────────────────────────────────
        public IActionResult Pagamento()
        {
            var pedidoId = HttpContext.Session.GetInt32("pedidoId") ?? 0;
            var total    = Banco.CalcularTotalPedido(pedidoId);
            ViewBag.Total    = total;
            ViewBag.PedidoId = pedidoId;
            return View();
        }

        [HttpPost]
        public IActionResult Finalizar(string forma)
        {
            var pedidoId = HttpContext.Session.GetInt32("pedidoId") ?? 0;
            var mesaId   = HttpContext.Session.GetInt32("mesaId") ?? 0;
            var total    = Banco.CalcularTotalPedido(pedidoId);

            Banco.RegistrarPagamento(pedidoId, forma, total);
            Banco.FinalizarPedido(pedidoId);
            Banco.AtualizarStatusMesa(mesaId, "Disponível");

            HttpContext.Session.Clear();
            TempData["Mensagem"] = $"Pedido #{pedidoId} finalizado com sucesso!";
            return RedirectToAction("Sucesso");
        }

        public IActionResult Sucesso() => View();
    }
}
