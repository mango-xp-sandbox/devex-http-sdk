namespace Dotnet.Http.Sdk.Messages
{
    public sealed record MessageContactsExtraInfo(
        MessageContactData Additional1,
        MessageContactData Additional2,
        MessageContactData Additional3
    );
}