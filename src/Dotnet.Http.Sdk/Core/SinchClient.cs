namespace Dotnet.Http.Sdk.Core
{
    using Public;

    /// <inheritdoc />
    internal class SinchClient(IContactsApi contacts, IMessagesApi messages) : ISinchClient
    {
        /// <inheritdoc />
        public IContactsApi Contacts { get; } = contacts;

        /// <inheritdoc />
        public IMessagesApi Messages { get; } = messages;
    }
}