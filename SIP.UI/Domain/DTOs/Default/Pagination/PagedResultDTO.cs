using System.Text.Json.Serialization;

namespace SIP.UI.Domain.DTOs.Default.Pagination;

public class PagedResultDTO<T> where T : class
{
    [JsonPropertyName("items")]
    public ICollection<T>? Items { get; set; } = [];

    [JsonPropertyName("totalCount")]
    public int TotalCount { get; set; }
}