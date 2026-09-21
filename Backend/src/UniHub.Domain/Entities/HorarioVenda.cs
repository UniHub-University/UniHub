using System;
using UniHub.Application.DTOs;

namespace UniHub.Domain.Entities
{
    public class HorarioVenda
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public DiaSemana DiaSemana { get; set; }
        public TimeSpan HoraInicio { get; set; } 
        public TimeSpan HoraFim { get; set; } 

        // Chave Estrangeira
        public Guid VendedorId { get; set; }
        public Vendedor? Vendedor { get; set; }
    }
}