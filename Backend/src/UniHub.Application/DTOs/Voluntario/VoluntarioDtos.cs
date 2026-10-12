using UniHub.Domain.Entities;

namespace UniHub.Application.DTOs;

/// Corpo de PUT /api/perfil/voluntario/areas. Substitui a lista inteira.
public record DefinirAreasDto(List<CategoriaApoio> Areas);

/// Perfil de voluntario devolvido ao proprio dono. Nao expoe nada do Usuario.
public record VoluntarioRespostaDto(
    string CodigoPublico,
    List<CategoriaApoio> AreasDeAtuacao,
    bool Disponivel);

public record VoluntarioResultDto(bool Sucesso, string Mensagem, VoluntarioRespostaDto? Voluntario);
