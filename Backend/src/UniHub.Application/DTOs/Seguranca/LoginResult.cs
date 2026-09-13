using System;

namespace UniHub.Application.DTOs.Seguranca; 
// DTO de retorno do login: em vez de devolver a entidade Usuario inteira 
// (que arriscaria vazar dados sensíveis e causar loop de serialização 
// caso Usuario tenha uma lista de Pedidos referenciando de volta), 
// escolhemos manualmente só os campos que o front-end precisa. 
public record LoginResult(
    string Token, 
    Guid Id, 
    string NomeCompleto, 
    string EmailInstitucional, 
    string FotoPerfilUrl
);