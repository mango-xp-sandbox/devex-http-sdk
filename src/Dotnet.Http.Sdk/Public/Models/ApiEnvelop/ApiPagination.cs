namespace Dotnet.Http.Sdk.Public
{
    public sealed record ApiPagination(
        int Page,
        int PageSize,
        long? Total = null,
        bool? HasNext = null
    );
}