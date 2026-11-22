using UniHub.Domain.Entities;

namespace UniHub.Application.DTOs.Denuncias;

// Entrada do PATCH /api/admin/denuncias/{id}/status.
// A API serializa enums como número, então o JSON é { "status": 2 }.
public record AtualizarStatusDenunciaDto(DenunciaStatus Status);
