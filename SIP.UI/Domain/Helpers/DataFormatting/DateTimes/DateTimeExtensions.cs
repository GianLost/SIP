namespace SIP.UI.Domain.Helpers.DataFormatting.DateTimes;

public static class DateTimeExtensions
{
    public static string ToBrasiliaTimeFormat(this DateTime dateTime)
    {
        try
        {
            // Tenta obter a ID IANA (mais compatível)
            TimeZoneInfo brasiliaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("America/Sao_Paulo");
            DateTime brasiliaTime = TimeZoneInfo.ConvertTimeFromUtc(dateTime, brasiliaTimeZone);

            return brasiliaTime.ToString("dd/MM/yyyy HH:mm");
        }
        catch (TimeZoneNotFoundException)
        {
            // Fallback para a ID do Windows se a primeira falhar (menos comum, mas seguro)
            try
            {
                TimeZoneInfo brasiliaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time");
                DateTime brasiliaTime = TimeZoneInfo.ConvertTimeFromUtc(dateTime.ToUniversalTime(), brasiliaTimeZone);

                return brasiliaTime.ToString("dd/MM/yyyy HH:mm");
            }
            catch
            {
                // Em caso de falha total, retorna a hora local
                return dateTime.ToLocalTime().ToString("dd/MM/yyyy HH:mm");
            }
        }
    }
}