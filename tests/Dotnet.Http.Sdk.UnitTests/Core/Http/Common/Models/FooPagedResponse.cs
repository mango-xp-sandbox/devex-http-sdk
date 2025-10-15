namespace Dotnet.Http.Sdk.UnitTests.Core.Http
{
    using Sdk.Core.Internal;

    internal sealed record FooPagedResponse(int Page, int QuantityPerPage) : PagedResponseDto(Page, QuantityPerPage);
}