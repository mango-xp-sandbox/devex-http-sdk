namespace Dotnet.Http.Sdk.Messages
{
    public sealed record MessageResponse(
        string From,
        string To,
        string Content,
        string Id,
        MessageStatus Status,
        DateTime CreatedAt,
        DateTime? DeliveredAt
    );
}