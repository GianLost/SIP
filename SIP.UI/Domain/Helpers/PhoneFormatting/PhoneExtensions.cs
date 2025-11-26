namespace SIP.UI.Domain.Helpers.PhoneFormatting;

/// <summary>
/// Métodos de extensão para manipulação dos campos relacionados à números de telefone.
/// </summary>
public static class PhoneExtensions
{
    /// <summary>
    /// Formata uma sequência de números de telefone em um formato legível para humanos.
    /// </summary>
    /// <param name="phoneNumber">Número de telefone para formatar.</param>
    /// <returns>Número de telefone formatado (DDD) 9XXXX-XXXX ou (DDD) XXXX-XXXX.</returns>
    public static string FormatPhoneNumber(this string? phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber)) return string.Empty;
        string digitsOnly = new(phoneNumber.Where(char.IsDigit).ToArray());

        return digitsOnly.Length switch
        {
            11 => $"({digitsOnly[..2]}) {digitsOnly.Substring(2, 5)}-{digitsOnly[7..]}",
            10 => $"({digitsOnly[..2]}) {digitsOnly.Substring(2, 4)}-{digitsOnly[6..]}",
            _ => phoneNumber
        };
    }
}