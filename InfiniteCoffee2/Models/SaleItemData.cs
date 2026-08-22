namespace InfiniteCoffee2.Models;

// Dados mínimos de cada item enviados para a transação de venda.
public sealed class SaleItemData
{
    public int ProdutoId { get; init; }
    public int Quantidade { get; init; }
}
