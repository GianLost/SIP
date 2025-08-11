namespace SIP.UI.Domain.Helpers.Endpoints;

public readonly struct ProtocolsEndpoints
{
    public readonly static string _createProtocol = "sip_api/Protocol/register_protocol";
    public readonly static string _protocolsCounter = "sip_api/Protocol/count";
    public readonly static string _getAllProtocols = "sip_api/Protocol/show";
    public readonly static string _protocolsPaginationFull = "sip_api/Protocol/show_paged?";
    public readonly static string _getProtocolsById = "sip_api/Protocol/";
    public readonly static string _updateProtocol = "sip_api/Protocol/update_protocol/";
    public readonly static string _deleteProtocol = "sip_api/Protocol/delete/";
}