using InfiniteCoffee2.Data;
using Microsoft.AspNetCore.Mvc;

namespace InfiniteCoffee2.Controllers.Api;

[ApiController]
[Route("api/produtos")]
public sealed class ProdutosApiController : ControllerBase
{
    /// <summary>Lista os produtos cadastrados.</summary>
    [HttpGet]
    public IActionResult Listar() => Ok(Banco.ListarProdutos());

    /// <summary>Cadastra um produto com seu estoque inicial.</summary>
    [HttpPost]
    public IActionResult Cadastrar([FromBody] CriarProdutoRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Nome) || request.Nome.Length > 100 || request.Preco <= 0 || request.Quantidade < 0 || request.Tipo.Length > 50 || request.Descricao?.Length > 500 || request.CodigoBarras?.Length > 50)
            return BadRequest(new { mensagem = "Informe dados válidos para o produto." });

        Banco.CadastrarProduto(request.Nome.Trim(), request.Preco, request.Tipo.Trim(), request.Quantidade, request.CodigoBarras ?? string.Empty, request.Descricao ?? string.Empty);
        return StatusCode(StatusCodes.Status201Created, new { mensagem = "Produto cadastrado com sucesso." });
    }
}

public sealed class CriarProdutoRequest
{
    public string Nome { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public string? CodigoBarras { get; set; }
    public string Tipo { get; set; } = "Produto";
    public decimal Preco { get; set; }
    public int Quantidade { get; set; }
}
