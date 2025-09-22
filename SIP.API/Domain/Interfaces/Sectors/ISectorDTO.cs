using SIP.API.Domain.Helpers.RegExpression;
using System.ComponentModel.DataAnnotations;

namespace SIP.API.Domain.Interfaces.Sectors;

public interface ISectorDTO
{
    [Required(ErrorMessage = "O nome do setor é obrigatório.")]
    [StringLength(150, MinimumLength = 3, ErrorMessage = "O nome deve ter entre 3 e 150 caracteres.")]
    [RegularExpression(ConstExpressions.NameRegex, ErrorMessage = "O nome deve conter apenas letras e espaços.")]
    public string Name { get; set; }

    [Required(ErrorMessage = "A sigla do setor é obrigatória.")]
    [StringLength(20, MinimumLength = 2, ErrorMessage = "A sigla deve ter entre 2 e 20 caracteres.")]
    [RegularExpression(ConstExpressions.LoginRegex, ErrorMessage = "A sigla deve conter apenas letras, números e os caracteres . _ -")]
    public string Acronym { get; set; }

    [Required(ErrorMessage = "O telefone é obrigatório.")]
    [StringLength(20, MinimumLength = 8, ErrorMessage = "O telefone deve ter entre 8 e 20 caracteres.")]
    [RegularExpression(ConstExpressions.PhoneNumberRegex, ErrorMessage = "O telefone informado não é válido.")]
    public string Phone { get; set; }
}