namespace Dotnet.Http.Sdk.Contacts
{
    using Core.Internal;

    internal sealed record ContactPagedDto(
        IReadOnlyList<ContactDto> Contacts,
        int Page,
        int QuantityPerPage) : PagedResponseDto(Page, QuantityPerPage);
}