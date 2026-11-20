namespace UniHub.Application.Interfaces.Vendas;

// Consultas de leitura sobre o perfil de Vendedor, usadas fora do fluxo de
// escrita do VendedorService. Hoje serve ao PainelHub (SignalR), que precisa
// resolver o Vendedor dono da conexao a partir do usuario autenticado para
// agrupar a conexao em "vendedor:{id}".
public interface IVendedorConsultaRepository
{
    // Retorna o Id do perfil de Vendedor do usuario informado, ou null se o
    // usuario nao for vendedor. Consulta somente leitura.
    Task<Guid?> ObterVendedorIdPorUsuarioAsync(Guid usuarioId);

    // Inverso do método acima: dado o Id do perfil de Vendedor, retorna o UsuarioId
    // dono, ou null se o vendedor não existir. Usado pela denúncia — o front só tem o
    // VendedorId (o UsuarioId não é exposto por LGPD), e o backend faz a tradução.
    Task<Guid?> ObterUsuarioIdPorVendedorAsync(Guid vendedorId);

}
