namespace SIP.UI.Domain.Helpers.Endpoints;

public readonly struct UsersEndpoints
{
    public readonly static string _createUser = "sip_api/User/register_user";
    public readonly static string _usersCounter = "sip_api/User/count";
    public readonly static string _getAllUsers = "sip_api/User/show";
    public readonly static string _usersPaginationFull = "sip_api/User/show_paged?";
    public readonly static string _getUsersById = "sip_api/User/";
    public readonly static string _defaultUpdateUser = "sip_api/User/default_update_user/";
    public readonly static string _defaultUpdatePassword = "sip_api/User/default-change-password";
    public readonly static string _deleteUser = "sip_api/User/delete/";
}