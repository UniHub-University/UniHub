using System;

namespace UniHub.Domain.Entities
{
    public class HorarioVenda
    {
        public Guid Id { get; set; } = Guid.NewGuid();
       public enum DiaSemana
{
    Domingo = 0, Segunda = 1, Terca = 2, Quarta = 3,
    Quinta = 4, Sexta = 5, Sabado = 6
}
        public TimeSpan HoraInicio { get; set; } 
        public TimeSpan HoraFim { get; set; } 

        // Chave Estrangeira
        public Guid VendedorId { get; set; }
        public Vendedor? Vendedor { get; set; }
    }
}