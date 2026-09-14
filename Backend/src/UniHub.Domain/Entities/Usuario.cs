using System;

namespace UniHub.Domain.Entities;

public enum UserRole
{
    Aluno = 0,
    Voluntario = 1,
    Admin = 1
}

/// Representa um usuario do sistema.
/// Um usuario pode, opcionalmente, se tornar um Vendedor (relacao 1:0..1).
public class Usuario
{
    // identificador unico do usuario
    public Guid Id { get; set; } = Guid.NewGuid();

    // dados pessoais
    public string NomeCompleto { get; set; } = string.Empty;

    public string EmailInstitucional { get; set; } = string.Empty;

    public string Telefone { get; set; } = string.Empty;

    public string FotoPerfilUrl { get; set; } = string.Empty;

    // caso sigamos com o Id usado para login/autenticacao via Google
    public string GoogleId { get; set; } = string.Empty;

    // papel do usuario no sistema. "Vendedor" nao entra aqui de proposito:
    // ser vendedor eh representado pela existencia (ou nao) de Usuario.Vendedor,
    // nao por um valor de enum -- assim nao ha risco de os dois ficarem
    // inconsistentes entre si (ex: Role = Vendedor mas Vendedor == null).
    public UserRole Role { get; set; } = UserRole.Aluno;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;

    // PARA VENDEDOR
    // propriedade de Navegacao (1:0..1) -> este usuario pode ter no maximo um perfil de Vendedor associado
    public Vendedor? Vendedor { get; set; }

    // PARA VOLUNTARIO
    // propriedade de Navegacao (1:0..1) -> este usuario pode ter no maximo um perfil de Voluntario associado
    public Voluntario? Voluntario { get; set; }

    // pedidos feitos por este usuario como comprador
    public ICollection<Pedido> Pedidos { get; set; } = new List<Pedido>();

    // solicitacoes de apoio feitas por este usuario (modulo Acao Solidaria)
    public ICollection<SolicitacaoApoio> Solicitacoes { get; set; } = new List<SolicitacaoApoio>();

    /// Atualiza os dados basicos de cadastro do usuario (nome, telefone e foto).
    /// O email institucional e o GoogleId nao sao alterados aqui de proposito,
    /// pois normalmente vem do provedor de login/autenticacao
    public void AtualizarDados(string nome, string telefone, string foto)
    {
        NomeCompleto = nome;
        Telefone = telefone;
        FotoPerfilUrl = foto;
    }

    /// Cria e associa um novo perfil de Vendedor a tal usuario.
    /// Lanca excecao caso o usuario ja possua um perfil de vendedor,
    /// ja que a relacao Usuario -> Vendedor eh 1:0..1 (no maximo um vendedor por usuario, pelo menos por enquanto).
    public Vendedor TornarSeVendedor()
    {
        if (Vendedor != null)
        {
            throw new InvalidOperationException("Este usuario ja possui um perfil de vendedor ativo.");
        }

        Vendedor = new Vendedor
        {
            UsuarioId = this.Id,
            Usuario = this
        };

        return Vendedor;
    }
    public Voluntario TornarSeVoluntario()
    {
        if (Voluntario != null)
        {
            throw new InvalidOperationException("Este usuario ja possui um perfil de voluntario ativo.");
        }

        Voluntario = new Voluntario
        {
            UsuarioId = this.Id,
            Usuario = this
        };

        return Voluntario;
    }
}
