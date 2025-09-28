using SIP.UI.Domain.Helpers.RegExpressions;
using System.ComponentModel.DataAnnotations;

namespace SIP.UI.Models.Sectors;

public class BaseSector
{
    [Required(ErrorMessage = "O nome do setor é obrigatório.")]
    [StringLength(150, MinimumLength = 3, ErrorMessage = "O nome deve ter entre 3 e 150 caracteres.")]
    [RegularExpression(ConstExpressions.NameRegex, ErrorMessage = "O nome deve conter apenas letras e espaços.")]
    public virtual string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "A sigla do setor é obrigatória.")]
    [StringLength(20, MinimumLength = 2, ErrorMessage = "A sigla deve ter entre 2 e 20 caracteres.")]
    [RegularExpression(ConstExpressions.AbridgementRegex, ErrorMessage = "A sigla deve conter apenas letras, números e os caracteres . _ -")]
    public virtual string Acronym { get; set; } = string.Empty;

    [Required(ErrorMessage = "O telefone é obrigatório.")]
    [StringLength(20, MinimumLength = 8, ErrorMessage = "O telefone deve ter entre 8 e 20 caracteres.")]
    [RegularExpression(ConstExpressions.PhoneNumberRegex, ErrorMessage = "O telefone informado não é válido.")]
    public virtual string Phone { get; set; } = string.Empty;
}