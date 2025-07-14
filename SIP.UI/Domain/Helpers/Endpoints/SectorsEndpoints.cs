namespace SIP.UI.Domain.Helpers.Endpoints;

public readonly struct SectorsEndpoints
{
    public readonly static string _createSector = "sip_api/Sector/register_sector";
    public readonly static string _sectorsCounter = "sip_api/Sector/count";
    public readonly static string _sectorsPagination = "sip_api/Sector/show?";
    public readonly static string _getSectorsById = "sip_api/Sector/";
    public readonly static string _updateSector = "sip_api/Sector/update_sector/";
    public readonly static string _deleteSector = "sip_api/Sector/delete/";
}