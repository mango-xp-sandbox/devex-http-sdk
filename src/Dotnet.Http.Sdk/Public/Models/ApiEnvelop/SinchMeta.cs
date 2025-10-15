namespace Dotnet.Http.Sdk.Public
{
    public sealed record SinchMeta(
        string RequestId,
        string TimestampUtc,
        SinchPagination? Pagination = null,
        IReadOnlyList<string>? Warnings = null
    );
}