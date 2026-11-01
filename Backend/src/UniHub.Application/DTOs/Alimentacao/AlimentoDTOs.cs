using System;
using UniHub.Domain.Entities;

namespace UniHub.Application.DTOs.Alimentacao;

public record CriarAlimentoDto(
    string Nome,
    string? Descricao,
    decimal Preco,
    int QuantidadeDisponivel,
    string Categoria,
    string? Restricoes,
    TimeSpan HorarioDisponibilidade
);

public record AtualizarAlimentoDto(
    string Nome,
    string? Descricao,
    decimal Preco,
    int QuantidadeDisponivel,
    string Categoria,
    string? Restricoes,
    TimeSpan HorarioDisponibilidade
);

public record AlimentoResumoDto(
    Guid Id,
    string Nome,
    string? Descricao,
    decimal Preco,
    int QuantidadeDisponivel,
    string Categoria,
    string? Restricoes,
    TimeSpan HorarioDisponibilidade,
    ProdutoStatus Status,
    DateTime DataPublicacao,
    Guid VendedorId
);

public record AlimentoRespostaDto(
    Guid Id,
    string Nome,
    string? Descricao,
    decimal Preco,
    int QuantidadeDisponivel,
    string Categoria,
    string? Restricoes,
    TimeSpan HorarioDisponibilidade,
    ProdutoStatus Status,
    DateTime DataPublicacao,
    Guid VendedorId,
    string? ChavePix
);
