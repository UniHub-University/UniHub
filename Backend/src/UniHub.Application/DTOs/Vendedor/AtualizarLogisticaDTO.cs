using System.Collections.Generic;

namespace UniHub.Application.DTOs
{
    public class AtualizarLogisticaDTO
    {
        public List<LocalVendaDTO> Locais { get; set; } = new List<LocalVendaDTO>();
        public List<HorarioVendaDTO> Horarios { get; set; } = new List<HorarioVendaDTO>();
    }

    public class LocalVendaDTO
    {
        public string Campus { get; set; } = string.Empty;
        public string PontoDeEncontro { get; set; } = string.Empty;
    }

    public class HorarioVendaDTO
    {
        public enum DiaSemana
{
    Domingo = 0, Segunda = 1, Terca = 2, Quarta = 3,
    Quinta = 4, Sexta = 5, Sabado = 6
}
        public string HoraInicio { get; set; } = string.Empty; // Enviar como string "12:00"
        public string HoraFim { get; set; } = string.Empty;
    }
}