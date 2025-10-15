namespace Dotnet.Http.Sdk.Public.Models
{
    public record SinchResponse(SinchMeta Meta);

    public sealed record SinchResponse<T>(
        T Data,
        SinchMeta Meta
    ) : SinchResponse(Meta);
}