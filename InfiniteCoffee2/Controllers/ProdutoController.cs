using InfiniteCoffee2.Data;
using Microsoft.AspNetCore.Mvc;

namespace InfiniteCoffee2.Controllers
{
    public class ProdutoController : Controller
    {
        public IActionResult Index()
        {
            var produtos = Banco.ListarProdutos();
            return View(produtos);
        }

        public IActionResult Cadastrar() => View();

        [HttpPost]
        public IActionResult Cadastrar(string nome, decimal preco, string tipo)
        {
            Banco.CadastrarProduto(nome, preco, tipo);
            TempData["Mensagem"] = "Produto cadastrado com sucesso!";
            return RedirectToAction("Index");
        }

        public IActionResult Editar(int id)
        {
            var produtos = Banco.ListarProdutos();
            var produto = produtos.FirstOrDefault(p => p["id_produto"].ToString() == id.ToString());
            if (produto == null) return NotFound();
            return View(produto);
        }

        [HttpPost]
        public IActionResult Editar(int id, string nome, decimal preco, string tipo)
        {
            Banco.AtualizarProduto(id, nome, preco, tipo);
            TempData["Mensagem"] = "Produto atualizado com sucesso!";
            return RedirectToAction("Index");
        }

        public IActionResult Excluir(int id)
        {
            Banco.ExcluirProduto(id);
            TempData["Mensagem"] = "Produto excluído.";
            return RedirectToAction("Index");
        }
    }
}
