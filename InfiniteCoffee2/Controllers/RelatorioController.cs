using InfiniteCoffee2.Data;
using Microsoft.AspNetCore.Mvc;

namespace InfiniteCoffee2.Controllers
{
    public class RelatorioController : Controller
    {
        public IActionResult Index() => View();

        public IActionResult PedidosDoDia()
        {
            var pedidos = Banco.PedidosDoDia();
            return View(pedidos);
        }

        public IActionResult MaisVendidos()
        {
            var produtos = Banco.ProdutosMaisVendidos();
            return View(produtos);
        }

        public IActionResult Historico(int? clienteId)
        {
            ViewBag.Clientes = Banco.ListarClientes();
            if (clienteId.HasValue)
            {
                ViewBag.ClienteId = clienteId.Value;
                var historico = Banco.HistoricoCliente(clienteId.Value);
                return View(historico);
            }
            return View(new List<Dictionary<string, object>>());
        }

        public IActionResult Faturamento()
        {
            var total = Banco.Faturamento();
            ViewBag.Total = total;
            return View();
        }
    }
}
