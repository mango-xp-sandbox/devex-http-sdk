namespace Dotnet.Http.Sdk.Public
{
    public sealed record PaginationOptions(
        int Page = 1,
        int PageSize = 50
    );
}