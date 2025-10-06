using SIP.API.Domain.Enums;
using SIP.API.Domain.Models.Protocols;
using System.ComponentModel.DataAnnotations;

namespace SIP.API.Domain.DTOs.Protocols;

public class ProtocolUpdateDTO : BaseProtocol
{
    public override int Number { get; set; }
    public override string Subject { get; set; } = string.Empty;
    public override string Description { get; set; } = string.Empty;
    public override ProtocolStatus Status { get; set; }
    public override bool IsArchived { get; set; } = false;


    [Required(ErrorMessage = "Obrigatório informar quem está gerando o protocolo.")]
    public Guid CreatedById { get; set; }

    public Guid? UpdatedById { get; set; }

    [Required(ErrorMessage = "Obrigatório informar o setor de origem do protocolo.")]
    public Guid OriginSectorId { get; set; }

    [Required(ErrorMessage = "Obrigatório informar o setor de destino do protocolo.")]
    public Guid DestinationSectorId { get; set; }

    [Required(ErrorMessage = "Obrigatório informar o usuário de destino do protocolo.")]
    public Guid DestinationUserId { get; set; }
}