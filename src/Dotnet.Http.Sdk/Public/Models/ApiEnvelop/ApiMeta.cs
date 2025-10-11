namespace Dotnet.Http.Sdk.Public
{
    public sealed record ApiMeta(
        string RequestId,
        string TimestampUtc,
        ApiPagination? Pagination = null,
        IReadOnlyList<string>? Warnings = null
    );
}