using Humanizer;

namespace SIP.UI.Domain.Helpers.Endpoints;

public readonly struct BaseEndpoints<T>
{
    private static readonly string entity = typeof(T).Name.Pluralize().ToLower();

    public static readonly string _base = $"sip_api/{entity}";

    public static readonly string _create = _base;
    public static readonly string _count = $"{_base}/count";
    public static readonly string _getAll = _base;
    public static readonly string _getPaged = $"{_base}/paged?";
    public static readonly string _getById = $"{_base}/";
    public static readonly string _update = $"{_base}/";
    public static readonly string _delete = $"{_base}/";
    public static readonly string _password = $"{_base}/password";
}