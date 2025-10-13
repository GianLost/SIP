using SIP.API.Domain.Helpers.ApplicationHelper;

namespace SIP.API.Domain.ModelView.Home;

/// <summary>
/// Representa informações gerais sobre a API, incluindo nome, versão e status atual.
/// </summary>
/// <param name="ApiName">Nome da API, obtido automaticamente da configuração da aplicação.</param>
/// <param name="Version">Versão atual da API, obtida automaticamente do assembly.</param>
/// <param name="Status">Status atual da API (ex: Online, Healthy, Degraded).</param>
/// <param name="DocumentationUrl">URL pública para a documentação interativa (Swagger).</param>
/// <param name="Message">Mensagem opcional de status detalhado.</param>
public record Home(
    string ApiName,
    string Version,
    string Status,
    string DocumentationUrl,
    string? Message = null
)
{
    public Home(string Status = "Online", string DocumentationUrl = "", string? Message = null)
        : this(ApplicationInfo.Name, ApplicationInfo.Version, Status, DocumentationUrl, Message) { }
}