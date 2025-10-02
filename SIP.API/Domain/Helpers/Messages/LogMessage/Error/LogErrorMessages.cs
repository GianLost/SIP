namespace SIP.API.Domain.Helpers.Messages.LogMessage.Error;

public class LogErrorMessages
{
    // =============================
    // Mensagens de Log - Error
    // =============================
    public const string CreateError = "Erro inesperado ao criar {EntityName}. Payload: {@Payload}";
    public const string UpdateError = "Erro inesperado ao atualizar {EntityName}. ID: {@Id}, Payload: {@Payload}";
    public const string DeleteError = "Erro inesperado ao deletar {EntityName}. ID: {@Id}";
    public const string GetByIdError = "Erro inesperado ao buscar {EntityName} pelo ID {@Id}";
    public const string GetAllError = "Erro inesperado ao buscar todos os {EntityNamePlural}";
    public const string PaginationError = "Erro inesperado ao buscar {EntityNamePlural} paginados. PageNumber: {PageNumber}, PageSize: {PageSize}, SearchString: {@SearchString}";
    public const string CountError = "Erro inesperado ao contar {EntityNamePlural}. SearchString: {@SearchString}";
}