namespace Dotnet.Http.Sdk.Public
{
    public sealed record SinchMeta(
        string RequestId,
        string ReceivedAtUtc,
        SinchPagination? Pagination = null,
        IReadOnlyList<string>? Warnings = null
    );
}