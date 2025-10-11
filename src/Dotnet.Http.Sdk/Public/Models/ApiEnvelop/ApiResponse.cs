namespace Dotnet.Http.Sdk.Public.Models
{
    public record ApiResponse(ApiMeta Meta);

    public sealed record ApiResponse<T>(
        T Data,
        ApiMeta Meta
    ) : ApiResponse(Meta);
}