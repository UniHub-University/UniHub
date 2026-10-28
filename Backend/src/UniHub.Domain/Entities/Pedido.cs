using System;
using System.Collections.Generic;

namespace UniHub.Domain.Entities;

public enum PedidoStatus
{
    Reservado = 0,  // estoque decrementado, aguardando retirada/pagamento
    Confirmado = 1,
    Concluido = 2,
    Cancelado = 3,
    Expirado = 4    // reserva que venceu (ExpiraEm passou); estoque devolvido
}

// Distingue uma compra (definitiva) de uma reserva (temporaria). A reserva
// so vale para ItemBazar e expira em ExpiraEm -- a validacao dessa regra
// fica na camada de Service, nao aqui.
public enum TipoPedido
{
    Compra = 0,
    Reserva = 1
}

/// Representa a acao de um usuario comprando/reservando um ou mais
/// produtos. Nao existia no diagrama original -- foi adicionada porque
/// eh o gatilho da logica de concorrencia (ver ItemPedido).
public class Pedido
{
    public Guid Id { get; set; } = Guid.NewGuid();

    // chave estrangeira para o Usuario comprador
    public Guid CompradorId { get; set; }
    public Usuario? Comprador { get; set; }

    public PedidoStatus Status { get; set; } = PedidoStatus.Reservado;

    // Compra (definitiva) ou Reserva (temporaria). Default Compra -- pedidos
    // existentes no banco entram como Compra ao aplicar a migration.
    public TipoPedido Tipo { get; set; } = TipoPedido.Compra;

    // Quando a reserva vence. Nulo para Compra (que nao expira). Setado pelo
    // Service ao criar uma Reserva (now + TTL).
    public DateTime? ExpiraEm { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public ICollection<ItemPedido> Itens { get; set; } = new List<ItemPedido>();
}

/// Cada linha representa "N unidades de um Produto dentro de um Pedido".
/// PrecoUnitarioNaCompra guarda o preco no momento da compra -- importante
/// porque o Produto pode mudar de preco depois, e o historico do pedido
/// nao pode mudar retroativamente.
public class ItemPedido
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid PedidoId { get; set; }
    public Pedido? Pedido { get; set; }

    public Guid ProdutoId { get; set; }
    public Produto? Produto { get; set; }

    public int Quantidade { get; set; }

    public decimal PrecoUnitarioNaCompra { get; set; }
}
