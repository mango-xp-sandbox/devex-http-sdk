namespace Dotnet.Http.Sdk.Core
{
    using Public;

    internal static class PaginationBuilderHelper
    {
        internal static string BuildPaginationEndpoint(string endpoint, PaginationOptions opts) => $"{endpoint}?page={opts.Page}&limit={opts.PageSize}";
    }
}