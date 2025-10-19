namespace SIP.API.Domain.DTOs.Default.Pagination;

/// <summary>
/// Represents a standardized structure for paginated results returned by API endpoints or service methods.
/// </summary>
/// <typeparam name="T">
/// The type of items contained in the paginated result set.
/// This type must be a reference type (class), typically a DTO representing the entity data.
/// </typeparam>
/// <remarks>
/// This DTO provides a consistent way to return paginated data across all API endpoints,
/// ensuring that clients receive both the data collection and the total number of records
/// that match the query or filter criteria.
/// 
/// It is designed to integrate seamlessly with frontend components that implement
/// pagination, filtering, and sorting features.
/// 
/// <para>Example usage:</para>
/// <code>
/// var pagedUsers = new PagedResultDTO&lt;UserListItemDTO&gt;
/// {
///     Items = new List&lt;UserListItemDTO&gt; 
///     { 
///         new() { Id = Guid.NewGuid(), Name = "Alice" },
///         new() { Id = Guid.NewGuid(), Name = "Bob" }
///     },
///     TotalCount = 42
/// };
/// </code>
/// </remarks>
public class PagedResultDTO<T> where T : class
{
    /// <summary>
    /// Gets or sets the collection of items returned in the current page of results.
    /// </summary>
    /// <value>
    /// A collection of objects of type <typeparamref name="T"/>, representing the data items.
    /// </value>
    /// <remarks>
    /// This property may contain fewer items than the total number of available records,
    /// depending on the page size and filtering applied.
    /// 
    /// If no items match the criteria, this property should contain an empty collection
    /// rather than <see langword="null"/>.
    /// </remarks>
    public ICollection<T>? Items { get; set; } = [];

    /// <summary>
    /// Gets or sets the total number of records that match the current search or filter criteria.
    /// </summary>
    /// <value>
    /// The total count of records in the dataset (not just the current page).
    /// </value>
    /// <remarks>
    /// This property is essential for pagination controls in frontend applications,
    /// allowing clients to calculate the total number of pages or items available.
    /// 
    /// <para>For example:</para>
    /// If <c>TotalCount = 120</c> and the frontend requests <c>pageSize = 20</c>,
    /// there will be exactly 6 available pages.
    /// </remarks>
    public int TotalCount { get; set; }
}