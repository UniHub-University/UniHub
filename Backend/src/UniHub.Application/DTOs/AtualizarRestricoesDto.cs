using System;
using System.Collections.Generic;

namespace UniHub.Application.DTOs
{
    public class AtualizarRestricoesDto
    {
        public List<Guid> RestricoesIds { get; set; } = new();
    }
}