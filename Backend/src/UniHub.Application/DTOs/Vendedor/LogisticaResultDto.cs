namespace UniHub.Application.DTOs
{
    public class LogisticaResultDto
    {
        public bool Sucesso { get; set; }
        public string Mensagem { get; set; }

        public LogisticaResultDto(bool sucesso, string mensagem)
        {
            Sucesso = sucesso;
            Mensagem = mensagem;
        }
    }
}