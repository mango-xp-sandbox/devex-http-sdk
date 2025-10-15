namespace Dotnet.Http.Sdk.Messages
{
    internal sealed record MessageDto(
        string From,
        string To,
        string Content,
        string Id,
        string Status,
        DateTime CreatedAt,
        DateTime? DeliveredAt
    );
}