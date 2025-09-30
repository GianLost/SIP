namespace SIP.API.Domain.Helpers.Extensions;

public static class LoggerExtensions
{
    public static void LogEntityInfo<T>(
    this ILogger logger,
    string messageTemplate,
    T payload,
    object? additionalData = null)
    {
        var entityName = typeof(T).Name.Replace("DTO", string.Empty);
        var entityNamePlural = entityName + "s"; 

        var message = messageTemplate
            .Replace("{EntityNamePlural}", entityNamePlural)
            .Replace("{EntityName}", entityName);

        if (additionalData is not null)
            logger.LogInformation(message, payload, additionalData);
        else
            logger.LogInformation(message, payload);
    }
}