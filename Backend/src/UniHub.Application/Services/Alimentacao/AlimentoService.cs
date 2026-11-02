using UniHub.Application.DTOs.Alimentacao;
using UniHub.Application.Interfaces.Alimentacao;
using UniHub.Domain.Entities;

namespace UniHub.Application.Services.Alimentacao;

public class AlimentoService
{
    private readonly IAlimentoRepository _alimentoRepository;

    public AlimentoService(IAlimentoRepository alimentoRepository)
    {
        _alimentoRepository = alimentoRepository;
    }

    public async Task<IReadOnlyList<AlimentoResumoDto>> ObterDisponiveisAsync()
    {
        var alimentos = await _alimentoRepository.ObterDisponiveisAsync();

        return alimentos
            .Select(MapearParaResumoDto)
            .ToList();
    }

    public async Task<AlimentoRespostaDto?> ObterPorIdAsync(Guid id)
    {
        var alimento = await _alimentoRepository
            .ObterPorIdComVendedorAsync(id);

        if (alimento is null ||
            alimento.Status == ProdutoStatus.Inativo)
        {
            return null;
        }

        return MapearParaRespostaDto(alimento);
    }

    public async Task<AlimentoRespostaDto> CriarAsync(
        Guid usuarioId,
        CriarAlimentoDto dto)
    {
        ValidarDados(dto.Nome, dto.Preco, dto.QuantidadeDisponivel,
            dto.Categoria, dto.HorarioDisponibilidade);

        var vendedorId = await _alimentoRepository
            .ObterVendedorIdPorUsuarioAsync(usuarioId);

        if (vendedorId is null)
        {
            throw new InvalidOperationException(
                "Usuário precisa ter um perfil de vendedor para publicar alimentos.");
        }

        var alimento = new Alimento
        {
            Nome = dto.Nome.Trim(),
            Descricao = dto.Descricao?.Trim(),
            Preco = dto.Preco,
            QuantidadeDisponivel = dto.QuantidadeDisponivel,
            Categoria = dto.Categoria.Trim(),
            Restricoes = dto.Restricoes?.Trim(),
            HorarioDisponibilidade = dto.HorarioDisponibilidade,
            VendedorId = vendedorId.Value,
            DataPublicacao = DateTime.UtcNow,
            Status = DefinirStatus(dto.QuantidadeDisponivel)
        };

        await _alimentoRepository.AdicionarAsync(alimento);

        var alimentoSalvo = await _alimentoRepository
            .ObterPorIdComVendedorAsync(alimento.Id);

        if (alimentoSalvo is null)
        {
            throw new InvalidOperationException(
                "Não foi possível recuperar o alimento criado.");
        }

        return MapearParaRespostaDto(alimentoSalvo);
    }

    public async Task<bool> AtualizarAsync(
        Guid usuarioId,
        Guid alimentoId,
        AtualizarAlimentoDto dto)
    {
        ValidarDados(dto.Nome, dto.Preco, dto.QuantidadeDisponivel,
            dto.Categoria, dto.HorarioDisponibilidade);

        var vendedorId = await _alimentoRepository
            .ObterVendedorIdPorUsuarioAsync(usuarioId);

        if (vendedorId is null)
        {
            return false;
        }

        var alimento = await _alimentoRepository
            .ObterDoVendedorAsync(alimentoId, vendedorId.Value);

        if (alimento is null ||
            alimento.Status == ProdutoStatus.Inativo)
        {
            return false;
        }

        alimento.Nome = dto.Nome.Trim();
        alimento.Descricao = dto.Descricao?.Trim();
        alimento.Preco = dto.Preco;
        alimento.QuantidadeDisponivel = dto.QuantidadeDisponivel;
        alimento.Categoria = dto.Categoria.Trim();
        alimento.Restricoes = dto.Restricoes?.Trim();
        alimento.HorarioDisponibilidade = dto.HorarioDisponibilidade;
        alimento.Status = DefinirStatus(dto.QuantidadeDisponivel);

        await _alimentoRepository.AtualizarAsync(alimento);

        return true;
    }

    public async Task<bool> InativarAsync(
        Guid usuarioId,
        Guid alimentoId)
    {
        var vendedorId = await _alimentoRepository
            .ObterVendedorIdPorUsuarioAsync(usuarioId);

        if (vendedorId is null)
        {
            return false;
        }

        var alimento = await _alimentoRepository
            .ObterDoVendedorAsync(alimentoId, vendedorId.Value);

        if (alimento is null)
        {
            return false;
        }

        return await _alimentoRepository.InativarAsync(alimentoId);
    }

    public async Task<bool> ReativarAsync(
        Guid usuarioId,
        Guid alimentoId)
    {
        var vendedorId = await _alimentoRepository
            .ObterVendedorIdPorUsuarioAsync(usuarioId);

        if (vendedorId is null)
        {
            return false;
        }

        var alimento = await _alimentoRepository
            .ObterDoVendedorAsync(alimentoId, vendedorId.Value);

        if (alimento is null ||
            alimento.QuantidadeDisponivel <= 0)
        {
            return false;
        }

        return await _alimentoRepository.ReativarAsync(alimentoId);
    }

    private static void ValidarDados(
        string nome,
        decimal preco,
        int quantidadeDisponivel,
        string categoria,
        TimeSpan horarioDisponibilidade)
    {
        if (string.IsNullOrWhiteSpace(nome))
        {
            throw new ArgumentException(
                "O nome do alimento é obrigatório.", nameof(nome));
        }

        if (string.IsNullOrWhiteSpace(categoria))
        {
            throw new ArgumentException(
                "A categoria do alimento é obrigatória.", nameof(categoria));
        }

        if (preco < 0)
        {
            throw new ArgumentException(
                "O preço não pode ser negativo.", nameof(preco));
        }

        if (quantidadeDisponivel < 0)
        {
            throw new ArgumentException(
                "A quantidade disponível não pode ser negativa.",
                nameof(quantidadeDisponivel));
        }

        if (horarioDisponibilidade < TimeSpan.Zero ||
            horarioDisponibilidade >= TimeSpan.FromDays(1))
        {
            throw new ArgumentException(
                "O horário de disponibilidade deve estar entre 00:00 e 23:59.",
                nameof(horarioDisponibilidade));
        }
    }

    private static ProdutoStatus DefinirStatus(int quantidadeDisponivel)
    {
        return quantidadeDisponivel > 0
            ? ProdutoStatus.Disponivel
            : ProdutoStatus.Esgotado;
    }

    private static AlimentoResumoDto MapearParaResumoDto(
        Alimento alimento)
    {
        return new AlimentoResumoDto(
            alimento.Id,
            alimento.Nome,
            alimento.Descricao,
            alimento.Preco,
            alimento.QuantidadeDisponivel,
            alimento.Categoria,
            alimento.Restricoes,
            alimento.HorarioDisponibilidade,
            alimento.Status,
            alimento.DataPublicacao,
            alimento.VendedorId);
    }

    private static AlimentoRespostaDto MapearParaRespostaDto(
        Alimento alimento)
    {
        return new AlimentoRespostaDto(
            alimento.Id,
            alimento.Nome,
            alimento.Descricao,
            alimento.Preco,
            alimento.QuantidadeDisponivel,
            alimento.Categoria,
            alimento.Restricoes,
            alimento.HorarioDisponibilidade,
            alimento.Status,
            alimento.DataPublicacao,
            alimento.VendedorId,
            alimento.Vendedor?.ChavePix);
    }
}
