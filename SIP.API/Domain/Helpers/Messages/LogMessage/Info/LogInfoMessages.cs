namespace SIP.API.Domain.Helpers.Messages.LogMessage.Info;

public static class LogInfoMessages
{
    // =============================
    // Mensagens de Log - Info
    // =============================
    public const string CreateRequest = "Solicitação recebida para criar um novo {EntityName}. Payload: {@Payload}";
    public const string UpdateRequest = "Solicitação recebida para atualizar {EntityName}. ID: {@Id}, Payload: {@Payload}";
    public const string DeleteRequest = "Solicitação recebida para deletar {EntityName}. ID: {@Id}";
    public const string GetByIdRequest = "Solicitação recebida para buscar {EntityName} pelo ID: {@Id}";
    public const string GetAllRequest = "Solicitação recebida para listar todos os {EntityNamePlural}.";
    public const string PaginationRequest = "Solicitação recebida para buscar {EntityNamePlural} paginados. PageNumber: {PageNumber}, PageSize: {PageSize}, SortLabel: {SortLabel}, SortDirection: {SortDirection}, SearchString: {@SearchString}";
    public const string CountRequest = "Solicitação recebida para contar {EntityNamePlural}. SearchString: {@SearchString}";
    public const string DefaultChangePasswordRequest = "Solicitação recebida para realizar o fluxo administrativo de alteração de senha para {EntityName}. ID: {@Id}";
}