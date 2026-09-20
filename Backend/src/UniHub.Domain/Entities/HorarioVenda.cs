using System;

namespace UniHub.Domain.Entities
{
    public class HorarioVenda
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string DiaSemana { get; set; } = string.Empty;
        public TimeSpan HoraInicio { get; set; } 
        public TimeSpan HoraFim { get; set; } 

        // Chave Estrangeira
        public Guid VendedorId { get; set; }
        public Vendedor? Vendedor { get; set; }
    }
}