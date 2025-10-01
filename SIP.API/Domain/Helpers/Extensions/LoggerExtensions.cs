namespace SIP.API.Domain.Helpers.Extensions;

/// <summary>
/// Fornece métodos de extensão para a interface <see cref="ILogger"/>,
/// padronizando a geração de logs relacionados a entidades de domínio.
/// Estes métodos injetam automaticamente nomes de entidade no singular e plural
/// nas mensagens de log, reduzindo a repetição e a chance de erros.
/// </summary>
public static class LoggerExtensions
{
    /// <summary>
    /// Deriva o nome de uma entidade no singular e plural a partir de um tipo genérico.
    /// A convenção padrão é remover o sufixo "DTO" do nome do tipo.
    /// </summary>
    /// <typeparam name="T">O tipo da entidade (ex: Sector, SectorDTO).</typeparam>
    /// <returns>Uma tupla contendo o nome no singular (ex: "Sector") e no plural (ex: "Sectors").</returns>
    private static (string entityName, string entityNamePlural) GetEntityNames<T>()
    {
        var entityName = typeof(T).Name.Replace("DTO", string.Empty);
        return (entityName, entityName + "s");
    }

    /// <summary>
    /// Formata a mensagem de log substituindo os placeholders de entidade.
    /// </summary>
    private static string FormatMessage<T>(string message)
    {
        var (entityName, entityNamePlural) = GetEntityNames<T>();

        // Substitui os placeholders na string em vez de alterar a lista de args
        return message
            .Replace("{EntityName}", entityName)
            .Replace("{EntityNamePlural}", entityNamePlural);
    }

    // --- LOG INFORMATION ---

    /// <summary>
    /// Registra uma mensagem no nível <see cref="LogLevel.Information"/> com nomenclatura de entidade padronizada.
    /// </summary>
    /// <typeparam name="T">O tipo da entidade relacionada à mensagem de log. Usado para gerar os valores para <c>{EntityName}</c> e <c>{EntityNamePlural}</c>.</typeparam>
    /// <param name="logger">A instância de <see cref="ILogger"/> a ser estendida.</param>
    /// <param name="message">
    /// O template da mensagem de log. Espera-se que os dois primeiros placeholders sejam <c>{EntityName}</c> e <c>{EntityNamePlural}</c>,
    /// que serão preenchidos automaticamente.
    /// </param>
    /// <param name="args">Um array de objetos com os valores para preencher os placeholders no template da mensagem, a partir do terceiro placeholder.</param>
    /// <example>
    /// Exemplo de uso:
    /// <code>
    /// _logger.LogInformation&lt;Sector&gt;("Buscando todos os {EntityNamePlural}.");
    /// // Resultado: "Buscando todos os Sectors."
    /// </code>
    /// </example>
    public static void LogInformation<T>(
    this ILogger logger,
    string message,
    params object?[] args)
    {
        var formattedMessage = FormatMessage<T>(message);
        logger.LogInformation(formattedMessage, args);
    }

    // --- LOG WARNING ---

    /// <summary>
    /// Registra uma mensagem no nível <see cref="LogLevel.Warning"/> com nomenclatura de entidade padronizada.
    /// </summary>
    /// <typeparam name="T">O tipo da entidade relacionada à mensagem de log. Usado para gerar os valores para <c>{EntityName}</c> e <c>{EntityNamePlural}</c>.</typeparam>
    /// <param name="logger">A instância de <see cref="ILogger"/> a ser estendida.</param>
    /// <param name="message">
    /// O template da mensagem de log. Espera-se que os dois primeiros placeholders sejam <c>{EntityName}</c> e <c>{EntityNamePlural}</c>.
    /// </param>
    /// <param name="args">Um array de objetos com os valores para preencher os placeholders no template da mensagem, a partir do terceiro placeholder.</param>
    /// <example>
    /// Exemplo de uso:
    /// <code>
    /// _logger.LogWarning&lt;Product&gt;("Nenhum {EntityName} encontrado para o ID {ProductId}.", productId);
    /// // Resultado: "Nenhum Product encontrado para o ID 123."
    /// </code>
    /// </example>
    public static void LogWarning<T>(
    this ILogger logger,
    string message,
    params object?[] args)
    {
        var formattedMessage = FormatMessage<T>(message);
        logger.LogWarning(formattedMessage, args);
    }

    /// <summary>
    /// Registra uma exceção e uma mensagem no nível <see cref="LogLevel.Warning"/> com nomenclatura de entidade padronizada.
    /// </summary>
    /// <typeparam name="T">O tipo da entidade relacionada à mensagem de log. Usado para gerar os valores para <c>{EntityName}</c> e <c>{EntityNamePlural}</c>.</typeparam>
    /// <param name="logger">A instância de <see cref="ILogger"/> a ser estendida.</param>
    /// <param name="exception">A exceção a ser registrada.</param>
    /// <param name="message">
    /// O template da mensagem de log. Espera-se que os dois primeiros placeholders sejam <c>{EntityName}</c> e <c>{EntityNamePlural}</c>.
    /// </param>
    /// <param name="args">Um array de objetos com os valores para preencher os placeholders no template da mensagem, a partir do terceiro placeholder.</param>
    public static void LogWarning<T>(
    this ILogger logger,
    Exception? exception,
    string message,
    params object?[] args)
    {
        var formattedMessage = FormatMessage<T>(message);
        logger.LogWarning(exception, formattedMessage, args);
    }

    // --- LOG ERROR ---

    /// <summary>
    /// Registra uma exceção e uma mensagem no nível <see cref="LogLevel.Error"/> com nomenclatura de entidade padronizada.
    /// </summary>
    /// <typeparam name="T">O tipo da entidade relacionada à mensagem de log. Usado para gerar os valores para <c>{EntityName}</c> e <c>{EntityNamePlural}</c>.</typeparam>
    /// <param name="logger">A instância de <see cref="ILogger"/> a ser estendida.</param>
    /// <param name="exception">A exceção a ser registrada.</param>
    /// <param name="message">
    /// O template da mensagem de log. Espera-se que os dois primeiros placeholders sejam <c>{EntityName}</c> e <c>{EntityNamePlural}</c>.
    /// </param>
    /// <param name="args">Um array de objetos com os valores para preencher os placeholders no template da mensagem, a partir do terceiro placeholder.</param>
    /// <example>
    /// Exemplo de uso:
    /// <code>
    /// _logger.LogError&lt;UserDTO&gt;(ex, "Falha inesperada ao tentar criar {EntityName}. Payload: {@Payload}", userPayload);
    /// // Resultado: "Falha inesperada ao tentar criar User. Payload: { Name = 'John', ... }"
    /// </code>
    /// </example>
    public static void LogError<T>(
    this ILogger logger,
    Exception? exception,
    string message,
    params object?[] args)
    {
        var formattedMessage = FormatMessage<T>(message);
        logger.LogError(exception, formattedMessage, args);
    }
}