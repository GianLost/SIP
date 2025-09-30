namespace SIP.UI.Domain.Helpers.Endpoints;

public readonly struct SectorsEndpoints
{
    public const string _base = "sip_api/sectors";

    public const string _create = _base;
    public const string _count = $"{_base}/count";
    public const string _getAll = _base;
    public const string _getPaged = $"{_base}/paged?";
    public const string _getById = $"{_base}/";
    public const string _update = $"{_base}/";
    public const string _delete = $"{_base}/";
}