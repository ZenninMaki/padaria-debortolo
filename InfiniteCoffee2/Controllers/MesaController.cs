using InfiniteCoffee2.Data;
using Microsoft.AspNetCore.Mvc;

namespace InfiniteCoffee2.Controllers
{
    public class MesaController : Controller
    {
        public IActionResult Index()
        {
            var mesas = Banco.ListarMesas();
            return View(mesas);
        }

        [HttpPost]
        public IActionResult AlterarStatus(int id, string status)
        {
            Banco.AtualizarStatusMesa(id, status);
            TempData["Mensagem"] = "Status da mesa atualizado!";
            return RedirectToAction("Index");
        }
    }
}
