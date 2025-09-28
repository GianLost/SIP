namespace SIP.API.Domain.Helpers.RegExpressionHelper;

public readonly struct ConstExpressions
{
    public const string NameRegex = @"^[A-Za-zÀ-ú\s]+$";

    public const string AbridgementRegex = @"^[a-zA-Z0-9_.-]+$";

    public const string StrongPasswordRegex = "^(?=.*[a-z])(?=.*[A-Z])(?=.*\\d)(?=.*[^a-zA-Z\\d]).{8,255}$";

    public const string PhoneNumberRegex = @"^[0-9()\s-]+$";

    public const string PhoneNumberResponseRegex = @"^\(\d{2}\)\s(?:9\d{4}-\d{4}|\d{4}-\d{4})$";
}