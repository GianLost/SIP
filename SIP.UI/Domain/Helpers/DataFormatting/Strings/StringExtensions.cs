namespace SIP.UI.Domain.Helpers.DataFormatting.Strings;

/// <summary>
/// Métodos de extensão para manipulação de strings.
/// </summary>
public static class StringExtensions
{
    /// <summary>
    /// Converte uma string para o formato de maiúsculas.
    /// Retorna uma string vazia se o valor for nulo ou em branco.
    /// </summary>
    /// <param name="text">A string a ser convertida.</param>
    /// <returns>A string em maiúsculas ou uma string vazia.</returns>
    public static string ToUppercase(this string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return string.Empty;

        return text.ToUpper();
    }
}