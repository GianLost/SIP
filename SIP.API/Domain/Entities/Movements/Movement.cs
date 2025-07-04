using SIP.API.Domain.Entities.Protocols;
using SIP.API.Domain.Entities.Sectors;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SIP.API.Domain.Entities.Movements;

[Table("tbl_movements")]
public class Movement
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; } = Guid.NewGuid();

    [ForeignKey(nameof(Protocols.Protocol))]
    public Guid ProtocolId { get; set; }
    public Protocol? Protocol { get; set; }

    public Guid FromSectorId { get; set; }
    public Sector? FromSector { get; set; }

    public Guid ToSectorId { get; set; }
    public Sector? ToSector { get; set; }

    public string Comment { get; set; } = string.Empty;
    public DateTime Date { get; set; } = DateTime.UtcNow;
}