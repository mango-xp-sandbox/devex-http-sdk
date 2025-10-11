namespace Dotnet.Http.Sdk.Public
{
    public interface ISinchClient
    {
        IContactsApi Contacts { get; }
        IMessagesApi Messages { get; }
    }
}