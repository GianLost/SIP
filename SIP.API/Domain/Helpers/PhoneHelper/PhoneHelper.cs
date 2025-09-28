namespace SIP.API.Domain.Helpers.PhoneHelper;

public static class PhoneHelper
{
    /// <summary>
    /// Returns only digits from the given phone string.
    /// </summary>
    public static string ExtractDigits(string? phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
            return string.Empty;

        return new string([.. phone.Where(char.IsDigit)]);
    }
}