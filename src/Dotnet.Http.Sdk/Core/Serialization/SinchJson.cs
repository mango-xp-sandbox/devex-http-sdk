namespace Dotnet.Http.Sdk.Core
{
    using System.Text.Json;
    using System.Text.Json.Serialization;

    internal static class SinchJson
    {
        public static readonly JsonSerializerOptions Options = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            WriteIndented = false
        };
    }
}