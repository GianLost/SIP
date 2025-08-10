namespace SIP.UI.Domain.Helpers.Endpoints;

public readonly struct CacheEndpoints
{
    public readonly static string _invalidateSectorCount = "sip_api/cache/invalidate?entityType=Sector";
    public readonly static string _invalidateUserCount = "sip_api/cache/invalidate?entityType=User";
}