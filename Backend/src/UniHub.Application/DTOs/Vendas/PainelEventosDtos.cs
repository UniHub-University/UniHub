using UniHub.Domain.Entities;

namespace UniHub.Application.DTOs.Vendas;

// Payloads dos eventos emitidos pelo Painel do Vendedor em Tempo Real
// (Frente 2 / BRV-185). Serializados em camelCase e enviados pelo SignalR ao
// grupo "vendedor:{id}". Status vai como enum (numero) para bater com o estado
// inicial do painel (MeuProdutoDto.Status) -- o front nao recebe enum como
// string neste projeto (AGENTS Secao 14).

// Disparado apos cada decremento de estoque bem-sucedido num pedido.
// QuantidadeDisponivel ja e a quantidade resultante (pos-decremento).
public record EstoqueAtualizadoDto(
    Guid ProdutoId,
    int QuantidadeDisponivel,
    ProdutoStatus Status);

// Disparado quando um pedido afeta um produto do vendedor. QuandoUtc em UTC
// (o front formata no fuso local). Um pedido com itens de produtos diferentes
// do mesmo vendedor gera um evento por produto afetado (decidido na A4).
public record NovoPedidoDto(
    Guid PedidoId,
    Guid ProdutoId,
    string NomeProduto,
    int Quantidade,
    DateTime QuandoUtc);
