namespace Dotnet.Http.Sdk.Messages
{
    public sealed record MessagePagedResponse(
        IEnumerable<MessageResponse> Messages,
        MessageContactsExtraInfo ContactsInfo
    );
}