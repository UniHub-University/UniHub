using System;

namespace UniHub.Domain.Entities
{
    public class LocalVenda
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Campus { get; set; } = string.Empty;
        public string PontoDeEncontro { get; set; } = string.Empty;
        
        // Chave Estrangeira
        public Guid VendedorId { get; set; }
        public Vendedor? Vendedor { get; set; }
    }
}