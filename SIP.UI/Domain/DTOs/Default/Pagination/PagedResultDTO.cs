namespace SIP.UI.Domain.DTOs.Default.Pagination;

public class PagedResultDTO<T> where T : class
{
    public ICollection<T>? Items { get; set; } = [];
    public int TotalCount { get; set; }
}