using System;
using System.Collections.Generic;


namespace UniHub.Domain.Entities
{
    public class RestricaoAlimentar
    {
        public Guid Id {get; set;}
        public string Nome {get; set;} = string. Empty;
        public string? Descricao {get; set;}

        // Propriedade de navegação para o relacionamento N:N com Usuario
        public virtual ICollection<Usuario> Ususarios {get; set;} = new List<Usuario>();
        
    }
}