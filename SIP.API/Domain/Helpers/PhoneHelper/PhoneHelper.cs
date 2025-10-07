namespace SIP.API.Domain.Helpers.PhoneHelper;

/// <summary>
/// Fornece métodos utilitários para tratamento e formatação de números de telefone.
/// </summary>
/// <remarks>
/// Esta classe contém funções auxiliares para manipulação de strings de telefone.
/// </remarks>
public static class PhoneHelper
{
    /// <summary>
    /// Extrai apenas os dígitos numéricos de uma string de telefone.
    /// </summary>
    /// <param name="phone">
    /// O número de telefone a ser processado. Pode conter caracteres como espaços, 
    /// parênteses, traços ou símbolos, os quais serão ignorados.
    /// </param>
    /// <returns>
    /// Uma string contendo apenas os dígitos do número de telefone.
    /// Retorna uma string vazia caso o valor fornecido seja nulo, vazio ou composto apenas por espaços.
    /// </returns>
    /// <remarks>
    /// Este método é útil para normalizar números de telefone antes de operações como:
    /// <list type="bullet">
    /// <item><description>Armazenamento no banco de dados;</description></item>
    /// <item><description>Comparação de números em diferentes formatos;</description></item>
    /// <item><description>Envio de dados para APIs que exigem apenas dígitos.</description></item>
    /// </list>
    /// </remarks>
    public static string ExtractDigits(string? phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
            return string.Empty;

        return new string([.. phone.Where(char.IsDigit)]);
    }
}