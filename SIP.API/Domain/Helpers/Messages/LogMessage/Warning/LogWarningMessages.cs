namespace SIP.API.Domain.Helpers.Messages.LogMessage.Warning;

public static class LogWarningMessages
{
    // =============================
    // Mensagens de Log - Warning
    // =============================
    public const string InvalidCreate = "Dados inválidos fornecidos ao criar {EntityName}. Payload: {@Payload}";
    public const string InvalidUpdate = "Dados inválidos fornecidos ao atualizar {EntityName}. Payload: {@Payload}";
    public const string NotFound = "Nenhum {EntityName} encontrado para o ID {@Id}";
    public const string Empty = "Nenhum {EntityNamePlural} encontrado na base de dados.";
    public const string EmptyPagination = "Nenhum {EntityNamePlural} encontrado para os parâmetros fornecidos. PageNumber: {PageNumber}, PageSize: {PageSize}, SearchString: {@SearchString}";
    public const string InvalidOperation = "Falha ao deletar {EntityName} devido a restrições de negócio. ID: {@Id}";
}