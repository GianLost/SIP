using SIP.API.Domain.Enums;
using SIP.API.Domain.Helpers.StatusHelper;
using System.ComponentModel.DataAnnotations;

namespace SIP.API.Domain.Models.Protocols;

/// <summary>
/// Provides base properties for protocol data transfer objects (DTOs).
/// </summary>
public abstract class BaseProtocol
{
    [Required(ErrorMessage = "O número do protocolo é obrigatório.")]
    [Range(0, int.MaxValue, ErrorMessage = "O número do protocolo deve ser um valor positivo.")]
    public virtual int Number { get; set; }

    [Required(ErrorMessage = "O assunto é obrigatório.")]
    [StringLength(200, MinimumLength = 3, ErrorMessage = "O assunto deve ter entre 3 e 200 caracteres.")]
    public virtual string Subject { get; set; } = string.Empty;

    [Required(ErrorMessage = "A descrição é obrigatória.")]
    [StringLength(2000, ErrorMessage = "A descrição deve ter no máximo 2000 caracteres.")]
    public virtual string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "O status do protocolo é obrigatório.")]
    public virtual ProtocolStatus Status { get; set; }

    public virtual bool IsArchived { get; set; } = false;
}