using System;
using System.ComponentModel.DataAnnotations;

namespace UniHub.Application.DTOs.Denuncias
{
    public class CriarDenunciaDto
    {
        [Required(ErrorMessage = "O ID do usuário denunciado é obrigatório.")]
        public Guid DenunciadoId {get; set;}

        public Guid? ProdutoId {get; set;} // opcional, pois a denúnicia pode ser apenas sobre o comportamento do usuário

        [Required(ErrorMessage = "O motivo da denúncia é obrigatório (ex.: Fraude, Assédio, etc.).")]
        public string Motivo {get; set;} = string.Empty;

        [Required(ErrorMessage = "A descrição detalhada da denúncia é obrigatória.")]
        [StringLength(500, MinimumLength = 10, ErrorMessage = "A descrição deve ter entre 10 e 500 caracteres.")]
        public string Descricao { get; set; } = string.Empty;
    }
}