using System.Reflection;

namespace SIP.API.Domain.Helpers.ApplicationHelper;

/// <summary>
/// Fornece informações globais sobre a aplicação, como nome e versão.
/// </summary>
public static class ApplicationInfo
{
    /// <summary>
    /// Nome padrão da aplicação.
    /// </summary>
    public static string Name =>
       Assembly.GetExecutingAssembly().GetName().Name ?? "Unknown";

    /// <summary>
    /// Retorna a versão atual da aplicação conforme definida no arquivo de projeto (.csproj).
    /// </summary>
    public static string Version
    {
        get
        {
            var version = Assembly
                .GetExecutingAssembly()
                .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
                .InformationalVersion
                ?? Assembly.GetExecutingAssembly().GetName().Version?.ToString()
                ?? "1.0.0";

            // Remove tudo após o '+', caso exista
            var plusIndex = version.IndexOf('+');
            return plusIndex >= 0 ? version[..plusIndex] : version;
        }
    }
}