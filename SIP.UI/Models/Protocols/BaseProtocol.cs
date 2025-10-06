using SIP.UI.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace SIP.UI.Models.Protocols;

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

    [Required(ErrorMessage = "Obrigatório informar quem está gerando o protocolo.")]
    public virtual Guid CreatedById { get; set; }

    [Required(ErrorMessage = "Obrigatório informar o setor de origem do protocolo.")]
    public virtual Guid OriginSectorId { get; set; }

    [Required(ErrorMessage = "Obrigatório informar o setor de destino do protocolo.")]
    public virtual Guid DestinationSectorId { get; set; }

    [Required(ErrorMessage = "Obrigatório informar o usuário de destino do protocolo.")]
    public virtual Guid DestinationUserId { get; set; }
}