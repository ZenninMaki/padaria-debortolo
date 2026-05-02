using InfiniteCoffee2.Data;
using Microsoft.AspNetCore.Mvc;

namespace InfiniteCoffee2.Controllers
{
    public class PedidoController : Controller
    {
        public IActionResult Index()
        {
            var pedidos = Banco.ListarPedidosAbertos();
            return View(pedidos);
        }

        [HttpPost]
        public IActionResult Fechar(int pedidoId, int mesaId)
        {
            Banco.FinalizarPedido(pedidoId);
            Banco.AtualizarStatusMesa(mesaId, "Disponível");
            TempData["Mensagem"] = "Pedido finalizado e mesa liberada!";
            return RedirectToAction("Index");
        }
    }
}
