using System;
using System.ComponentModel.DataAnnotations;

namespace UniHub.Application.DTOs.Denuncias
{
    public class CriarDenunciaDto
    {
        [Required(ErrorMessage = "O ID do alvo (produto ou usuário) é obrigatório.")]
        public Guid AlvoId {get; set;}

        [Required(ErrorMessage = "O tipo de alvo é obrigatório (ex.: Produto, Usuario).")]
        public string TipoAlvo {get; set;}

        [Required(ErrorMessage = "O motivo da denúncia é obrigatório.")]
        [StringLength(500, MinimumLength = 10, ErrorMessage = "O motivo deve ter entre 10 e 500 caracteres.")]
        public string Motivo {get; set;}
    }
}