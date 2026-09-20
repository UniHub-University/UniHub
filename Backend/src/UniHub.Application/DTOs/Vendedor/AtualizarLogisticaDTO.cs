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
        public string DiaSemana { get; set; } = string.Empty;
        public string HoraInicio { get; set; } = string.Empty; // Enviar como string "12:00"
        public string HoraFim { get; set; } = string.Empty;
    }
}