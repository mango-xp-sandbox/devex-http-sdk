namespace Dotnet.Http.Sdk.Public
{
    public sealed record SinchPagination(
        int Page,
        int PageSize,
        long? Total = null,
        bool? HasNext = null
    );
}