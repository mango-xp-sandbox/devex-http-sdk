namespace Dotnet.Http.Sdk.Public
{
    public sealed record PaginationOptions(
        int Page = 0,
        int PageSize = 50
    );
}