namespace Dotnet.Http.Sdk.Messages
{
    internal static class MessagesMapper
    {
        internal static MessageResponse ToCanonical(this MessageDto dto) =>
            new(
                dto.From,
                dto.To,
                dto.Content,
                dto.Id,
                MapStatus(dto.Status),
                dto.CreatedAt,
                dto.DeliveredAt
            );

        internal static MessagePagedResponse? ToCanonical(this MessagePagedDto dto) =>
            new(
                dto.Messages.ToCanonical(),
                dto.Data != null && dto.Data.Contacts != null ?
                    new MessageContactsExtraInfo(
                        dto.Data.Contacts.Additional1.ToCanonical(),
                        dto.Data.Contacts.Additional2.ToCanonical(),
                        dto.Data.Contacts.Additional3.ToCanonical()
                    ) :
                    null
            );

        private static IReadOnlyList<MessageResponse> ToCanonical(this IEnumerable<MessageDto> dtos) =>
            dtos.Select(ToCanonical).ToList().AsReadOnly();

        private static MessageContactData? ToCanonical(this MessageContactDataDto? dto) =>
            dto != null ?
                new MessageContactData(
                    dto.Name,
                    dto.Phone
                ) :
                null;

        private static MessageStatus MapStatus(string raw) => raw.ToLowerInvariant() switch
        {
            "queued" => MessageStatus.Queued,
            "ack" or "acknowledged" or "delivered" => MessageStatus.Delivered,
            "failed" => MessageStatus.Failed,
            _ => MessageStatus.Failed // defensive default
        };
    }
}