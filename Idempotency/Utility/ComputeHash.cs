
namespace Idempotency.Utility;

public static class ComputeHash
{
    public static string ComputeHashForRequest(object body)
    {
        if (body is null) return string.Empty;

        var json = JsonSerializer.Serialize(body, _options);

        byte[] inputBytes = Encoding.UTF8.GetBytes(json);
        byte[] hashBytes = SHA256.HashData(inputBytes); 

        return Convert.ToBase64String(hashBytes);
    }

    private static readonly JsonSerializerOptions _options = new()
    {
        //Ensure properties are always in the same order
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

}
