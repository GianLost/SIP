using System.Security.Claims;
using System.Text.Json;

namespace SIP.UI.Domain.Services.Users.Auth;

public static class JwtParser
{
    public static IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
    {
        string payload = jwt.Split('.')[1];

        byte[] jsonBytes = ParseBase64WithoutPadding(payload);

        var keyValuePairs = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonBytes);

        if (keyValuePairs == null)
            return [];

        return keyValuePairs.SelectMany(kvp => 
            CreateClaims(kvp.Key, kvp.Value));
    }

    public static bool IsJwtExpired(string jwt)
    {
        IEnumerable<Claim> claims = ParseClaimsFromJwt(jwt);

        Claim? expClaim = claims.FirstOrDefault(c => c.Type == "exp");

        if (expClaim == null || !long.TryParse(expClaim.Value, out var secondsSinceEpoch))
            return false;

        DateTime expiration = DateTimeOffset.FromUnixTimeSeconds(secondsSinceEpoch).UtcDateTime;

        return expiration <= DateTime.UtcNow;
    }

    private static byte[] ParseBase64WithoutPadding(string base64)
    {
        int remainder = base64.Length % 4;
        string padded = base64;

        if (remainder == 2)
            padded = base64 + "==";
        else if (remainder == 3)
            padded = base64 + "=";

        return Convert.FromBase64String(padded.Replace('-', '+').Replace('_', '/'));
    }

    private static IEnumerable<Claim> CreateClaims(string key, object value)
    {
        switch (value)
        {
            case JsonElement element when element.ValueKind == JsonValueKind.Array:

                foreach (JsonElement arrayElement in element.EnumerateArray())
                {
                    yield return new Claim(key, arrayElement.GetString() ?? string.Empty);
                }
                break;

            default:

                yield return new Claim(key, value.ToString() ?? string.Empty);
                break;
        }
    }
}