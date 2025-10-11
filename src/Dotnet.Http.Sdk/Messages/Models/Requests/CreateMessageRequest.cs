namespace Dotnet.Http.Sdk.Messages
{
    public sealed record CreateMessageRequest(string From, string Content, CreateMessageReceiverRequest To);
}