using InfiniteCoffee2.Data;
using Microsoft.AspNetCore.Mvc;

namespace InfiniteCoffee2.Controllers;

// Tela adicional de controle de saidas e alerta de estoque baixo.
public sealed class EstoqueController : Controller
{
    public IActionResult Index(string? busca)
    {
        ViewBag.Busca = busca ?? string.Empty;
        ViewBag.Alertas = Banco.ListarEstoqueBaixo();
        ViewBag.Movimentacoes = Banco.ListarMovimentacoesEstoque();
        return View(Banco.ListarEstoque(busca ?? string.Empty));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Saida(int produtoId, int quantidade, string motivo)
    {
        if (quantidade < 1 || string.IsNullOrWhiteSpace(motivo) || motivo.Length > 200)
        {
            TempData["Mensagem"] = "Informe uma quantidade válida e o motivo da saída.";
            return RedirectToAction(nameof(Index));
        }

        TempData["Mensagem"] = Banco.RegistrarSaidaEstoque(produtoId, quantidade, motivo)
            ? "Saída registrada e estoque atualizado."
            : "Não foi possível registrar a saída. Verifique o produto e o saldo disponível.";
        return RedirectToAction(nameof(Index));
    }
}
