using System;
using System.Collections.Generic;

namespace UniHub.Domain.Entities;

public class Vendedor
{
    public Guid Id { get; set; } = Guid.NewGuid();

    // chave estrangeira para o Usuario "dono" deste perfil de vendedor.
    // usando Guid para bater com o tipo de Usuario.Id.
    public Guid UsuarioId { get; set; }

    // propriedade de navegacao de volta para o Usuario (relacao 1:0..1)
    public Usuario? Usuario { get; set; }

    // private set, pois no diagrama esses dois campos so devem mudar atraves de ReceberAvaliacao(),
    // assim ninguem de fora da classe consegue "trapacear"
    public decimal NotaMedia { get; private set; } = 0;
    public int QtdAvaliacoes { get; private set; } = 0;

    public ICollection<LocalVenda> Locais { get; set; } = new List<LocalVenda>();
    public ICollection<HorarioVenda> Horarios { get; set; } = new List<HorarioVenda>();

    public List<Produto> ProdutosPublicados { get; set; } = new();

    // Adiciona um novo produto a lista de produtos publicados pelo vendedor.
    // Tambem seta o VendedorId no produto, para o relacionamento ficar
    // explicito dos dois lados (evita depender de shadow property do EF Core).
    public void DivulgarProduto(Produto produto)
    {
        if (produto is null)
        {
            throw new ArgumentNullException(nameof(produto));
        }

        produto.VendedorId = this.Id;
        produto.Vendedor = this;
        ProdutosPublicados.Add(produto);
    }

    /// Registra uma nova avaliacao (nota de 0 a 5) e recalcula
    /// a media (NotaMedia) e o total de avaliacoes (QtdAvaliacoes).
    public void ReceberAvaliacao(int nota)
    {
        if (nota < 0 || nota > 5)
        {
            throw new ArgumentOutOfRangeException(nameof(nota), "A nota deve ser entre 0 e 5.");
        }

        // Recalcula a media mantendo a soma total implicita:
        // soma_atual = media_atual * qtd_atual
        // nova_soma = soma_atual + nota_nova
        // nova_media = nova_soma / (qtd_atual + 1)
        decimal somaAtual = NotaMedia * QtdAvaliacoes;
        QtdAvaliacoes++;
        NotaMedia = (somaAtual + nota) / QtdAvaliacoes;
    }
}
