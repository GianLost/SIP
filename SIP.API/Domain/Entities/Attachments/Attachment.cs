using SIP.API.Domain.Entities.Protocols;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SIP.API.Domain.Entities.Attachments;

[Table("tbl_attachments")]
public class Attachment
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; } = Guid.NewGuid();

    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public long FileSize { get; set; } = 0;
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; } = null;

    [ForeignKey("tbl_protocols")]
    public Guid ProtocolId { get; set; }
    public Protocol? Protocol { get; set; }
}