namespace SIP.API.Domain.Helpers.Messages.LogMessage.Success;

public static class LogSuccessMessages
{
    // =============================
    // Mensagens de Log - Success
    // =============================
    public const string Created = "{EntityName} criado com sucesso. ID: {Id}, Nome: {Name}, CriadoEm: {CreatedAt}";
    public const string Updated = "{EntityName} atualizado com sucesso. ID: {Id}";
    public const string Deleted = "{EntityName} deletado com sucesso. ID: {Id}";
    public const string FoundById = "{EntityName} encontrado. ID: {Id}";
    public const string FoundAll = "Consulta concluída. Total de {EntityNamePlural} encontrados: {Total}";
    public const string FoundPaged = "Consulta de {EntityNamePlural} paginados concluída. Retornados: {ReturnedCount}, Total geral: {TotalCount}";
    public const string Counted = "Total de {EntityNamePlural} encontrados: {Total}. SearchString: {@SearchString}";
    public const string PasswordChanged = "A senha para o {EntityName} foi atualizada com sucesso. ID: {Id}";
}