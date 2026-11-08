using System;
using System.Collections.Generic;

namespace UniHub.Domain.Entities;

public enum ProdutoStatus
{
    Disponivel = 0,
    Esgotado = 1,
    Inativo = 2
}

/// Classe base abstrata para os produtos do sistema. No banco, vira uma
/// unica tabela "Produtos" com uma coluna discriminadora (Table-Per-Hierarchy
/// no EF Core), que separa Alimento de ItemBazar sem precisar de JOIN
/// para listar tudo junto.
///
/// QuantidadeDisponivel eh o campo mais sensivel do sistema: sofre a
/// corrida de dados no pico do intervalo. NUNCA deve ser alterado por um
/// simples "produto.QuantidadeDisponivel--" seguido de SaveChanges no
/// Service -- isso precisa passar por um UPDATE condicional no repositorio
/// (WHERE QuantidadeDisponivel >= @quantidade), garantindo atomicidade
/// mesmo com multiplas instancias da API rodando ao mesmo tempo.
public abstract class Produto
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Nome { get; set; } = string.Empty;

    public string? Descricao { get; set; }

    // URL pública da imagem do produto (1 foto por anúncio), hospedada no
    // Cloudinary via POST /api/upload/imagem?pasta=produtos. 
    // Opcional -> um produto pode não ter imagem
    public string? ImagemUrl { get; set; }

    public decimal Preco { get; set; }

    public int QuantidadeDisponivel { get; set; }

    // chave estrangeira explicita para o Vendedor dono deste produto.
    // Guid para bater com o tipo de Vendedor.Id.
    public Guid VendedorId { get; set; }

    // propriedade de navegacao de volta para o Vendedor
    public Vendedor? Vendedor { get; set; }

    public DateTime DataPublicacao { get; set; } = DateTime.UtcNow;

    public ProdutoStatus Status { get; set; } = ProdutoStatus.Disponivel;

    // lado "muitos" do relacionamento com ItemPedido
    public ICollection<ItemPedido> ItensPedido { get; set; } = new List<ItemPedido>();

    public abstract string GetTipo();
}
