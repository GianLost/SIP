using SIP.API.Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SIP.API.Domain.Models.Protocols;

/// <summary>
/// Provides base properties for protocol data transfer objects (DTOs).
/// </summary>
public class BaseProtocol
{
    [Required]
    [Column(TypeName = "int")]
    public int Number { get; set; }

    [Required]
    [StringLength(200)]
    [Column(TypeName = "varchar(200)")]
    public string Subject { get; set; } = string.Empty;

    [Required]
    [Column(TypeName = "mediumtext")]
    public string Description { get; set; } = string.Empty;

    [Required]
    [Column(TypeName = "varchar(50)")]
    public ProtocolStatus Status { get; set; }

    [Required]
    [Column(TypeName = "tinyint(1)")]
    public bool IsArchived { get; set; } = false;
}