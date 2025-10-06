namespace SIP.API.Domain.ModelView.Home;

public record Home(string ApiName = "SIP_API", string Version = "1.0", string Status = "Online", string DocumentationUrl = "")
{
    public string Message => $"Bem vindo à {ApiName} v{Version}. A API está sendo executada com sucesso.";
}