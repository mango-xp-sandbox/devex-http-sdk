namespace Dotnet.Http.Sdk.Messages
{
    using Core.Internal;

    internal sealed record MessagePagedDto(
        IReadOnlyList<MessageDto> Messages,
        MessageExtraDataDto? Data,
        int Page,
        int QuantityPerPage) : PagedResponseDto(Page, QuantityPerPage);
}