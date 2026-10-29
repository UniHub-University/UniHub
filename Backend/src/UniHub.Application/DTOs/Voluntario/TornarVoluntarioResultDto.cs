namespace UniHub.Application.DTOs.Voluntario;

// Resultado da operacao de tornar-se voluntario (contrato service -> controller).
// Espelha TornarVendedorResultDto: nunca lanca excecao, o "ja e voluntario" vem
// como Sucesso=false com o VoluntarioId existente.
public record TornarVoluntarioResultDto(bool Sucesso, string Mensagem, Guid? VoluntarioId);
